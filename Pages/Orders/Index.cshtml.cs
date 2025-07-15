using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using static NLI_POS.Services.BasePageModel;
using TimeZoneConverter;

namespace NLI_POS.Pages.Orders
{
    [Authorize(Roles = "Admin,CS")]
    public class IndexModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Order> Order { get; set; } = default!;
        public List<OfficeSelectItem> OfficeList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            OfficeList = await GetUserOfficesAsync();
            return Page();
        }

        public async Task<JsonResult> OnGetOrdersAsync(string officeId, string locale)
        {

            var query = _context.Orders.OrderByDescending(o=>o.OrderDate)
                   .Include(o => o.Customers)
                   .Include(o => o.Office)
                   .Include(o => o.ProductItems)
                   .AsQueryable();

            if (!string.IsNullOrEmpty(officeId))
            {
                if (int.TryParse(officeId, out int officeIdInt))
                {
                    query = query.Where(o => o.OfficeId == officeIdInt);
                }
            }

            var orders = await query
                .Select(o => new
                {
                    o.OrderNo,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customers.FirstName + " " + o.Customers.LastName,
                    Office = o.Office.Name,
                    o.IsVoided,
                    TotAmount = o.TotAmount
                })
                .ToListAsync();

            await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Order List");

            return new JsonResult(new { data = orders });
        }

      

    }
}
