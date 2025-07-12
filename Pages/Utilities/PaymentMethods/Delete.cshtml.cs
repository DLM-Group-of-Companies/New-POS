using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Utilities.PaymentMethods
{
    public class DeleteModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DeleteModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PaymentMethod PaymentMethods { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentmethods = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id);

            if (paymentmethods == null)
            {
                return NotFound();
            }
            else
            {
                PaymentMethods = paymentmethods;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentmethods = await _context.PaymentMethods.FindAsync(id);
            if (paymentmethods != null)
            {
                PaymentMethods = paymentmethods;
                _context.PaymentMethods.Remove(PaymentMethods);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
