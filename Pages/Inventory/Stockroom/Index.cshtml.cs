using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NLI_POS.Pages.Inventory.Stockroom
{
    [Authorize(Roles = "Admin,Inventory")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Location")]
        public int? locationId { get; set; }

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
                    ProductName = i.Product.ProductName,
                    ProductDescription = i.Product.ProductDescription,
                    StockQty = i.StockQty,
                    LocationId = i.LocationId,
                    LocationName = i.Location.Name
                    
                })
                .ToListAsync();

            return new JsonResult(new { data = dtoList });
        }
    }
}
