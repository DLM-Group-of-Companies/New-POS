using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public List<ProductItem> SelectedProducts { get; set; } = new();

        public IActionResult OnGet()
        {
            SelectedProducts = HttpContext.Session.GetObject<List<ProductItem>>("Cart") ?? new List<ProductItem>();

            var customer = _context.Customer
                .Select(p => new
                {
                    Id = p.Id,
                    FullName = p.CustCode + " | " + p.FirstName + " " + p.LastName
                })
                .ToList();

            ViewData["CustomerId"] = new SelectList(customer, "Id", "FullName");
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");

            var products = _context.Products
            .Select(p => new { p.Id, p.ProductName })
            .ToList();

            // Insert default item at the top
            products.Insert(0, new { Id = 0, ProductName = "-- Select --" });

            ViewData["ProductId"] = new SelectList(products, "Id", "ProductName");

            //ViewData["ProductCombos"] = new SelectList(_context.ProductCombos, "Id", "ProductsDesc");
            return Page();
        }

        public JsonResult OnGetProductsByCategory(string categoryId)
        {
            var products = _context.Products
                .Where(p => p.ProductCategory == categoryId)
                .Select(p => new { id = p.Id, name = p.ProductName })
                .ToList();

            return new JsonResult(products);
        }

        public IActionResult OnGetCustomers()
        {

            List<SelectListItem> SectionList = (from c in _context.Customer.Include(c => c.CustClasses)
                                                select new SelectListItem
                                                {
                                                    Text = c.CustCode + " | " + c.FirstName + " " + c.LastName,
                                                    Value = c.Id.ToString()
                                                }).ToList();

            return new JsonResult(SectionList);
        }

        public IActionResult OnGetProductList(string ProdCat)
        {
            ModelState.Clear();

            List<SelectListItem> SectionList = (from d in _context.Products.Where(p => p.ProductCategory == ProdCat)
                                                select new SelectListItem
                                                {
                                                    Text = d.ProductName,
                                                    Value = d.Id.ToString()
                                                }).ToList();

            //SectionList.Insert(0, new SelectListItem { Text = "--Select Product--", Value = "" });
            if (SectionList.Count == 0)
            {
                SectionList.Insert(0, new SelectListItem { Text = "No Product Available", Value = "" });
            }
            else
            {
                SectionList.Insert(0, new SelectListItem { Text = "--Select--", Value = "" });
            }
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
                SectionList.Insert(0, new SelectListItem { Text = "No Combination Available", Value = "" });
            }
            else
            {
                SectionList.Insert(0, new SelectListItem { Text = "--Select--", Value = "" });
            }


            return new JsonResult(SectionList);
        }

        public JsonResult OnGetGetProducAmount(int id)
        {
            //ViewData["ProductCombos"] = new SelectList(_context.ProductCombos.Where(c=>c.ProductId==id), "Id", "ProductsDesc");

            var ProdAmount = _context.Products.FirstOrDefault(p => p.Id == id);

            return new JsonResult(ProdAmount.RegPrice);

        }

        [BindProperty]
        public Order Order { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {

            var cart = HttpContext.Session.GetObject<List<ProductItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Cart is empty. Please add products.");
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
            long ticks = DateTime.Now.Ticks;
            Order.OrderNo = $"MLAHQ-{ticks.ToString().Substring(0, 10)}";
            Order.EncodeDate = DateTime.UtcNow.AddHours(8);

            var oType = await _context.Customer.Include(c => c.CustClasses).FirstOrDefaultAsync(c => c.Id == Order.CustomerId);
            Order.OrderType = oType.CustClasses.Name;

            ModelState.Remove("Order.OrderNo");
            ModelState.Remove("Order.EncodeDate");
            ModelState.Remove("Order.Office");
            ModelState.Remove("Order.CustClasses");
            ModelState.Remove("Order.Products");
            ModelState.Remove("Order.ProductCombos");
            ModelState.Remove("Order.Office");
            ModelState.Remove("Order.CustClasses");
            ModelState.Remove("Order.Customers");
            ModelState.Remove("Order.OrderType");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            int i = cart.Count();
            for (int j = 0; j < i; j++)
            {
                Order.ProductId = cart[j].ProductId;
                Order.ComboId = cart[j].ProductCombo;
                Order.Qty = cart[j].Quantity;
                Order.Price = cart[j].Price;
                Order.Amount = Order.Price * Order.Qty;
                Order.ItemNo = j + 1;
                _context.Orders.Add(Order);
                await _context.SaveChangesAsync();
                Order.Id = Order.Id + 1;
            }
            //_context.Orders.Add(Order);
            //await _context.SaveChangesAsync();

            //// ✅ Deduct stock here
            //var product = await _context.Products.FindAsync(cart[j].ProductId);
            //if (product != null)
            //{
            //    product.StockQuantity -= cart[j].Quantity;
            //    if (product.StockQuantity < 0) product.StockQuantity = 0; // optional safeguard
            //    _context.Products.Update(product);
            //    await _context.SaveChangesAsync();
            //}

            HttpContext.Session.Remove("Cart");
            return RedirectToPage("./Index");
        }

        public class ProductInput
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int? ProductCombo { get; set; }
            public string ComboName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        public class ProductItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int? ProductCombo { get; set; }
            public string ComboName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        public IActionResult OnPostSyncCart([FromBody] List<ProductItem> updatedCart)
        {
            HttpContext.Session.SetObject("Cart", updatedCart);
            return new JsonResult(new { success = true });
        }

        public IActionResult OnPostRemoveProduct([FromBody] int productId)
        {
            var cart = HttpContext.Session.GetObject<List<ProductItem>>("Cart") ?? new();
            cart = cart.Where(p => p.ProductId != productId).ToList();
            HttpContext.Session.SetObject("Cart", cart);
            return new JsonResult(new { success = true });
        }

        public IActionResult OnPostAddProduct([FromBody] ProductInput input)
        {
            var sessionCart = HttpContext.Session.GetObject<List<ProductItem>>("Cart") ?? new List<ProductItem>();

            var existing = sessionCart.FirstOrDefault(p => p.ProductId == input.ProductId);
            if (existing != null)
            {
                existing.Quantity += input.Quantity;
            }
            else
            {
                sessionCart.Add(new ProductItem
                {
                    ProductId = input.ProductId,
                    ProductName = input.ProductName,
                    ProductCombo = input.ProductCombo,
                    Price = input.Price,
                    Quantity = input.Quantity
                });
            }

            HttpContext.Session.SetObject("Cart", sessionCart);
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostSaveOrderAsync()
        {
            var sessionCart = HttpContext.Session.GetObject<List<ProductItem>>("Cart");

            if (sessionCart == null || !sessionCart.Any())
                return BadRequest();

            // TODO: Save to your DB here (create Order, then OrderItems)

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            //return RedirectToPage("./Index");


            return new JsonResult(new { success = true });
        }

        public JsonResult OnGetSearchCustomer(string term)
        {
            //Search Customers via Modal
            var results = _context.Customer
                .Where(c => c.FirstName.Contains(term) || c.LastName.Contains(term) || c.CustCode.Contains(term) || c.MobileNo.Contains(term) || c.Email.Contains(term))
                .Select(c => new
                {
                    id = c.Id,
                    custcode = c.CustCode,
                    name = c.FirstName + " " + c.LastName,
                    email = c.Email,
                    phone = c.MobileNo
                })
                .Take(10)
                .ToList();

            return new JsonResult(results);
        }

    }
}

