using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Report
{
    public class MonthlySalesNewModel : BasePageModel
    {
        private readonly MainProductSalesReportService _reportService;

        public MonthlySalesNewModel(
            MainProductSalesReportService reportService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
            : base(context, userManager)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public int? OfficeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TimeZoneId { get; set; } = "Asia/Manila";

        [BindProperty(SupportsGet = true)]
        public DateTime? SelectedMonth { get; set; }

        [BindProperty]
        public List<Dictionary<string, object>>? PivotData { get; set; }

        public List<OfficeSelectItem> OfficeList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            OfficeList = await GetUserOfficesAsync();

            if (SelectedMonth == null)
                SelectedMonth = DateTime.Today;

            return Page();
        }

        private (DateTime UtcStart, DateTime UtcEnd) GetUtcRange()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

            DateTime localStart = new DateTime(SelectedMonth.Value.Year, SelectedMonth.Value.Month, 1);
            DateTime localEnd = localStart.AddMonths(1);

            return (
                TimeZoneInfo.ConvertTimeToUtc(localStart, tz),
                TimeZoneInfo.ConvertTimeToUtc(localEnd, tz)
            );
        }

        public async Task<IActionResult> OnPostShowAsync()
        {
            var (utcStart, utcEnd) = GetUtcRange();
            PivotData = await _reportService.GetOrderPivotReport(utcStart, utcEnd, OfficeId);

            OfficeList = await GetUserOfficesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var (utcStart, utcEnd) = GetUtcRange();
            var data = await _reportService.GetOrderPivotReport(utcStart, utcEnd, OfficeId);
            var excelBytes = _reportService.ExportOrderPivotToExcel(data);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Monthly_Sales_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }
    }

}
