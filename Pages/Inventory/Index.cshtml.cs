using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Inventory
{
    [Authorize(Roles = "Admin,Inventory")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        //[BindProperty]
        //public string Office { get; set; }

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
                .Include(i => i.Office)
                .Include(i => i.Products)
                .Where(m => m.Products.ProductClass == "Main");

            if (officeId.HasValue)
            {
                query = query.Where(m => m.Office.Id == officeId.Value);
            }

            var inventoryStockMain = await query.ToListAsync();

            return new JsonResult(new { data = inventoryStockMain });
        }

        public async Task<JsonResult> OnGetCollateral(int? officeId)
        {
            var query = _context.InventoryStocks
                .Include(i => i.Office)
                .Include(i => i.Products)
                .Where(m => m.Products.ProductClass == "Collateral");

            if (officeId.HasValue)
            {
                query = query.Where(m => m.Office.Id == officeId.Value);
            }

            var inventoryStockColl = await query.ToListAsync();

            return new JsonResult(new { data = inventoryStockColl });
        }
    }
}
