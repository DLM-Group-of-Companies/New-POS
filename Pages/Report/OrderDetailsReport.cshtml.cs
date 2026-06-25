using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Report
{
    public class OrderDetailsReportModel : BasePageModel
    {
        private readonly MainProductSalesReportService _reportService;

        public OrderDetailsReportModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            MainProductSalesReportService reportService)
            : base(context, userManager)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public int? OfficeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TimeZoneId { get; set; } = "Asia/Manila";

        [BindProperty(SupportsGet = true)]
        public int SelectedMonth { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<OfficeSelectItem> OfficeList { get; set; }
        public List<Order>? Orders { get; set; }
        public int? SelectedOrderId { get; set; }
        public Order? SelectedOrder { get; set; }
        public decimal TotalSales { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            OfficeList = await GetUserOfficesAsync();

            if (SelectedMonth == 0)
                SelectedMonth = DateTime.UtcNow.Month;

            if (SelectedYear == 0)
                SelectedYear = DateTime.UtcNow.Year;

            if (!OfficeId.HasValue)
            {
                Orders = new List<Order>();
                TotalSales = 0;
                SelectedOrder = null;
                return Page();
            }

            await LoadOrdersAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Products)
                .Include(o => o.Customers)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return Partial("_OrderDetailsPartial", order);
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            var (utcStart, utcEnd) = GetUtcRange();
            var data = await _reportService.GetOrderPivotReport(utcStart, utcEnd, OfficeId);
            var excelBytes = _reportService.ExportOrderPivotToExcel(data.Data, utcStart, utcEnd, OfficeId, data.MainProductDict);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Order_Details_Report_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }

        private async Task LoadOrdersAsync()
        {
            if (!OfficeId.HasValue)
            {
                Orders = new List<Order>();
                TotalSales = 0;
                SelectedOrder = null;
                return;
            }

            var (utcStart, utcEnd) = GetUtcRange();

            var ordersQuery = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Products)
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Where(o => o.OrderDate >= utcStart && o.OrderDate <= utcEnd && !o.IsVoided)
                .Where(o => o.OfficeId == OfficeId.Value);

            Orders = await ordersQuery.OrderByDescending(o => o.OrderDate).ToListAsync();
            TotalSales = Orders.Sum(o => o.TotAmount);

            if (SelectedOrderId.HasValue)
            {
                SelectedOrder = Orders.FirstOrDefault(o => o.Id == SelectedOrderId.Value);
            }
            else if (Orders.Any())
            {
                SelectedOrder = Orders.First();
            }
        }

        private (DateTime UtcStart, DateTime UtcEnd) GetUtcRange()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            var localStart = new DateTime(SelectedYear, SelectedMonth, 1);
            var localEnd = localStart.AddMonths(1);
            return (
                TimeZoneInfo.ConvertTimeToUtc(localStart, tz),
                TimeZoneInfo.ConvertTimeToUtc(localEnd, tz)
            );
        }
    }
}
