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

namespace NLI_POS.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<InventoryStock> InventoryStockMain { get; set; } = default!;
        public IList<InventoryStock> InventoryStockColl { get; set; } = default!;
        public string Office { get; set; }

        public async Task OnGetAsync()
        {
            User.IsInRole("Admin");

            var offices = _context.OfficeCountry
            .Where(o => o.isActive)
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Name
            })
            .ToList();

            // Insert the default "--Select--" item at the top
            if (offices.Count > 1)
            {
                offices.Insert(0, new SelectListItem { Value = "", Text = "--Select--" });
            }


            ViewData["Office"] = offices;

            InventoryStockMain = await _context.InventoryStocks
                .Include(i => i.Office)
                .Include(i => i.Products)
                .Where(m => m.Products.ProductClass == "Main")
                .ToListAsync();

            InventoryStockColl = await _context.InventoryStocks
    .Include(i => i.Office)
    .Include(i => i.Products)
    .Where(m => m.Products.ProductClass == "Collateral")
    .ToListAsync();
        }
    }
}
