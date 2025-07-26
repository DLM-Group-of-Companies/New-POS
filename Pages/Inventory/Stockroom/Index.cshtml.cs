using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Models.Dto;
using NLI_POS.Models.ViewModels;
using NLI_POS.Pages.Inventory.Shared;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NLI_POS.Pages.Inventory.Stockroom
{
    [Authorize(Roles = "Admin,Inventory")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        private readonly TransferViewModelBuilder _transferBuilder;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
            _transferBuilder = new TransferViewModelBuilder(context);
        }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Location")]
        public int? locationId { get; set; }

        public TransferInventoryViewModel TransferView { get; set; }

        public IActionResult OnGet(int? LocationId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Inventory"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage();
            }

            ViewData["LocationId"] = new SelectList(_context.InventoryLocations
                .Include(l => l.Office)
                .Where(l => l.IsActive && l.Id == 2),//Make sure it's locked to StockRoom (2)
                    "Id",
                    "Name"
                );

            return Page();
        }

        public async Task<JsonResult> OnGetMain(int? locationId)
        {
            locationId = 2; //Fixed for Stockroom

            var query = _context.InventoryStocks
                .Include(i => i.Location)
                .Include(i => i.Product)
                .Where(m => m.Product.ProductClass == "Main");

            if (locationId.HasValue)
            {
                query = query.Where(m => m.Location.Id == locationId.Value);
            }

            var dtoList = await query
                .Select(i => new InventoryStockDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    ProductDescription = i.Product.ProductDescription,
                    StockQty = i.StockQty,
                    LocationId = i.LocationId,
                    LocationName = i.Location.Name,
                    OfficeId = i.Location.OfficeId ?? 0
                })
                .ToListAsync();

            return new JsonResult(new { data = dtoList });
        }


        public async Task<JsonResult> OnGetCollateral(int? locationId)
        {
            locationId = 2; //Fixed for Stockroom

            var query = _context.InventoryStocks
                .Include(i => i.Location)
                .Include(i => i.Product)
                .Where(m => m.Product.ProductClass == "Collateral");

            if (locationId.HasValue)
            {
                query = query.Where(m => m.Location.Id == locationId.Value);
            }

            var dtoList = await query
                .Select(i => new InventoryStockDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    ProductDescription = i.Product.ProductDescription,
                    StockQty = i.StockQty,
                    LocationId = i.LocationId,
                    LocationName = i.Location.Name
                    
                })
                .ToListAsync();

            return new JsonResult(new { data = dtoList });
        }

        public async Task<PartialViewResult> OnGetTransferPartialAsync(int locationId, int productId)
        {
            var vm = await _transferBuilder.BuildAsync(locationId, productId, lockFrom: true);
            return Partial("~/Pages/Inventory/Shared/_TransferPartial.cshtml", vm);
        }

        public async Task<IActionResult> OnPostTransferAsync([FromBody] TransferInventoryViewModel vm)
        {

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid quantity." });
            }

            // Check available stock
            var fromStock = await _context.InventoryStocks
                .FirstOrDefaultAsync(s => s.LocationId == vm.FromLocationId && s.ProductId == vm.ProductId);

            if (fromStock == null || vm.Quantity > fromStock.StockQty)
            {
                TempData["Error"] = "Transfer quantity exceeds available stock.";
                return new JsonResult(new { success = false, message = "Transfer quantity exceeds available stock." });
            }

            // Perform the transfer logic here
            // Deduct from source
            fromStock.StockQty -= vm.Quantity;

            // Add to destination
            var toStock = await _context.InventoryStocks
                .FirstOrDefaultAsync(s => s.LocationId == vm.ToLocationId && s.ProductId == vm.ProductId);

            if (toStock != null)
            {
                toStock.StockQty += vm.Quantity;
            }
            else
            {
                _context.InventoryStocks.Add(new InventoryStock
                {
                    LocationId = vm.ToLocationId,
                    ProductId = vm.ProductId,
                    StockQty = vm.Quantity
                });
            }

            await _context.SaveChangesAsync();

            // Save transaction log
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = vm.ProductId,
                FromLocationId = vm.FromLocationId,
                ToLocationId = vm.ToLocationId,
                Quantity = vm.Quantity,
                TransactionType = "Transfer",
                TransactionDate = DateTime.UtcNow,
                EncodedBy = User.Identity?.Name ?? "SYSTEM"
            });

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}
