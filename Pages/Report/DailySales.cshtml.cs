using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using NLI_POS.Models.ViewModels;
using static NLI_POS.Services.BasePageModel;

namespace NLI_POS.Pages.Report
{
    public class DailySalesModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public DailySalesModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
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

        public async Task<IActionResult> OnGetAsync(int? officeId, string timeZone, DateTime? dDate)
        {
            OfficeList = await GetUserOfficesAsync();
            
            //if (dDate == null)
            //{
            //    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            //    DateTime localStart = DateTime.UtcNow;
            //    dDate = localStart;
            //    DateTime localEnd = localStart.AddDays(1);

            //    DateTime utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
            //    DateTime utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);
            //}

            Order = new List<Order>();

            if (!officeId.HasValue || string.IsNullOrEmpty(timeZone) || !dDate.HasValue)
            {
                return Page(); // Don't query unless all filters are supplied
            }

            var query = _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems)
                .Include(o => o.Payments)
                .Where(o => o.OfficeId == officeId)
                .AsQueryable();

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                DateTime localStart = dDate.Value.Date;
                DateTime localEnd = localStart.AddDays(1);

                DateTime utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
                DateTime utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

                query = query.Where(o => o.OrderDate >= utcStart && o.OrderDate < utcEnd);
            }
            catch (Exception ex)
            {
                // Log error (optional)
            }

            Order = await query.ToListAsync();
            return Page();
        }

    }
}


