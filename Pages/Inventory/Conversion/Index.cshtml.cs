using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models.NLI_POS.Models;

namespace NLI_POS.Pages.Inventory.Conversion
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProductConversion> Conversions { get; set; }
        [BindProperty]
        public ProductConversionModalModel Modal { get; set; }

        public class ProductConversionModalModel
        {
            public ProductConversion Conversion { get; set; }
            [ValidateNever]
            public SelectList FromProducts { get; set; }
            [ValidateNever]
            public SelectList ToProducts { get; set; }

            public int ConvertQty { get; set; }
        }

        public async Task OnGetAsync()
        {
            Conversions = await _context.ProductConversions
                .Include(x => x.FromProduct)
                .Include(x => x.ToProduct)
                .ToListAsync();
        }

        // Load modal (Create or Edit)
        public async Task<IActionResult> OnGetCreateEditAsync(int? id)
        {
            var fromProducts = await _context.Products
                .Where(p => p.ProductType.ToUpper() == "BOX" && p.ProductCategory == "Regular" && p.IsActive)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var toProducts = await _context.Products
                .Where(p => (p.ProductType == "SACHET"|| p.ProductType == "BLISTER") && p.IsActive)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            var model = new ProductConversionModalModel
            {
                Conversion = id == null
                    ? new ProductConversion()
                    : await _context.ProductConversions.FindAsync(id),
                FromProducts = new SelectList(fromProducts, "Id", "ProductName"),
                ToProducts = new SelectList(toProducts, "Id", "ProductName")
            };

            return Partial("_CreateEdit", model);
        }


        // Save (Create or Edit)
        //public async Task<IActionResult> OnPostSaveAsync(ProductConversionModalModel model)
        public async Task<IActionResult> OnPostSaveAsync()
        {
            //if (!ModelState.IsValid)
            //    return new JsonResult(new { success = false, message = "Invalid data." });

            if (!ModelState.IsValid)
            {
                // 🔴 REPOPULATE DROPDOWNS
                Modal.FromProducts = new SelectList(
                    _context.Products
                        .Where(p => p.ProductType.ToUpper() == "BOX")
                        .OrderBy(p => p.ProductName),
                    "Id",
                    "ProductName");

                Modal.ToProducts = new SelectList(
                    _context.Products
                        .Where(p => p.ProductType.ToUpper() == "SACHET")
                        .OrderBy(p => p.ProductName),
                    "Id",
                    "ProductName");

                return new JsonResult(new
                {
                    success = false,
                    message = "Validation failed. Please check inputs."
                });
            }

            var conversion = Modal.Conversion;

            if (conversion.FromProductId == conversion.ToProductId)
                return new JsonResult(new { success = false, message = "From and To product cannot be the same." });

            bool exists = await _context.ProductConversions.AnyAsync(x =>
                x.FromProductId == conversion.FromProductId &&
                x.ToProductId == conversion.ToProductId &&
                x.Id != conversion.Id);

            if (exists)
                return new JsonResult(new { success = false, message = "Conversion already exists." });

            if (conversion.Id == 0)
                _context.ProductConversions.Add(conversion);
            else
                _context.ProductConversions.Update(conversion);

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

    }


}
