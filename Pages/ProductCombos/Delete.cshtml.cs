using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace NLI_POS.Pages.ProductCombos
{
    public class DeleteModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        private readonly INotyfService _toastNotification;

        public DeleteModel(NLI_POS.Data.ApplicationDbContext context, INotyfService toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public ProductCombo ProductCombo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productcombo = await _context.ProductCombos.FirstOrDefaultAsync(m => m.Id == id);

            if (productcombo == null)
            {
                return NotFound();
            }
            else
            {
                ProductCombo = productcombo;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productcombo = await _context.ProductCombos.FindAsync(id);
            if (productcombo != null)
            {
                ProductCombo = productcombo;
                _context.ProductCombos.Remove(ProductCombo);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
