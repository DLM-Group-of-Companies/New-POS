using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Inventory
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

            var inventorystock =  await _context.InventoryStocks.FirstOrDefaultAsync(m => m.Id == id);
            if (inventorystock == null)
            {
                return NotFound();
            }
            InventoryStock = inventorystock;
           ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
           ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
            return Page();
        }

        public async Task<PartialViewResult> OnGetPartialAsync(int id)
        {
            InventoryStock = await _context.InventoryStocks
                .Include(i => i.Products)
                .Include(i => i.Office)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (InventoryStock == null)
            {
                return Partial("_NotFoundPartial", null); // optional
            }

            ViewData["ProductId"] = new SelectList(
                await _context.Products.ToListAsync(), "Id", "ProductName", InventoryStock.ProductId);

            ViewData["OfficeId"] = new SelectList(
                await _context.OfficeCountry.ToListAsync(), "Id", "Name", InventoryStock.OfficeId);

            Console.WriteLine("Offices: " + _context.OfficeCountry.Count());
            Console.WriteLine("Products: " + _context.Products.Count());


            return Partial("_EditInventoryPartial", this);
        }


        public async Task<IActionResult> OnPostAsync()
        {
            InventoryStock.Products = null;
            InventoryStock.Office = null;

            var offc = _context.InventoryStocks
                .AsNoTracking()
                .Include(o => o.Office)
                .FirstOrDefault(o => o.Id == InventoryStock.Id);

            var cntry = _context.OfficeCountry
                .AsNoTracking()
                .Include(o => o.Country)
                .FirstOrDefault(o => o.Id == offc.OfficeId);

            var localTime = AuditHelpers.GetLocalTime(cntry.Country.TimeZone);

            InventoryStock.UpdateDate = localTime;
            InventoryStock.UpdatedBy = User?.Identity?.Name;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch original entity from the database to preserve EncodedBy
            var original = await _context.InventoryStocks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == InventoryStock.Id);

            if (original == null)
                return NotFound();

            // Keep the original EncodedBy
            InventoryStock.EncodedBy = original.EncodedBy;

            // Attach and mark only specific properties as modified
            _context.Attach(InventoryStock);
            _context.Entry(InventoryStock).Property(x => x.StockQty).IsModified = true;
            _context.Entry(InventoryStock).Property(x => x.Remarks).IsModified = true;
            //_context.Entry(InventoryStock).Property(x => x.UpdateDate).IsModified = true;
            //_context.Entry(InventoryStock).Property(x => x.UpdatedBy).IsModified = true;
            // Don't touch EncodedBy!

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

            return RedirectToPage("./Index");
        }


        private bool InventoryStockExists(int id)
        {
            return _context.InventoryStocks.Any(e => e.Id == id);
        }
    }
}
