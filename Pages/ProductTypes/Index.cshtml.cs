using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data; // adjust namespace
using NLI_POS.Models; // adjust namespace
using System.Threading.Tasks;

namespace NLI_POS.Pages.ProductTypes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnGetListAsync()
        {
            var data = await _context.ProductTypes
                .Select(pt => new
                {
                    pt.Id,
                    pt.Name,
                    pt.Description,
                    pt.IsActive
                })
                .ToListAsync();

            return new JsonResult(new { data });
        }

        public async Task<IActionResult> OnPostCreateAsync([FromBody] ProductType productType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            productType.IsActive = true;
            _context.ProductTypes.Add(productType);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditAsync([FromBody] ProductType productType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.ProductTypes.FindAsync(productType.Id);
            if (existing == null)
                return NotFound();

            existing.Name = productType.Name;
            existing.Description = productType.Description;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int id)
        {
            var existing = await _context.ProductTypes.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
    }
}