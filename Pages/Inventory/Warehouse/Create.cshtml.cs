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
using NLI_POS.Services;
using NuGet.Versioning;

namespace NLI_POS.Pages.Inventory.Warehouse
{
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? locationId { get; set; }

        public IActionResult OnGet(int locationId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Inventory"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage();
            }

            //ViewData["OfficeId"] = new SelectList(_context.OfficeCountry.Where(o => o.Id == officeId), "Id", "Name");

            ViewData["LocationId"] = new SelectList(_context.InventoryLocations
            .Include(l => l.Office) 
            .Where(l => l.IsActive && l.Id == locationId),
                "Id",
                "Name"
            );


            //ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");

            var usedProductIds = _context.InventoryStocks.Where(i => i.Location.Id == locationId)
            .Select(s => s.ProductId)
            .ToList();

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.ProductCategory != "Package")
                    .Where(p => !usedProductIds.Contains(p.Id))
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.ProductName + " - [" + p.ProductCode + "]"
                    })
                    .ToList(),
                "Id",
                "Name"
            );


            return Page();
        }

        [BindProperty]
        public InventoryStock InventoryStock { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync(int locationId)
        {
            //var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            //var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);

            var cntryOffice2 = _context.OfficeCountry.Include(c => c.Country).FirstOrDefault(c => c.Id == locationId);
            var cntryOffice = _context.InventoryLocations
                .Include(l => l.Office)
                .FirstOrDefault(l => l.Id == 1);

            InventoryStock.EncodeDate = DateTime.UtcNow;
            InventoryStock.EncodedBy = User?.Identity.Name;


            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.InventoryStocks.Add(InventoryStock);
            await _context.SaveChangesAsync();

            var product = await _context.Products.FindAsync(InventoryStock.ProductId);
            var productName = product?.ProductName ?? "Unknown";
            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Added inventory product: {productName} with {InventoryStock.StockQty} stock(s) for {cntryOffice.Name}");

            // Save transaction log
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = InventoryStock.ProductId,
                FromLocationId = InventoryStock.LocationId,
                ToLocationId = null,
                Quantity = InventoryStock.StockQty,
                TransactionType = "Product IN",
                TransactionDate = DateTime.UtcNow,
                EncodedBy = User.Identity?.Name ?? "SYSTEM"
            });

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { locationId = locationId });
        }
    }
}
