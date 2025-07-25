using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Inventory.Office
{
    public class EditModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public EditModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InventoryStock InventoryStock { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventorystock = await _context.InventoryStocks.FirstOrDefaultAsync(m => m.Id == id);
            if (inventorystock == null)
            {
                return NotFound();
            }
            InventoryStock = inventorystock;
            //ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.InventoryLocations
     .Include(l => l.Office), "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
            return Page();
        }

        public async Task<PartialViewResult> OnGetPartialAsync(int id)
        {
            InventoryStock = await _context.InventoryStocks
                .Include(i => i.Product)
                .Include(i => i.Location) // ✅ updated
                .FirstOrDefaultAsync(i => i.Id == id);

            if (InventoryStock == null)
            {
                return Partial("_NotFoundPartial", null); // optional
            }

            ViewData["ProductId"] = new SelectList(
                await _context.Products.ToListAsync(), "Id", "ProductName", InventoryStock.ProductId);

            ViewData["LocationId"] = new SelectList(
                await _context.InventoryLocations.Include(l => l.Office).ToListAsync(), "Id", "Name", InventoryStock.LocationId);

            Console.WriteLine("Offices: " + _context.OfficeCountry.Count());
            Console.WriteLine("Products: " + _context.Products.Count());


            return Partial("_EditInventoryPartial", this);
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Ensure only valid submission
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get original record (for preserving EncodedBy, etc.)
            var original = await _context.InventoryStocks
                .AsNoTracking()
                .Include(i => i.Location)
                .ThenInclude(l => l.Office)
                .FirstOrDefaultAsync(s => s.Id == InventoryStock.Id);

            if (original == null)
                return NotFound();

            // Use OfficeId to find country & timezone
            var countryInfo = await _context.OfficeCountry
                .Include(o => o.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == original.Location.OfficeId);

            var localTime = AuditHelpers.GetLocalTime(countryInfo?.Country?.TimeZone ?? "Asia/Manila");

            // Manually update only allowed fields
            original.StockQty = InventoryStock.StockQty;
            original.Remarks = InventoryStock.Remarks;
            original.UpdateDate = localTime;
            original.UpdatedBy = User?.Identity?.Name;

            // Mark only modified fields
            _context.InventoryStocks.Update(original);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryStockExists(InventoryStock.Id))
                    return NotFound();
                else
                    throw;
            }

            // Get product name for audit trail
            var product = await _context.Products.FindAsync(original.ProductId);
            var productName = product?.ProductName ?? "Unknown";

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Updated inventory stock: {original.StockQty} for: {productName} on {countryInfo?.Name}");

            return RedirectToPage("./Index", new { locationId = original.LocationId });

        }

        private bool InventoryStockExists(int id)
        {
            return _context.InventoryStocks.Any(e => e.Id == id);
        }
    }
}
