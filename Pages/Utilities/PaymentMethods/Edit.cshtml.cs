using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Utilities.PaymentMethods
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public EditModel(NLI_POS.Data.ApplicationDbContext context)
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
            PaymentMethods = paymentmethods;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (PaymentMethods.ServiceCharge != null && PaymentMethods.ServiceCharge > 0)
            {
                PaymentMethods.ServiceCharge /= 100;
            }

            _context.Attach(PaymentMethods).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentMethodsExists(PaymentMethods.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PaymentMethodsExists(int id)
        {
            return _context.PaymentMethods.Any(e => e.Id == id);
        }
    }
}
