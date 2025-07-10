using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Report
{
    public class SalesByDateRangeModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public SalesByDateRangeModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Order> Order { get; set; }
        public List<OfficeCountry> Offices { get; set; }

        public async Task OnGetAsync()
        {
            Order = await _context.Orders
                .Include(o => o.Customers)
                //.Include(o => o.Product)
                //.Include(o => o.ProductCombos)
                .Include(o => o.ProductItems)
                .Include(o => o.Office)
                .ToListAsync();

            Offices = await _context.OfficeCountry.ToListAsync();
        }

    }
}
