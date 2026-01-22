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

namespace NLI_POS.Pages.Inventory.Office
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

        public class ConvertInventoryModel
        {
            public int FromProductId { get; set; }
            public int LocationId { get; set; }
            [Required]
            [Range(1, int.MaxValue)]
            public int ConvertQty { get; set; }
        }

        [BindProperty]
        public ConvertInventoryModel ConvertModal { get; set; }


        public IActionResult OnGet(int? LocationId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Inventory"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage();
            }

            ViewData["LocationId"] = new SelectList(_context.InventoryLocations
                .Include(l => l.Office)
                .Where(l => l.IsActive && !new[] { 1, 2 }.Contains(l.Id)),
                    "Id",
                    "Name"
                );

            return Page();
        }

        //public async Task<JsonResult> OnGetMain(int? LocationId)
        //{
        //    var query = _context.InventoryStocks
        //        .Include(i => i.Location)
        //        .Include(i => i.Product)
        //        .Where(m => m.Product.ProductClass == "Main");

        //    if (locationId.HasValue)
        //    {
        //        query = query.Where(m => m.Location.Id == LocationId);
        //    }

        //    var dtoList = await query
        //        .Select(i => new InventoryStockDto
        //        {
        //            Id = i.Id,
        //            ProductId = i.ProductId,
        //            ProductName = i.Product.ProductName,
        //            ProductDescription = i.Product.ProductDescription,
        //            StockQty = i.StockQty,
        //            LocationId = i.LocationId,
        //            LocationName = i.Location.Name,
        //            OfficeId = i.Location.OfficeId ?? 0
        //        })
        //        .ToListAsync();

        //    return new JsonResult(new { data = dtoList });
        //}

        public async Task<JsonResult> OnGetMain(int? LocationId)
        {
            var query = _context.InventoryStocks
                .Include(i => i.Location)
                .Include(i => i.Product)
                .Where(m => m.Product.ProductClass == "Main");

            if (LocationId.HasValue)
            {
                query = query.Where(m => m.Location.Id == LocationId);
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

            // Fetch all warehouse MinLevels once
            var warehouseMinLevels = await _context.InventoryStocks
                .Where(i => i.LocationId == 1) // Warehouse LocationId
                .ToDictionaryAsync(i => i.ProductId, i => i.MinLevel);

            // Enrich each DTO with its corresponding warehouse MinLevel
            foreach (var dto in dtoList)
            {
                if (warehouseMinLevels.TryGetValue(dto.ProductId, out var minLevel))
                {
                    dto.WarehouseMinLevel = minLevel;
                }
            }

            return new JsonResult(new { data = dtoList });
        }



        public async Task<JsonResult> OnGetCollateral(int? LocationId)
        {
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
                    LocationName = i.Location.Name,
                    OfficeId = i.Location.OfficeId ?? 0
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

        public IActionResult OnGetLoadConvertModal(int productId, int locationId)
        {
            return Partial("_ProductConvertPartial", new ConvertInventoryModel
            {
                FromProductId = productId,
                LocationId = locationId,
                ConvertQty = 1
            });
        }

        public async Task<IActionResult> OnPostConvertAsync(ConvertInventoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Invalid conversion quantity."
                });
            }

            // 1️⃣ Get stock for product at selected location
            var stock = await _context.InventoryStocks
                .FirstOrDefaultAsync(s =>
                    s.ProductId == ConvertModal.FromProductId &&
                    s.LocationId == locationId);

            if (stock == null || stock.StockQty < ConvertModal.ConvertQty)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Insufficient stock."
                });
            }

            // 2️⃣ Get conversion rule (BOX → SACHET)
            var conversion = await _context.ProductConversions
                .FirstOrDefaultAsync(c => c.FromProductId == ConvertModal.FromProductId);

            if (conversion == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "No conversion rule defined."
                });
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3️⃣ Deduct BOX
                stock.StockQty -= ConvertModal.ConvertQty;

                // 4️⃣ Add SACHET
                var toStock = await _context.InventoryStocks
                    .FirstOrDefaultAsync(s =>
                        s.ProductId == conversion.ToProductId &&
                        s.LocationId == locationId);

                var addQty = ConvertModal.ConvertQty * conversion.ConversionQty;

                if (toStock == null)
                {
                    _context.InventoryStocks.Add(new InventoryStock
                    {
                        ProductId = conversion.ToProductId,
                        LocationId = locationId.Value,
                        StockQty = addQty
                    });
                }
                else
                {
                    toStock.StockQty += addQty;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return new JsonResult(new { success = true, message = "Successfully converted." });
            }
            catch
            {
                await tx.RollbackAsync();
                return new JsonResult(new
                {
                    success = false,
                    message = "Conversion failed."
                });
            }
        }

    }
}
