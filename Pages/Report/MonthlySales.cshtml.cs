using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Models.ViewModels;

namespace NLI_POS.Pages.Report
{
    public class MonthySalesModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public MonthySalesModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Office { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        public List<Order> Order { get; set; }
        public List<OfficeCountry> Offices { get; set; }
        public SelectList OfficeList { get; set; }
        public OrderSummaryViewModel OrderSummary { get; set; }

        public async Task OnGetAsync()
        {
            Offices = _context.OfficeCountry.Where(o => o.isActive).ToList();

            // Set default Office if none selected
            if (string.IsNullOrEmpty(Office))
            {
                Office = Offices.FirstOrDefault()?.Name;
            }

            // Set default Date to today if none selected
            if (!Date.HasValue)
            {
                Date = DateTime.Today;
            }

            OfficeList = new SelectList(Offices.DistinctBy(o => o.Name), "Name", "Name", Office);

            int? officeId = int.TryParse(Office, out var tempId) ? tempId : null;
            var date = Date;

            Order = await _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems)
                .Include(o => o.Payments)
                .Where(o => (!officeId.HasValue || o.OfficeId == officeId.Value) &&
                            (!date.HasValue || (o.OrderDate.Month == date.Value.Month && o.OrderDate.Year == date.Value.Year)))
                .ToListAsync();


            //Order = query.ToList();
        }


    }
}

