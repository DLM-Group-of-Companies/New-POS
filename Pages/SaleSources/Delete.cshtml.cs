using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.SaleSources
{
    public class DeleteModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DeleteModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SalesSources SalesSources { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salessources = await _context.SalesSources.FirstOrDefaultAsync(m => m.id == id);

            if (salessources == null)
            {
                return NotFound();
            }
            else
            {
                SalesSources = salessources;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salessources = await _context.SalesSources.FindAsync(id);
            if (salessources != null)
            {
                SalesSources = salessources;
                _context.SalesSources.Remove(SalesSources);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
