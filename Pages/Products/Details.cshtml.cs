using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Product Products { get; set; } = default!;
        public ProductPrice ProductPrice { get; set; }
        public List<ProductCombo> ProductCombos { get; set; }
        public List<Country> Countries { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Products = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (Products == null)
                return NotFound();

            ProductPrice = await _context.ProductPrices.FirstOrDefaultAsync(p => p.ProductId == id);
            ProductCombos = await _context.ProductCombos.Where(p => p.ProductId == id).ToListAsync();
            Countries = await _context.Country.Where(c => c.IsActive).ToListAsync();

            return Page();
        }
    }

}
