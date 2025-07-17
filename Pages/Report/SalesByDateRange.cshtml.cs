using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Models.ViewModels;
using NLI_POS.Services;

namespace NLI_POS.Pages.Report
{
    public class SalesByDateRangeModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public SalesByDateRangeModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string Office { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<Order> Order { get; set; }

        public List<OfficeSelectItem> OfficeList { get; set; }
        public OrderSummaryViewModel OrderSummary { get; set; }

        [BindProperty(SupportsGet = true)]
        public string timeZone { get; set; }

        public async Task<IActionResult> OnGetAsync(int? officeId, string timeZone, DateTime? dDateFrom, DateTime? dDateTo)
        {
            OfficeList = await GetUserOfficesAsync();
            Order = new List<Order>();

            if (!officeId.HasValue || !dDateFrom.HasValue || !dDateTo.HasValue)
            {
                return Page(); // skip querying if filters not set
            }

            // fallback to first office's timezone if none provided
            timeZone ??= OfficeList.FirstOrDefault(o => o.Value == officeId?.ToString())?.TimeZone;

            if (string.IsNullOrEmpty(timeZone))
            {
                return Page(); // prevent querying with invalid timezone
            }

            var query = _context.Orders.Where(o => !o.IsVoided)
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems)
                .Include(o => o.Payments)
                .Where(o => o.OfficeId == officeId)
                .AsQueryable();

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                DateTime localStart = dDateFrom.Value.Date;
                DateTime localEnd = dDateTo.Value.Date.AddDays(1);

                DateTime utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
                DateTime utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

                // Enforce UTC kind (important if you save timestamps as UTC)
                utcStart = DateTime.SpecifyKind(utcStart, DateTimeKind.Utc);
                utcEnd = DateTime.SpecifyKind(utcEnd, DateTimeKind.Utc);

                query = query.Where(o => o.OrderDate >= utcStart && o.OrderDate < utcEnd);
            }
            catch (Exception ex)
            {
                // optionally log ex
            }

            Order = await query.ToListAsync();
            Office = officeId.ToString();
            DateFrom = dDateFrom;
            DateTo = dDateTo;

            return Page();
        }



    }
}
