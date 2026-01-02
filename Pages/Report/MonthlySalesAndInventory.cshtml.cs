using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Models.ViewModels;
using NLI_POS.Services;
using static NLI_POS.Services.BasePageModel;

namespace NLI_POS.Pages.Report
{
    public class MonthlySalesAndInventoryModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public MonthlySalesAndInventoryModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string Office { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        public List<Order> Order { get; set; }

        public List<OfficeSelectItem> OfficeList { get; set; }
        public OrderSummaryViewModel OrderSummary { get; set; }

        [BindProperty(SupportsGet = true)]
        public string timeZone { get; set; }

        public int? OfficeId { get; set; }
        public DateTime? SelectedDate { get; set; }

        public List<PivotedOrderViewModel> PivotedOrders { get; set; } = new();


        public List<MonthlySalesRowVM> ReportRows { get; set; } = new();
        public List<Product> MainProducts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? Office, string timeZone, DateTime? Date)
        {
            OfficeList = await GetUserOfficesAsync();

            if (!Date.HasValue)
            {
                Date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            }

            if (!Office.HasValue)
            {
                return Page();
            }

            timeZone ??= OfficeList.FirstOrDefault(o => o.Value == Office?.ToString())?.TimeZone;
            if (string.IsNullOrEmpty(timeZone))
            {
                return Page();
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            DateTime localStart = new DateTime(Date.Value.Year, Date.Value.Month, 1);
            DateTime localEnd = localStart.AddMonths(1);

            DateTime utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
            DateTime utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

            // Get distinct main products for columns
            MainProducts = await _context.Products
                .Where(p => p.ProductClass == "Main" && p.ProductCategory=="Regular")
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            // Query orders
            var orders = await _context.Orders
                .Where(o => !o.IsVoided && o.OfficeId == Office && o.OrderDate >= utcStart && o.OrderDate < utcEnd)
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems).ThenInclude(o => o.Product)
                .Include(o => o.Payments)
                .ToListAsync();

            // Build ReportRows
            ReportRows = orders.Select(o => new MonthlySalesRowVM
            {
                OrderNo = o.OrderNo,
                OrderDate = o.OrderDate,
                CustomerCode = o.Customers?.CustCode,
                CustomerName = $"{o.Customers?.FirstName} {o.Customers?.LastName}",
                OrderType = o.OrderType,
                OfficeName = o.Office?.Name,
                TotalAmount = o.ProductItems.Sum(pi => pi.Price * pi.Quantity),
                ProductItems = o.ProductItems.Select(pi => new OrderProductItemVM
                {
                    ProductCat = pi.ProductCat,
                    ProductName = pi.ProductName,
                    ComboName = pi.ComboName,
                    Price = pi.Price,
                    Quantity = pi.Quantity
                }).ToList(),
                MainProductCounts = o.ProductItems
                    .Where(pi => pi.Product.ProductClass == "Main")
                    .GroupBy(pi => pi.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(pi => pi.Quantity))
            }).ToList();

            return Page();
        }



    }
}

