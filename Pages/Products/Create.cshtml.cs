using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Products { get; set; } = default!;

        [BindProperty]
        public IList<ProductCombo> ProductCombos { get; set; }

        //To Display existing combos
        public ProductCombo ProductCombs { get; set; }

        public IActionResult OnGet(int? Id)
        {
            ViewData["ProducTypeId"] = new SelectList(_context.ProductTypes, "Id", "Name");
            ViewData["Product"] = new SelectList(_context.Products.OrderBy(p=>p.ProductName), "Id", "ProductName");
            ProductCombos = _context.ProductCombos.Where(p => p.ProductId == Id).ToList();
            return Page();
        }

        public List<SelectListItem> GetProductList()
        {
            List<SelectListItem> SectionList = (from d in _context.Products
                                                select new SelectListItem
                                                {
                                                    Text = d.ProductName,
                                                    Value = d.Id.ToString()
                                                }).ToList();

            SectionList.Insert(0, new SelectListItem { Text = "--Select Product--", Value = "" });

            return SectionList;
        }

        public IActionResult OnGetProductList()
        {
            return new JsonResult(GetProductList());
        }

        public JsonResult OnPostSendData(List<ProductCombo> data)
        {
            // Process the data
            return new JsonResult(new { success = true, received = data });
        }

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            int arrayCount = ProductCombos.Count;
            for(int i = 0; i < arrayCount; i++)
            {
                ModelState.Remove("ProductCombos[" + i + "].Products");                
            }

            ModelState.Remove("Products.ProductTypes");

            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            
            _context.Products.Add(Products);
            await _context.SaveChangesAsync();

            var lastId = Products.Id;

            for(int j=0;j< arrayCount; j++)
            {
                ProductCombos[j].ProductId = lastId;
            }
            

            _context.ProductCombos.AddRange(ProductCombos);
            await _context.SaveChangesAsync();

            if (Products.ProductCategory == "Promo")
            {
                return RedirectToPage("./Edit", new { id = Products.Id });
            }
            else
            {
                return RedirectToPage("./Index");
            }
           
        }
    }
}
