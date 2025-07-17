using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.ProductCombos
{
    public class EditModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public EditModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProductCombo ProductCombo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productcombo =  await _context.ProductCombos.FirstOrDefaultAsync(m => m.Id == id);
            if (productcombo == null)
            {
                return NotFound();
            }
            ProductCombo = productcombo;
           ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
            return Page();
        }

        public async Task<JsonResult> OnPostToggleStatusAsync(int id, bool isActive)
        {
            var combo = await _context.ProductCombos.FindAsync(id);
            if (combo == null)
            {
                return new JsonResult(new { success = false, error = "Record not found." });
            }

            combo.IsActive = isActive;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnGetRefreshCombosAsync(int? productId)
        {
            var combos = await _context.ProductCombos.Include(p => p.Products).Where(p=>p.ProductId == productId).ToListAsync();
            return Partial("_ProductComboPartial", combos);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ProductCombo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductComboExists(ProductCombo.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProductComboExists(int id)
        {
            return _context.ProductCombos.Any(e => e.Id == id);
        }
    }
}
