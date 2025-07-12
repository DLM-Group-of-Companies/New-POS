using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Orders
{
    [Authorize(Roles = "Admin,CS")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Order> Order { get;set; } = default!;

        public async Task<JsonResult> OnGetOrdersAsync()
        {
            var orders = _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems)
                .GroupBy(o => new
                {
                    o.OrderNo,
                    OrderDate = o.OrderDate.Date,
                    o.CustomerId,
                    o.OfficeId,
                    o.OrderType,
                    CustomerName = o.Customers.FirstName + " " + o.Customers.LastName,
                    OfficeName = o.Office.Name,
                    o.IsVoided
                })
                .Select(g => new
                {
                    g.Key.OrderNo,
                    OrderDate = g.Key.OrderDate,
                    CustomerName = g.Key.CustomerName,
                    Office = g.Key.OfficeName,
                    isVoided = g.Key.IsVoided,
                    TotAmount = g.Sum(x => x.TotPaidAmount)
                })
                .ToList();

                await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Order List");

            return new JsonResult(new { data = orders });
        }
    }
}
