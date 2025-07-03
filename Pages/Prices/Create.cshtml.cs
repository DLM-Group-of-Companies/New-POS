using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Prices
{
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CountryId"] = new SelectList(_context.Country.Where(c=>c.IsActive), "Id", "Name");
        ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
            return Page();
        }

        [BindProperty]
        public ProductPrice ProductPrice { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ProductPrices.Add(ProductPrice);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
