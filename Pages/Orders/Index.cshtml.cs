using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;

namespace NLI_POS.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Order> Order { get;set; } = default!;

        //public async Task OnGetAsync()
        //{
        //    Order = await _context.Orders
        //        .Include(o => o.Customers)
        //        .Include(o => o.Office)
        //        .Include(o => o.Products).ToListAsync();
        //}

        public JsonResult OnGetOrders()
        {
            var orders = _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.Products)
                .Select(o => new {
                    o.OrderNo,
                    OrderDate = o.OrderDate.ToString("yyyy-MM-dd"),
                    o.ItemNo,
                    CustomerName = o.Customers.FirstName + " " + o.Customers.LastName,
                    Office = o.Office.Name,
                    o.Qty,
                    ProductName = o.Products.ProductName,
                    Amount = o.Amount,
                    o.Id
                }).ToList();

            return new JsonResult(new { data = orders });
        }
    }
}
