using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Inventory.Locations
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InventoryLocation InventoryLocation { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            InventoryLocation = await _context.InventoryLocations
                .Include(i => i.Office)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (InventoryLocation == null) return NotFound();

            ViewData["Offices"] = new SelectList(
                await _context.OfficeCountry.ToListAsync(),
                "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["Offices"] = new SelectList(
                    await _context.OfficeCountry.ToListAsync(),
                    "Id", "Name");
                return Page();
            }

            var existing = await _context.InventoryLocations.FindAsync(InventoryLocation.Id);
            if (existing == null) return NotFound();

            // Update fields
            existing.Name = InventoryLocation.Name;
            existing.LocationType = InventoryLocation.LocationType;
            existing.OfficeId = InventoryLocation.OfficeId;

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
