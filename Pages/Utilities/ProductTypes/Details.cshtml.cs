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
    public class DetailsModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DetailsModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}
