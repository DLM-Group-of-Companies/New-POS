using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using NLI_POS.ViewModels;

namespace NLI_POS.Pages.Inventory.Warehouse
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

            //ViewData["OfficeId"] = new SelectList(
            //    await _context.OfficeCountry.ToListAsync(), "Id", "Name", InventoryStock.OfficeId);

            ViewData["LocationId"] = new SelectList(
                await _context.InventoryLocations.Include(l => l.Office).ToListAsync(), "Id", "Name", InventoryStock.LocationId);

            Console.WriteLine("Offices: " + _context.OfficeCountry.Count());
            Console.WriteLine("Products: " + _context.Products.Count());
            var found = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Pages", "Inventory", "Shared", "_EditInventoryPartial.cshtml"));
            Console.WriteLine("Partial view exists: " + found);

            var location = await _context.InventoryLocations
            .Where(l => l.Id == InventoryStock.LocationId)
            .Select(l => l.Name)
            .FirstOrDefaultAsync();

            var vm = new InventoryStockEditViewModel
            {
                Id = InventoryStock.Id,
                ProductId = InventoryStock.ProductId,
                ProductName = InventoryStock.Product.ProductName,
                LocationId = InventoryStock.LocationId,
                LocationName = location,
                StockQty = InventoryStock.StockQty,
                Remarks = InventoryStock.Remarks
                // Map other needed fields
            };

            return Partial("~/Pages/Inventory/Shared/_EditInventoryPartial.cshtml", vm);

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

            // Save transaction log
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = original.ProductId,
                FromLocationId = original.Location.Id,
                ToLocationId = null,
                Quantity = original.StockQty,
                TransactionType = "Modified stock quantity",
                TransactionDate = DateTime.UtcNow,
                EncodedBy = User.Identity?.Name ?? "SYSTEM"
            });

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { locationId = original.LocationId });

        }

        private bool InventoryStockExists(int id)
        {
            return _context.InventoryStocks.Any(e => e.Id == id);
        }
    }
}
