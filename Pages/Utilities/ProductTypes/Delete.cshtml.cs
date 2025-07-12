using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Utilities.ProdutcTypes
{
    public class DeleteModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DeleteModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProductType ProductType { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producttype = await _context.ProductTypes.FirstOrDefaultAsync(m => m.Id == id);

            if (producttype == null)
            {
                return NotFound();
            }
            else
            {
                ProductType = producttype;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producttype = await _context.ProductTypes.FindAsync(id);
            if (producttype != null)
            {
                ProductType = producttype;
                _context.ProductTypes.Remove(ProductType);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
