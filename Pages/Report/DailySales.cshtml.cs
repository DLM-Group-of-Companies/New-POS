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

namespace NLI_POS.Pages.Report
{
    public class DailySalesModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DailySalesModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Office { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        public List<Order> Order { get; set; }
        public List<OfficeCountry> Offices { get; set; }
        public SelectList OfficeList { get; set; }
        public void OnGet()
        {
            Offices = _context.OfficeCountry.Where(o=>o.isActive).ToList();
            OfficeList = new SelectList(Offices.DistinctBy(o => o.Name), "Name", "Name", Office);
            var query = _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Products)
                .Include(o => o.ProductCombos)
                .Include(o => o.Office)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Office))
            {
                query = query.Where(o => o.Office.Name == Office);
            }

            if (Date.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date == Date.Value.Date);
            }

            Order = query.ToList();
        }

    }
}


