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
            //ViewData["ProductCombos"] = new SelectList(_context.ProductCombos, "Id", "ProductsDesc");
            return Page();
        }

        public IActionResult OnGetCustomers()
        {

            List<SelectListItem> SectionList = (from c in _context.Customer
                                                select new SelectListItem
                                                {
                                                    Text = c.CustCode + " | " + c.FirstName + " " + c.LastName,
                                                    Value = c.Id.ToString()
                                                }).ToList();
  
            return new JsonResult(SectionList);
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

        public IActionResult OnGetProductComboList(int ProdId)
        {
            List<SelectListItem> SectionList = (from d in _context.ProductCombos.Where(p => p.ProductId == ProdId)
                                                select new SelectListItem
                                                {
                                                    Text = d.ProductsDesc,
                                                    Value = d.Id.ToString()
                                                }).ToList();

            if (SectionList.Count == 0)
            {
                SectionList.Insert(0, new SelectListItem { Text = "NA", Value = "" });
            }


            return new JsonResult(SectionList);
        }

        public JsonResult OnGetGetProducAmount(int id)
        {
            //ViewData["ProductCombos"] = new SelectList(_context.ProductCombos.Where(c=>c.ProductId==id), "Id", "ProductsDesc");

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
