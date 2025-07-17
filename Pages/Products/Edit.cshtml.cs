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
using NLI_POS.Services;
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

        public List<Country> Countries { get; set; } = new(); // For dropdown

        [BindProperty]
        public ProductPrice ProductPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PageNumber { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Console.WriteLine("Page Number: " + PageNumber);
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

            ViewData["ProductType"] = new SelectList(_context.ProductTypes, "Name", "Name");

            Countries = await _context.Country.Where(c => c.IsActive).ToListAsync();

            ProductPrice = await _context.ProductPrices.FirstOrDefaultAsync(p => p.ProductId == id );

            return Page();
        }

        public List<SelectListItem> GetProductList()
        {
            List<SelectListItem> SectionList = (from d in _context.Products.Where(p => p.IsActive).OrderByDescending(p => p.ProductCategory)
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
                if (combText.Trim() == "")
                {
                    prodIds = productComboList[i][1];
                    combText += selectedText;
                    qtyList = productComboList[i][2];
                }
                else
                {
                    if (productComboList[i][3] != "")
                    {
                        prodIds += ", " + productComboList[i][1];
                        combText += ", " + selectedText;
                        qtyList += ", " + productComboList[i][2];
                    }
                }
            }

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

        public async Task<IActionResult> OnGetPrices(int countryId, int? id)
        {
            var price = await _context.ProductPrices
                .Where(p => p.CountryId == countryId && p.ProductId== id)
                .OrderByDescending(p => p.EncodeDate)
                .FirstOrDefaultAsync();

            if (price == null)
            {
                return new JsonResult(new
                {
                    UnitCost = 0m,
                    RegPrice = 0m,
                    DistPrice = 0m,
                    StaffPrice = 0m,
                    BPPPrice = 0m,
                    MedPackPrice = 0m,
                    CorpAccPrice = 0m,
                    NaturoPrice = 0m
                });
            }
            else
            {
                return new JsonResult(new
                {
                    price.UnitCost,
                    price.RegPrice,
                    price.DistPrice,
                    price.StaffPrice,
                    price.BPPPrice,
                    price.MedPackPrice,
                    price.CorpAccPrice,
                    price.NaturoPrice
                });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Countries = await _context.Country.Where(c => c.IsActive).ToListAsync();
                return Page();
            }

            // Load the current product from the database
            var originalProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == Products.Id);

            if (originalProduct == null)
            {
                return NotFound();
            }

            // Track if the product entity was changed
            bool isProductModified = !_context.Entry(Products).CurrentValues.Properties.All(prop =>
            {
                var originalValue = originalProduct.GetType().GetProperty(prop.Name)?.GetValue(originalProduct);
                var postedValue = Products.GetType().GetProperty(prop.Name)?.GetValue(Products);
                return Equals(originalValue, postedValue);
            });

            // Attach and mark the product as modified only if changed
            if (isProductModified)
            {
                Products.UpdateDate = DateTime.UtcNow;
                Products.UpdatedBy = User.Identity?.Name;
                _context.Attach(Products).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            // Always save or update the ProductPrice (as requested)
            ProductPrice.ProductId = Products.Id;
            ProductPrice.EncodeDate = DateTime.UtcNow;
            ProductPrice.EncodedBy = User.Identity?.Name ?? "System";

            // Check if price exists
            var existingPrice = await _context.ProductPrices
                .FirstOrDefaultAsync(p => p.ProductId == Products.Id && p.CountryId == ProductPrice.CountryId);

            if (existingPrice == null)
            {
                _context.ProductPrices.Add(ProductPrice);
            }
            else
            {
                existingPrice.UnitCost = ProductPrice.UnitCost;
                existingPrice.RegPrice = ProductPrice.RegPrice;
                existingPrice.DistPrice = ProductPrice.DistPrice;
                existingPrice.StaffPrice = ProductPrice.StaffPrice;
                existingPrice.BPPPrice = ProductPrice.BPPPrice;
                existingPrice.MedPackPrice = ProductPrice.MedPackPrice;
                existingPrice.CorpAccPrice = ProductPrice.CorpAccPrice;
                existingPrice.NaturoPrice = ProductPrice.NaturoPrice;
                existingPrice.EncodedBy = ProductPrice.EncodedBy;
                existingPrice.EncodeDate = ProductPrice.EncodeDate;
            }

            await _context.SaveChangesAsync();

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Edited Product: {originalProduct.ProductName}");
            return RedirectToPage("./Index");
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
