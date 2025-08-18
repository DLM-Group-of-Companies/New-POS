using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Customers
{
    public class DetailsModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DetailsModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Customer Customer { get; set; } = default!;
        public IList<OrderSummary> Order { get; set; } = new List<OrderSummary>();

        public class OrderSummary
        {
            public string OrderNo { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string Office { get; set; } = string.Empty;
            public decimal TotAmount { get; set; }
        }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!User.HasPermission("View"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage("./Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var customer = await _context.Customer.Include(c => c.OfficeCountry).Include(c => c.CustClasses).FirstOrDefaultAsync(m => m.Id == id);
                ViewData["Country"] = _context.Country.FirstOrDefault(c => c.Code == customer.Country)?.Name ?? "";

                if (customer == null)
                {
                    return NotFound();
                }
                else
                {

                    Customer = customer;
                    Order = _context.Orders
                        .Where(o => o.CustomerId == id && !o.IsVoided)
                              .Include(o => o.Customers)
                              .Include(o => o.Office)
                    //.Include(o => o.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Include(o => o.ProductItems)
                              .AsEnumerable()
                              .GroupBy(o => new
                              {
                                  o.OrderNo,
                                  OrderDate = o.OrderDate.Date,
                                  o.CustomerId,
                                  o.OfficeId,
                                  o.OrderType,
                                  CustomerName = o.Customers.FirstName + " " + o.Customers.LastName,
                                  OfficeName = o.Office.Name
                              })
                              .Select(g => new OrderSummary
                              {
                                  OrderNo = g.Key.OrderNo,
                                  OrderDate = g.Key.OrderDate,
                                  CustomerName = g.Key.CustomerName,
                                  Office = g.Key.OfficeName,
                                  TotAmount = (decimal)g.Sum(x => x.TotAmount)
                              })
                              .ToList();
                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = "We’re having trouble connecting to the server. Please try again later.";
                return RedirectToPage("/Error");
            }
            return Page();
        }
    }
}
