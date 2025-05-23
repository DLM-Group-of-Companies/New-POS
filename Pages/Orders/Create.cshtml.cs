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

namespace NLI_POS.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {

            var customer = _context.Customer
                .Select(p => new
                {
                    Id = p.Id,
                    FullName = p.CustCode + " | " + p.FirstName + " " + p.LastName
                })
                .ToList();

            ViewData["CustomerId"] = new SelectList(customer, "Id", "FullName");
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
            return Page();
        }

        public IActionResult OnGetProductList(string ProdCat)
        {
            List<SelectListItem> SectionList = (from d in _context.Products.Where(p=>p.ProductCategory==ProdCat)
                                                select new SelectListItem
                                                {
                                                    Text = d.ProductName,
                                                    Value = d.Id.ToString()
                                                }).ToList();

            //SectionList.Insert(0, new SelectListItem { Text = "--Select Product--", Value = "" });

            return new JsonResult(SectionList);
        }

        public JsonResult OnGetGetProducAmount(int id)
        {
            var ProdAmount = _context.Products.FirstOrDefault(p=>p.Id==id);

            return new JsonResult( ProdAmount.RegPrice);

        }

        [BindProperty]
        public Order Order { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
