using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NLI_POS.Pages.Inventory.Office
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
        [Display(Name ="Office")]
        public int? officeId { get; set; }

        public IActionResult OnGet(int? officeId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Inventory"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage();
            }

            var offices = _context.OfficeCountry
            .Where(o => o.isActive)
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Name,
                Selected = (officeId != null && o.Id == officeId)
            })
            .ToList();

            ViewData["OfficeId"] = offices;
            return Page();
        }

        public async Task<JsonResult> OnGetMain(int? officeId)
        {
            var query = _context.InventoryStocks
                .Include(i => i.Location)
                .Include(i => i.Product)
                .Where(m => m.Product.ProductClass == "Main");

            if (officeId.HasValue)
            {
                query = query.Where(m => m.Location.OfficeId == officeId.Value);
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
                    OfficeId = (int)i.Location.OfficeId
                })
                .ToListAsync();

            return new JsonResult(new { data = dtoList });
        }


        public async Task<JsonResult> OnGetCollateral(int? officeId)
        {
            var query = _context.InventoryStocks
                .Include(i => i.Location)
                .Include(i => i.Product)
                .Where(m => m.Product.ProductClass == "Collateral");

            if (officeId.HasValue)
            {
                query = query.Where(m => m.Location.OfficeId == officeId.Value);
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
                    OfficeId = (int)i.Location.OfficeId
                })
                .ToListAsync();

            return new JsonResult(new { data = dtoList });
        }
    }
}
