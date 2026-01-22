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

        //[BindProperty(SupportsGet = true)]
        //public DateTime? SelectedMonth { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SelectedMonth { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; }


        [BindProperty]
        public List<Dictionary<string, object>>? PivotData { get; set; }

        public List<OfficeSelectItem> OfficeList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            OfficeList = await GetUserOfficesAsync();

            if (SelectedMonth == 0)
                SelectedMonth = DateTime.UtcNow.Month;

            if (SelectedYear==0)
                SelectedYear = DateTime.UtcNow.Year;

            return Page();
        }

        private (DateTime UtcStart, DateTime UtcEnd) GetUtcRange()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

            var selectedDate = new DateTime(SelectedYear, SelectedMonth, 1);

            DateTime localStart = new DateTime(SelectedYear, SelectedMonth, 1);
            DateTime localEnd = localStart.AddMonths(1);

            return (
                TimeZoneInfo.ConvertTimeToUtc(localStart, tz),
                TimeZoneInfo.ConvertTimeToUtc(localEnd, tz)
            );
        }

        public async Task<IActionResult> OnPostShowAsync()
        {
            var (utcStart, utcEnd) = GetUtcRange();
            var report = await _reportService.GetOrderPivotReport(utcStart, utcEnd, OfficeId);
            PivotData = report.Data;

            OfficeList = await GetUserOfficesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var (utcStart, utcEnd) = GetUtcRange();
            var data = await _reportService.GetOrderPivotReport(utcStart, utcEnd, OfficeId);
            var excelBytes = _reportService.ExportOrderPivotToExcel(data.Data, utcStart, utcEnd, OfficeId, data.MainProductDict);

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Monthly_Sales_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }
    }

}
