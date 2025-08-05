using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Models.ViewModels;
using NLI_POS.Services;
using System.Text;

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

        [BindProperty]
        public PromoSetting PromoSetting { get; set; }

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

            PromoSetting = await _context.PromoSettings
                .FirstOrDefaultAsync(p => p.ProductId == id)
                ?? new PromoSetting { ProductId = id.Value };

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
            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Added Combo {ProductCombo.ProductsDesc} for Product ID: {ProductCombo.ProductId}");
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

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    Countries = await _context.Country.Where(c => c.IsActive).ToListAsync();

        //    if (!ModelState.IsValid)
        //    {
        //        TempData["ErrorMessage"] = "Promo date range is required if not On Going.";
        //        //ModelState.AddModelError("PromoSetting", "Date range is required.");
        //        return Page();
        //    }

        //    if (!ProductWithPromoViewModel.PromoSetting.IsOngoing)
        //    {
        //        if (!ProductWithPromoViewModel.PromoSetting.StartDate.HasValue)
        //            ModelState.AddModelError("ProductWithPromoViewModel.PromoSetting.StartDate", "Start Date is required when promo is not ongoing.");

        //        if (!ProductWithPromoViewModel.PromoSetting.EndDate.HasValue)
        //            ModelState.AddModelError("ProductWithPromoViewModel.PromoSetting.EndDate", "End Date is required when promo is not ongoing.");
        //    }

        //    if (!ModelState.IsValid)
        //        return Page();

        //    var selectedCountry = Countries.FirstOrDefault(c => c.Id == ProductPrice.CountryId);
        //    string selectedCountryName = selectedCountry?.Name;

        //    var originalProduct = await _context.Products.AsNoTracking()
        //        .FirstOrDefaultAsync(p => p.Id == Products.Id);

        //    if (originalProduct == null)
        //    {
        //        return NotFound();
        //    }

        //    AuditHelpers.TryGetChanges(originalProduct, Products, out var productChanges, "UpdateDate", "UpdatedBy");

        //    bool isProductModified = productChanges.Any();

        //    if (isProductModified)
        //    {
        //        Products.UpdateDate = DateTime.UtcNow;
        //        Products.UpdatedBy = User.Identity?.Name;
        //        _context.Attach(Products).State = EntityState.Modified;
        //        await _context.SaveChangesAsync();
        //    }

        //    ProductPrice.ProductId = Products.Id;
        //    ProductPrice.EncodeDate = DateTime.UtcNow;
        //    ProductPrice.EncodedBy = User.Identity?.Name ?? "System";

        //    var existingPrice = await _context.ProductPrices
        //        .FirstOrDefaultAsync(p => p.ProductId == Products.Id && p.CountryId == ProductPrice.CountryId);

        //    List<string> priceChanges;

        //    if (existingPrice == null)
        //    {
        //        _context.ProductPrices.Add(ProductPrice);
        //        priceChanges = new List<string> { $"New pricing added for {selectedCountryName}." };
        //    }
        //    else
        //    {
        //        priceChanges = AuditHelpers.CompareProductPrice(existingPrice, ProductPrice);

        //        // Apply the changes
        //        existingPrice.UnitCost = ProductPrice.UnitCost;
        //        existingPrice.RegPrice = ProductPrice.RegPrice;
        //        existingPrice.DistPrice = ProductPrice.DistPrice;
        //        existingPrice.StaffPrice = ProductPrice.StaffPrice;
        //        existingPrice.BPPPrice = ProductPrice.BPPPrice;
        //        existingPrice.MedPackPrice = ProductPrice.MedPackPrice;
        //        existingPrice.CorpAccPrice = ProductPrice.CorpAccPrice;
        //        existingPrice.NaturoPrice = ProductPrice.NaturoPrice;
        //        existingPrice.EncodedBy = ProductPrice.EncodedBy;
        //        existingPrice.EncodeDate = ProductPrice.EncodeDate;
        //    }

        //    var promo = ProductWithPromoViewModel.PromoSetting;

        //    if (promo != null)
        //    {
        //        // Update existing or add new
        //        var existingPromo = await _context.PromoSettings
        //            .FirstOrDefaultAsync(p => p.ProductId == ProductWithPromoViewModel.Product.Id);

        //        if (existingPromo != null)
        //        {
        //            existingPromo.StartDate = promo.StartDate;
        //            existingPromo.EndDate = promo.EndDate;
        //            existingPromo.IsOngoing = promo.IsOngoing;
        //            existingPromo.IsVoucherBased = promo.IsVoucherBased;
        //            //existingPromo.IsActive = promo.IsActive;
        //            // No need to call Update() if tracked
        //        }
        //        else
        //        {
        //            promo.ProductId = ProductWithPromoViewModel.Product.Id;
        //            _context.PromoSettings.Add(promo);
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    //await _context.SaveChangesAsync();

        //    var fullAuditMessage = new StringBuilder();
        //    fullAuditMessage.AppendLine($"Edited Product: {originalProduct.ProductName}");

        //    if (productChanges.Any())
        //    {
        //        fullAuditMessage.AppendLine("Product detail changes:");
        //        foreach (var change in productChanges)
        //            fullAuditMessage.AppendLine($" - {change}");
        //    }

        //    if (priceChanges.Any())
        //    {
        //        fullAuditMessage.AppendLine($"Product price changes for {selectedCountryName}:");
        //        foreach (var change in priceChanges)
        //            fullAuditMessage.AppendLine($" - {change}");
        //    }

        //    await AuditHelpers.LogAsync(HttpContext, _context, User, fullAuditMessage.ToString());

        //    return RedirectToPage("./Index");
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            Countries = await _context.Country.Where(c => c.IsActive).ToListAsync();

            if (PromoSetting == null)
            {
                PromoSetting = new PromoSetting();
            }

            if (!PromoSetting.IsOngoing)
            {
                if (!PromoSetting.StartDate.HasValue)
                    ModelState.AddModelError("PromoSetting.StartDate", "Start Date is required when promo is not ongoing.");

                if (!PromoSetting.EndDate.HasValue)
                    ModelState.AddModelError("PromoSetting.EndDate", "End Date is required when promo is not ongoing.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Promo date range is required if not On Going.";
                return Page();
            }

            var selectedCountry = Countries.FirstOrDefault(c => c.Id == ProductPrice.CountryId);
            string selectedCountryName = selectedCountry?.Name;

            var originalProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == Products.Id);

            if (originalProduct == null)
                return NotFound();

            AuditHelpers.TryGetChanges(originalProduct, Products, out var productChanges, "UpdateDate", "UpdatedBy");

            var priceChanges = new List<string>();

            // TRACK MODIFIED PRODUCT
            if (productChanges.Any())
            {
                Products.UpdateDate = DateTime.UtcNow;
                Products.UpdatedBy = User.Identity?.Name;
                _context.Attach(Products).State = EntityState.Modified;
            }

            // HANDLE PRICING
            ProductPrice.ProductId = Products.Id;
            ProductPrice.EncodeDate = DateTime.UtcNow;
            ProductPrice.EncodedBy = User.Identity?.Name ?? "System";

            var existingPrice = await _context.ProductPrices
                .FirstOrDefaultAsync(p => p.ProductId == Products.Id && p.CountryId == ProductPrice.CountryId);

            if (existingPrice == null)
            {
                _context.ProductPrices.Add(ProductPrice);
                priceChanges.Add($"New pricing added for {selectedCountryName}.");
            }
            else
            {
                priceChanges = AuditHelpers.CompareProductPrice(existingPrice, ProductPrice);

                // Apply changes manually (EF is tracking existingPrice)
                existingPrice.UnitCost = ProductPrice.UnitCost;
                existingPrice.RegPrice = ProductPrice.RegPrice;
                existingPrice.DistPrice = ProductPrice.DistPrice;
                existingPrice.StaffPrice = ProductPrice.StaffPrice;
                existingPrice.BPPPrice = ProductPrice.BPPPrice;
                existingPrice.MedPackPrice = ProductPrice.MedPackPrice;
                existingPrice.CorpAccPrice = ProductPrice.CorpAccPrice;
                existingPrice.NaturoPrice = ProductPrice.NaturoPrice;
                existingPrice.EncodeDate = ProductPrice.EncodeDate;
                existingPrice.EncodedBy = ProductPrice.EncodedBy;
            }

            // HANDLE PROMO SETTINGS
            if (PromoSetting != null)
            {
                var existingPromo = await _context.PromoSettings
                    .FirstOrDefaultAsync(p => p.ProductId == Products.Id);

                if (existingPromo != null)
                {
                    existingPromo.StartDate = PromoSetting.StartDate;
                    existingPromo.EndDate = PromoSetting.EndDate;
                    existingPromo.IsOngoing = PromoSetting.IsOngoing;
                    existingPromo.IsVoucherBased = PromoSetting.IsVoucherBased;
                }
                else
                {
                    PromoSetting.ProductId = Products.Id;
                    _context.PromoSettings.Add(PromoSetting);
                }
            }

            // SINGLE SAVE CALL
            await _context.SaveChangesAsync();

            // AUDIT LOGGING
            var fullAuditMessage = new StringBuilder();
            fullAuditMessage.AppendLine($"Edited Product: {originalProduct.ProductName}");

            if (productChanges.Any())
            {
                fullAuditMessage.AppendLine("Product detail changes:");
                foreach (var change in productChanges)
                    fullAuditMessage.AppendLine($" - {change}");
            }

            if (priceChanges.Any())
            {
                fullAuditMessage.AppendLine($"Product price changes for {selectedCountryName}:");
                foreach (var change in priceChanges)
                    fullAuditMessage.AppendLine($" - {change}");
            }

            await AuditHelpers.LogAsync(HttpContext, _context, User, fullAuditMessage.ToString());

            TempData["SuccessMessage"] = "Product was updated successfully!";

            return RedirectToPage("./Index");
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
