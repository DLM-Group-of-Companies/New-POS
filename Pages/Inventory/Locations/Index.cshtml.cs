using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Inventory.Locations
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<InventoryLocation> InventoryLocations { get; set; }

        public async Task OnGetAsync()
        {
            InventoryLocations = await _context.InventoryLocations
                .Include(i => i.Office)
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var loc = await _context.InventoryLocations.FindAsync(id);
            if (loc == null) return NotFound();

            loc.IsActive = !loc.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }

}
