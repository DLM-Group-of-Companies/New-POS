using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Orders
{
    [Authorize(Roles = "Admin,CS")]
    public class NewModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public NewModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public List<ProductItem> SelectedProducts { get; set; } = new();
        public List<OfficeSelectItem> OfficeList { get; set; }
        [BindProperty]
        public Order Order { get; set; } = default!;
        [BindProperty]
        public List<OrderPayment> Payments { get; set; } = new(); //Payment Methods

        public async Task<IActionResult> OnGetAsync()
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
            //ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            OfficeList = await GetUserOfficesAsync(); //Filters office by Office Assignment
            ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethods, "Name", "Name");

            var products = _context.Products.Where(p => p.IsActive)
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
                .Where(p => p.IsActive && p.ProductCategory == categoryId)
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

        public IActionResult OnGetProductList(string ProdCat, string? custclass)
        {
            ModelState.Clear();

            List<SelectListItem> SectionList = _context.Products
                .Where(p => p.IsActive && p.ProductCategory == ProdCat &&
                           (custclass == "Staff" ? p.isStaffAvailable : true))
                .Select(d => new SelectListItem
                {
                    Text = d.ProductName,
                    Value = d.Id.ToString()
                })
                .ToList();


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

        public JsonResult OnGetCustomerClass(int id)
        {
            var customer = _context.Customer
                .Include(c => c.CustClasses)
                .FirstOrDefault(c => c.Id == id);

            if (customer == null)
                return new JsonResult(new { success = false });

            return new JsonResult(new
            {
                success = true,
                custClassId = customer.CustClass,
                custClassName = customer.CustClasses.Name
            });
        }


        public JsonResult OnGetGetProducAmount(int id, string? custclass, int officeId)
        {
            var OfficeCountry = _context.OfficeCountry.FirstOrDefault(o => o.Id == officeId); //Determine the country by officeid
            var ProdAmount = _context.ProductPrices.FirstOrDefault(p => p.ProductId == id && p.CountryId == OfficeCountry.CountryId);

            if (custclass == "Staff")
            {
                return new JsonResult(ProdAmount?.StaffPrice);
            }
            else if (custclass == "Standard Distributor" || custclass == "Legacy")
            {
                return new JsonResult(ProdAmount?.DistPrice);
            }
            else if (custclass == "Over the Counter" || custclass == "Others")
            {
                return new JsonResult(ProdAmount?.RegPrice);
            }
            if (custclass == "BPP Partner")
            {
                return new JsonResult(ProdAmount?.BPPPrice);
            }
            if (custclass == "Corporate Account")
            {
                return new JsonResult(ProdAmount?.CorpAccPrice);
            }
            if (custclass == "Medical Package")
            {
                return new JsonResult(ProdAmount?.MedPackPrice);
            }
            if (custclass == "Naturopath Package")
            {
                return new JsonResult(ProdAmount?.NaturoPrice);
            }
            else
            {
                return new JsonResult(ProdAmount?.RegPrice);
            }

            //return new JsonResult(0);
        }

        private async Task LoadDropdownsAsync()
        {
            var customer = await _context.Customer
                .Select(p => new
                {
                    Id = p.Id,
                    FullName = p.CustCode + " | " + p.FirstName + " " + p.LastName
                })
                .ToListAsync();

            ViewData["CustomerId"] = new SelectList(customer, "Id", "FullName");
            //ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            OfficeList = await GetUserOfficesAsync(); //Filters office by Office Assignment
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
            ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethods, "Name", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var cart = HttpContext.Session.GetObject<List<ProductItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Cart is empty. Please add products.");
                await LoadDropdownsAsync(); // Reload select lists
                ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethods, "Name", "Name");
                return Page();
            }

            // Check inventory before proceeding
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                // Check if product is a promo bundle
                if (product != null && product.ProductCategory == "Promo")
                {
                    var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.Id == item.ProductCombo);

                    if (combo != null)
                    {
                        var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
                        var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

                        for (int i = 0; i < productIds.Count; i++)
                        {
                            int componentProductId = productIds[i];
                            int requiredQty = qtyList[i] * item.Quantity; // Multiply by how many promos were ordered

                            var inventory = await _context.InventoryStocks
                                .FirstOrDefaultAsync(i => i.ProductId == componentProductId && i.OfficeId == Order.OfficeId);

                            if (inventory == null || inventory.StockQty < requiredQty)
                            {
                                var componentProduct = await _context.Products.FindAsync(componentProductId);
                                string componentName = componentProduct?.ProductName ?? $"Product ID {componentProductId}";

                                ModelState.AddModelError("", $"Not enough stock for promo component: {componentName}. Available: {inventory?.StockQty ?? 0}, Required: {requiredQty}");
                                await LoadDropdownsAsync();
                                return Page();
                            }
                        }
                    }
                }
                else
                {
                    // Regular product stock check
                    var inventory = await _context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.OfficeId == Order.OfficeId);

                    if (inventory == null || inventory.StockQty < item.Quantity)
                    {
                        string productName = product?.ProductName ?? $"Product ID {item.ProductId}";

                        ModelState.AddModelError("", $"Not enough stock for {productName}. Available: {inventory?.StockQty ?? 0}, Requested: {item.Quantity}");
                        await LoadDropdownsAsync();
                        return Page();
                    }
                }
            }

            decimal cartTotal = cart.Sum(item => item.Price * item.Quantity);

            //// Compare with PaidAmount
            //if (Order.PaidAmount < cartTotal)
            //{
            //    ModelState.AddModelError(string.Empty, $"Paid amount (₱{Order.PaidAmount:N2}) is less than the total amount due (₱{cartTotal:N2}). Please correct the payment.");
            //    await LoadDropdownsAsync(); 
            //    return Page();
            //}

            // Setup base order data
            string orderNo = "";
            bool isUnique = false;
            var off = await _context.OfficeCountry.FirstOrDefaultAsync(o => o.Id == Order.OfficeId);

            //do //Generate Order Number and make sure it will not have duplicates incase multiple users saved record at same time
            //{                
                long ticks = DateTime.UtcNow.Ticks;
                orderNo = $"{off?.OffCode}-{ticks.ToString().Substring(0, 10)}";

            //    isUnique = !await _context.Orders.AnyAsync(o => o.OrderNo == orderNo);
            //} while (!isUnique);


            DateTime encodeDate = DateTime.UtcNow.AddHours(8);

            var oType = await _context.Customer
                .Include(c => c.CustClasses)
                .FirstOrDefaultAsync(c => c.Id == Order.CustomerId);

            string orderType = oType?.CustClasses?.Name ?? "Others";

            int itemNo = 1;

            foreach (var item in cart)
            {
                var order = new Order
                {
                    OrderNo = orderNo,
                    OrderDate = encodeDate.Date,
                    EncodeDate = encodeDate,
                    CustomerId = Order.CustomerId,
                    OfficeId = Order.OfficeId,
                    OrderType = orderType,
                    ProductCategory = item.ProductCat,
                    ProductId = item.ProductId,
                    ComboId = item.ProductCombo,
                    Qty = item.Quantity,
                    Price = item.Price,
                    Amount = item.Price * item.Quantity,
                    PaymentMethod = Order.PaymentMethod,
                    RefNo = Order.RefNo,
                    EncodedBy = User.Identity.Name,
                    PaidAmount = Payments.Sum(p => p.Amount),
                    ItemNo = itemNo++
                };


                _context.Orders.Add(order);
            }

            await _context.SaveChangesAsync();


            //Payment Methods
            var orders = new Order
            {
                OrderNo = orderNo,
                OrderDate = encodeDate.Date,
                EncodeDate = encodeDate,
                CustomerId = Order.CustomerId,
                OfficeId = Order.OfficeId,
                OrderType = orderType,
                EncodedBy = User.Identity.Name,
                PaidAmount = Payments.Sum(p => p.Amount),
                Notes = Order.Notes,
                PaymentMethod = null, // Remove if you use multiple payments
                RefNo = null // Remove if you use multiple payments
            };

            orders.Payments = Payments;
            //foreach (var payment in Payments)
            //{
                //payment.Order = orders; // optional if navigation property is configured
                                       // OR just rely on Order.Payments collection being assigned
            //}
            _context.Orders.Add(orders);
            await _context.SaveChangesAsync();

            // ✅ Update the inventory
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product != null && product.ProductCategory == "Promo")
                {
                    // Promo 
                    var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.Id == item.ProductCombo);
                    if (combo != null)
                    {
                        var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
                        var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

                        for (int i = 0; i < productIds.Count; i++)
                        {
                            int componentProductId = productIds[i];
                            int requiredQty = qtyList[i] * item.Quantity;

                            var inventory = await _context.InventoryStocks
                                .FirstOrDefaultAsync(i => i.ProductId == componentProductId && i.OfficeId == Order.OfficeId);

                            //var inventory = await _context.InventoryStocks
                            //    .FirstOrDefaultAsync(i => i.ProductId == componentProductId && i.CountryId == Order.Office.CountryId);

                            if (inventory != null)
                            {
                                inventory.StockQty -= requiredQty;
                                if (inventory.StockQty < 0) inventory.StockQty = 0;
                                _context.InventoryStocks.Update(inventory);
                            }
                        }
                    }
                }
                else
                {
                    // Regular product
                    var inventory = await _context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.OfficeId == Order.OfficeId);

                    if (inventory != null)
                    {
                        inventory.StockQty -= item.Quantity;
                        if (inventory.StockQty < 0) inventory.StockQty = 0;
                        _context.InventoryStocks.Update(inventory);
                    }
                }
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Order has been successfully submitted.";
            return RedirectToPage("./Details", new { orderNo = orderNo });
        }

        public async Task<IActionResult> OnGetGetStockAsync(int productId, int officeId)
        {
            var stock = await _context.InventoryStocks
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.OfficeId == officeId);

            return new JsonResult(stock?.StockQty ?? 0);
        }



        public class ProductInput
        {
            public int ProductId { get; set; }
            public string ProductCat { get; set; }
            public string ProductName { get; set; }
            public int? ProductCombo { get; set; }
            public string ComboName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        public class ProductItem
        {
            public int ProductId { get; set; }
            public string ProductCat { get; set; }
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
