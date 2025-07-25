using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Utilities.PaymentMethods
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public PaymentMethod PaymentMethods { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            PaymentMethods.ServiceCharge = PaymentMethods.ServiceCharge / 100;
            _context.PaymentMethods.Add(PaymentMethods);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
