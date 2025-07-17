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
    public class MonthySalesModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public MonthySalesModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
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

        public async Task<IActionResult> OnGetAsync(int? Office, string timeZone, DateTime? Date)
        {
            OfficeList = await GetUserOfficesAsync();
            Order = new List<Order>();

            OfficeId = Office;
            SelectedDate = Date;

            if (!Date.HasValue)
            {
                Date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            }

            if (!Office.HasValue)
            {
                return Page(); // Date will now be set before this point
            }

            // fallback to first office's timezone if none provided
            timeZone ??= OfficeList.FirstOrDefault(o => o.Value == Office?.ToString())?.TimeZone;

            if (string.IsNullOrEmpty(timeZone))
            {
                return Page(); // prevent querying with invalid timezone
            }

            var query = _context.Orders.Where(o => !o.IsVoided)
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems)
                .Include(o => o.Payments)
                .Where(o => o.OfficeId == Office)
                .AsQueryable();

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                // Get the first day of the selected month
                DateTime localStart = new DateTime(Date.Value.Year, Date.Value.Month, 1);

                // Get the first day of the next month
                DateTime localEnd = localStart.AddMonths(1);

                // Convert both to UTC
                DateTime utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
                DateTime utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

                // Apply filter for the entire month
                query = query.Where(o => o.OrderDate >= utcStart && o.OrderDate < utcEnd);
            }
            catch (Exception ex)
            {
                // optionally log ex
            }

            Order = await query.ToListAsync();
            //Office = Office;
            //Date = dDate;

            return Page();
        }


    }
}

