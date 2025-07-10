using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models.ViewModels;

namespace NLI_POS.Pages.Shared
{
    public class _OrderDetailsPartialModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public _OrderDetailsPartialModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        //public IList<Order> Order { get;set; } = default!;

        public OrderSummaryViewModel OrderSummary { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var items = await _context.ProductItems
                .Where(p => p.OrderId == order.Id)
                .ToListAsync();

            var payments = await _context.OrderPayments
                .Where(p => p.OrderId == order.Id)
                .ToListAsync();

            OrderSummary = new OrderSummaryViewModel
            {
                Order = order,
                ProductItems = await _context.ProductItems
                    .Where(p => p.OrderId == order.Id)
                    .ToListAsync(),

                Payments = await _context.OrderPayments
                    .Where(p => p.OrderId == order.Id)
                    .ToListAsync()
            };


            return Page();
        }


    }
}
