using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Prices
{
    [Authorize(Roles = "Admin,Accounting")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProductPrice> ProductPrice { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ProductPrice = await _context.ProductPrices
                .Include(p => p.Country)
                .Include(p => p.Product).ToListAsync();
        }
    }
}
