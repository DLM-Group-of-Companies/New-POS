using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Customers
{
    public class DeleteModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DeleteModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer.FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            else
            {
                Customer = customer;
            }
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int? id)
        {
            if (id == null)
            {
                return new JsonResult(new { success = false, message = "Invalid ID" }) { StatusCode = 400 };
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return new JsonResult(new { success = false, message = "Customer not found" }) { StatusCode = 404 };
            }

            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
            if (hasOrders)
            {
                return new JsonResult(new { success = false, message = "Cannot delete: customer has transaction(s)." }) { StatusCode = 400 };
            }

            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Customer deleted successfully." });
        }

    }
}
