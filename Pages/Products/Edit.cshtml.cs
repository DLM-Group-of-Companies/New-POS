using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NuGet.Packaging.Signing;

namespace NLI_POS.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public EditModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Products { get; set; } = default!;

        [BindProperty]
        public IList<IList<string>> ProductComboList { get; set; } 

        [BindProperty]
        public IList<ProductCombo> ProductCombos { get; set; }

        public ProductCombo ProductCombo { get; set; }

        [BindProperty]
        public IList<string> CombDesc { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product =  await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            Products = product;

            ProductCombos = _context.ProductCombos.Where(p => p.ProductId == id).ToList();

            ViewData["ProducTypeId"] = new SelectList(_context.ProductTypes, "Id", "Name");
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

        public async Task<IActionResult> OnPostAddRowAsync([FromBody] IList<IList<string>> productComboList)
        {
            // Process the data

            string prodId = "";
            string prodIds="";
            string combText = "";
            string qtyList = "";
            string selectedText = "";
            for (int i=0;i < productComboList.Count(); i++)
            {
                prodId = productComboList[i][0];
                //prodIds = productComboList[i][1]; 
                //qtyList = productComboList[i][2];
                selectedText = productComboList[i][3];
                if (combText == "")
                {
                    prodIds = productComboList[i][1];
                    combText += selectedText;
                    qtyList = productComboList[i][2];
                }
                else
                {
                    prodIds += ", " + productComboList[i][1];
                    combText += ", " + selectedText;
                    qtyList += ", " + productComboList[i][2];
                }
            }

            //ProductCombo.ProductId = int.Parse(prodId);
            //ProductCombo.ProductIdList = prodIds;
            //ProductCombo.ProductsDesc = combText;
            //ProductCombo.QuantityList = qtyList;

            if (!int.TryParse(prodId, out int parsedId))
            {
                throw new ArgumentException("Invalid Product ID");
            }

            var ProductCombo = new ProductCombo
            {
                ProductId = parsedId,
                ProductIdList = prodIds,       // ensure types match
                ProductsDesc = combText,
                QuantityList = qtyList         // ensure types match
            };

            ModelState.Remove("Products");
            _context.ProductCombos.Add(ProductCombo);
            await _context.SaveChangesAsync();
            return Page();
        }
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                GetProductList();
                return Page();
            }

            _context.Attach(Products).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Products.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
