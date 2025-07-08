using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;

namespace NLI_POS.Pages.Products
{
    [Authorize(Roles = "Admin,Accounting,Inventory")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get;set; } = default!;
        public List<Country> Countries { get; set; } = new();

        public async Task OnGetAsync()
        {
            Countries = _context.Country.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            Product = await _context.Products.ToListAsync();
            //.Include(p => p.ProductTypes).ToListAsync();
        }

        public JsonResult OnGetProducts(int? countryId)
        {
            var query = _context.Products
                //.Include(p => p.Country) // Assuming nav property
                .AsQueryable();

            //if (countryId.HasValue)
            //{
            //    query = query.Where(p => p.CountryCd == countryId.Value);
            //}

            var result = query.OrderByDescending(p => p.ProductClass)
                .Select(p => new {
                code = p.ProductCode,
                name = p.ProductName,
                type = p.ProductType,
                category = p.ProductCategory,
                productClass = p.ProductClass, 
                active = p.IsActive
            }).ToList();

            return new JsonResult(new { data = result });
        }
    }
}
