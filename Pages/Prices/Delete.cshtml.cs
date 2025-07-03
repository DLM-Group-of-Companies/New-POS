using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Prices
{
    public class DeleteModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DeleteModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProductPrice ProductPrice { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productprice = await _context.ProductPrices.FirstOrDefaultAsync(m => m.Id == id);

            if (productprice == null)
            {
                return NotFound();
            }
            else
            {
                ProductPrice = productprice;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productprice = await _context.ProductPrices.FindAsync(id);
            if (productprice != null)
            {
                ProductPrice = productprice;
                _context.ProductPrices.Remove(ProductPrice);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
