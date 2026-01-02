using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Migrations;
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
        public Order Order { get; set; } = new();

        [BindProperty]
        public List<OrderPayment> Payments { get; set; } = new();

        [BindProperty]
        public List<OrderDetails> OrderDetails { get; set; } = new();

        [BindProperty]
        public string GrandTotal { get; set; }

        public class PaymentMethodVM
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal? ServiceCharge { get; set; }
        }

        [BindProperty]
        public decimal ServiceChargeValue { get; set; }

        public List<SelectListItem> SalesPersons { get; set; }

        [BindProperty]
        public string OfficeTimeZone { get; set; } 
        
        [BindProperty]
        public string OfficeLocale { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedOfficeId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? officeId )
        {
            //Reset cart
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("Payments");

            SelectedProducts = HttpContext.Session.GetObject<List<ProductItem>>("Cart") ?? new List<ProductItem>();

            ViewData["CustomerId"] = new SelectList(_context.Customer.OrderByDescending(c => c.Id)
                .Take(30)
                .Select(c => new
                {
                    c.Id,
                    FullName = c.CustCode + " | " + c.FirstName + " " + c.LastName
                }), "Id", "FullName");

            OfficeList = await GetUserOfficesAsync(); //Filters office by Office Assignment

            SelectedOfficeId = officeId;

            ViewData["PaymentMethod"] = _context.PaymentMethods
                .Where(p => p.IsActive)
                .Select(p => new PaymentMethodVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    ServiceCharge = p.ServiceCharge
                })
                .ToList();

            var usersInSales = await _userManager.GetUsersInRoleAsync("Sales");

            SalesPersons = usersInSales.Select(u => new SelectListItem
            {
                Value = u.UserName,
                Text = u.FullName ?? u.UserName
            }).ToList();
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

        public IActionResult OnGetProductList(string ProdCat, string? custclass, string? OfficeTimeZone)
        {
            //ModelState.Clear();

            bool isFreebieCat = ProdCat?.Trim() == "Freebie";
            bool isStaff = custclass?.Trim() == "Staff";

            //var productsQuery = _context.Products.AsQueryable();
            var productsQuery = _context.Products
    .Include(p => p.PromoSetting) // To check if Promo/Package is on going or by date
    .Where(p => p.IsActive);

            //productsQuery = productsQuery.Where(p => p.IsActive);

            if (isFreebieCat)
            {
                productsQuery = productsQuery.Where(p => p.isFreebieAvailable == true);
            }
            else
            {
                productsQuery = productsQuery.Where(p => p.ProductCategory == ProdCat);
            }

            if (isStaff)
            {
                productsQuery = productsQuery.Where(p => p.isStaffAvailable == true);
            }

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
     TimeZoneInfo.FindSystemTimeZoneById(OfficeTimeZone)); // from office dropdown

            // ✅ Filter Promo Packages if applicable
            if (ProdCat?.Trim() == "Package")
            {
                productsQuery = productsQuery.Where(p =>
                    p.PromoSetting == null || (
                        (p.PromoSetting.IsOngoing) ||
                        (p.PromoSetting.StartDate <= nowLocal && p.PromoSetting.EndDate >= nowLocal)
                    )
                );
            }

            var SectionList = productsQuery
                .Select(p => new SelectListItem
                {
                    Text = p.ProductName,
                    Value = p.Id.ToString()
                })
                .ToList();

            if (SectionList.Count == 0)
            {
                SectionList.Insert(0, new SelectListItem { Text = "No Product Available", Value = "" });
            }
            else
            {
                SectionList.Insert(0, new SelectListItem { Text = "-- Select --", Value = "" });
            }

            return new JsonResult(SectionList);
        }


        public IActionResult OnGetProductComboList(int ProdId)
        {
            List<SelectListItem> SectionList = (from d in _context.ProductCombos.Where(p => p.ProductId == ProdId && p.IsActive)
                                                select new SelectListItem
                                                {
                                                    Text = d.ProductsDesc,
                                                    Value = d.Id.ToString()
                                                }).ToList();

            if (SectionList.Count == 0)
            {
                SectionList.Insert(0, new SelectListItem { Text = "Not Available", Value = "" });
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
            //if (custclass == "Naturopath Package")
            if (custclass.StartsWith("Naturopath"))
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
            //ViewData["PaymentMethod"] = new SelectList(_context.PaymentMethods, "Name", "Name");
            ViewData["PaymentMethod"] = _context.PaymentMethods
                .Where(p => p.IsActive)
                .Select(p => new PaymentMethodVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    ServiceCharge = p.ServiceCharge
                })
                .ToList();

        }

        public IActionResult OnPostClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            OfficeList = await GetUserOfficesAsync();

            ViewData["PaymentMethod"] = _context.PaymentMethods
    .Where(p => p.IsActive)
    .Select(p => new PaymentMethodVM
    {
        Id = p.Id,
        Name = p.Name,
        ServiceCharge = p.ServiceCharge
    })
    .ToList();

            var cart = HttpContext.Session.GetObject<List<ProductItem>>("Cart");

            // Check inventory before proceeding
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                // Check if product is a package bundle
                if (product != null && product.ProductCategory == "Package")
                {
                    var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.Id == item.ProductCombo);

                    if (combo != null)
                    {
                        var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
                        var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

                        for (int i = 0; i < productIds.Count; i++)
                        {
                            int componentProductId = productIds[i];
                            int requiredQty = qtyList[i] * item.Quantity; // Multiply by how many packages were ordered

                            var inventory = await _context.InventoryStocks
                                .FirstOrDefaultAsync(i => i.ProductId == componentProductId && i.Location.OfficeId == Order.OfficeId);

                            if (inventory == null || inventory.StockQty < requiredQty)
                            {
                                var componentProduct = await _context.Products.FindAsync(componentProductId);
                                string componentName = componentProduct?.ProductName ?? $"Product ID {componentProductId}";

                                ModelState.AddModelError("", $"Not enough stock for package component: {componentName}. Available: {inventory?.StockQty ?? 0}, Required: {requiredQty}");
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
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.Location.OfficeId == Order.OfficeId);

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

            // Setup base order data
            string orderNo = "";
            //bool isUnique = false;
            var off = await _context.OfficeCountry.FirstOrDefaultAsync(o => o.Id == Order.OfficeId);

            //do //Generate Order Number and make sure it will not have duplicates incase multiple users saved record at same time
            //{                
            long ticks = DateTime.UtcNow.Ticks;
            orderNo = $"{off?.OffCode}-{ticks.ToString().Substring(0, 10)}";

            //    isUnique = !await _context.Orders.AnyAsync(o => o.OrderNo == orderNo);
            //} while (!isUnique);


            DateTime encodeDate = DateTime.UtcNow;

            var oType = await _context.Customer
                .Include(c => c.CustClasses)
                .FirstOrDefaultAsync(c => c.Id == Order.CustomerId);

            string orderType = oType?.CustClasses?.Name ?? "Others";

            // Parent Order
            var order = new Order
            {
                OrderNo = orderNo,
                OrderDate = encodeDate,
                EncodeDate = encodeDate,
                CustomerId = Order.CustomerId,
                OfficeId = Order.OfficeId,
                OrderType = orderType,
                EncodedBy = User.Identity.Name,
                TotAmount = Order.TotAmount,
                TotPaidAmount = Payments.Sum(p => p.Amount) + ServiceChargeValue,
                Notes = Order.Notes,
                SalesBy = Order.SalesBy
            };

            // Attach Product Items
            int itemNo = 1;
            order.ProductItems = cart.Select(item => new ProductItem
            {
                ProductId = item.ProductId,
                ProductCat = item.ProductCat,
                ProductName = item.ProductName,
                ProductCombo = item.ProductCombo,
                ComboName = item.ComboName,
                Price = item.Price,
                Quantity = item.Quantity,
                Amount = item.Price * item.Quantity,
                ServiceChargeAmount = item.ServiceChargeAmount,
                ServiceChargePct = item.ServiceChargePct,
                ItemNo = itemNo++
            }).ToList();

            // Attach Payments
            order.Payments = Payments;

            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            // Save entire order 
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Save OrderDetails (normalized version of product items)
            foreach (var item in order.ProductItems)
            {
                var detail = new OrderDetails
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ComboId = item.ProductCombo,
                    ProductCategory = item.ProductCat,
                    Price = item.Price,
                    Quantity = item.Quantity
                    // No need to assign TotalPrice; it's a computed property
                };

                _context.OrderDetails.Add(detail);
            }
            await _context.SaveChangesAsync();


            // ✅ Update the inventory
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product != null && product.ProductCategory == "Package")
                {
                    // Package 
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
                                .FirstOrDefaultAsync(i => i.ProductId == componentProductId && i.Location.OfficeId == Order.OfficeId);

                            if (inventory != null)
                            {
                                inventory.StockQty -= requiredQty;
                                if (inventory.StockQty < 0) inventory.StockQty = 0;
                                _context.InventoryStocks.Update(inventory);

                                //Log Inventory Trans
                                _context.InventoryTransactions.Add(new InventoryTransaction
                                {
                                    OrderNo = order.OrderNo,
                                    ProductId = componentProductId,
                                    FromLocationId = inventory.LocationId,
                                    ToLocationId = null,
                                    Quantity = requiredQty,
                                    TransactionType = "Product OUT",
                                    TransactionDate = DateTime.UtcNow,
                                    EncodedBy = User.Identity?.Name ?? "SYSTEM"
                                });



                            }
                        }
                    }
                }
                else
                {
                    // Regular product
                    var inventory = await _context.InventoryStocks
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.Location.OfficeId == Order.OfficeId);

                    if (inventory != null)
                    {
                        inventory.StockQty -= item.Quantity;
                        if (inventory.StockQty < 0) inventory.StockQty = 0;
                        _context.InventoryStocks.Update(inventory);

                        //Log Inventory Trans
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            OrderNo = order.OrderNo,
                            ProductId = item.ProductId,
                            FromLocationId = inventory.LocationId,
                            ToLocationId = null,
                            Quantity = item.Quantity,
                            TransactionType = "Product OUT",
                            TransactionDate = DateTime.UtcNow,
                            EncodedBy = User.Identity?.Name ?? "SYSTEM"
                        });

                    }
                }
            }

            await _context.SaveChangesAsync();

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Submitted Order {orderNo}");

            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Order has been successfully submitted.";
            return RedirectToPage("./Details", new { orderNo = orderNo });
        }

        //Verify Stocks
        //public async Task<IActionResult> OnGetGetStockAsync(int productId, int officeId)
        //{
        //    // Get the product to determine if it's Package or Regular
        //    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

        //    if (product == null)
        //    {
        //        return new JsonResult(0);
        //    }

        //    if (product.ProductCategory == "Package")
        //    {
        //        // It's a Package item — find the corresponding combo
        //        var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.ProductId == productId);

        //        if (combo == null)
        //        {
        //            return new JsonResult(0); // No combo found
        //        }

        //        var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
        //        var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

        //        int minBundles = int.MaxValue;

        //        for (int i = 0; i < productIds.Count; i++)
        //        {
        //            int componentId = productIds[i];
        //            int requiredQtyPerBundle = qtyList[i];

        //            var stock = await _context.InventoryStocks
        //                .FirstOrDefaultAsync(s => s.ProductId == componentId && s.Location.OfficeId == officeId);

        //            int available = stock?.StockQty ?? 0;

        //            // Determine how many full bundles can be made based on this component
        //            int bundles = available / requiredQtyPerBundle;

        //            if (bundles < minBundles)
        //                minBundles = bundles;
        //        }

        //        return new JsonResult(minBundles);
        //    }
        //    else
        //    {
        //        // Regular product
        //        var stock = await _context.InventoryStocks
        //            .FirstOrDefaultAsync(s => s.ProductId == productId && s.Location.OfficeId == officeId);

        //        return new JsonResult(stock?.StockQty ?? 0);
        //    }
        //}
        public async Task<IActionResult> OnGetGetStockAsync(int productId, int? comboId, int officeId, int qty)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return new JsonResult(new { success = false, message = "Product not found" });
            }

            if (product.ProductCategory == "Package")
            {
                var combo = await _context.ProductCombos.FirstOrDefaultAsync(c => c.ProductId == productId && c.IsActive);

                if (combo == null)
                {
                    return new JsonResult(new { success = false, message = "No product found for this package." });
                }

                var productIds = combo.ProductIdList.Split(',').Select(int.Parse).ToList();
                var qtyList = combo.QuantityList.Split(',').Select(int.Parse).ToList();

                var insufficientItems = new List<object>();
                int minBundles = int.MaxValue;

                //for (int i = 0; i < productIds.Count; i++)
                //{
                //    int componentId = productIds[i];
                //    int requiredQtyPerBundle = qtyList[i];
                //    int totalRequired = requiredQtyPerBundle * qty;

                //    var stock = await _context.InventoryStocks
                //        .Include(s => s.Product)
                //        .FirstOrDefaultAsync(s => s.ProductId == componentId && s.Location.OfficeId == officeId);

                //    int available = stock?.StockQty ?? 0;

                //    if (available < totalRequired)
                //    {
                //        insufficientItems.Add(new
                //        {
                //            name = stock?.Product?.ProductName ?? $"Product ID {componentId}",
                //            available,
                //            required = totalRequired
                //        });
                //    }

                //    // Optional: Compute how many bundles can be made from this item
                //    int bundles = available / requiredQtyPerBundle;
                //    if (bundles < minBundles)
                //        minBundles = bundles;
                //}

                for (int i = 0; i < productIds.Count; i++)
                {
                    int componentId = productIds[i];
                    int requiredQtyPerBundle = qtyList[i];

                    var stock = await _context.InventoryStocks
                        .Include(s => s.Product)
                        .FirstOrDefaultAsync(s => s.ProductId == componentId && s.Location.OfficeId == officeId);

                    int available = stock?.StockQty ?? 0;
                    int bundles = available / requiredQtyPerBundle;

                    if (bundles < minBundles)
                        minBundles = bundles;

                    if (available < requiredQtyPerBundle)
                    {
                        string productName = stock?.Product?.ProductName;

                        if (string.IsNullOrEmpty(productName))
                        {
                            var prod = await _context.Products.FindAsync(componentId);
                            productName = prod?.ProductName ?? $"Product ID {componentId}";
                        }

                        insufficientItems.Add(new
                        {
                            name = productName,
                            available,
                            required = requiredQtyPerBundle
                        });
                    }
                }


                return new JsonResult(new
                {
                    success = insufficientItems.Count == 0,
                    isPackage = true,
                    availableBundles = minBundles,
                    insufficientItems
                });
            }
            else
            {
                // Regular product
                var stock = await _context.InventoryStocks
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.Location.OfficeId == officeId);

                int available = stock?.StockQty ?? 0;

                return new JsonResult(new
                {
                    success = available >= qty,
                    isPackage = false,
                    available
                });
            }
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
            public int Id { get; set; }

            public int OrderId { get; set; }                 // Foreign key
            public virtual Order Order { get; set; }         // Navigation property

            public int ProductId { get; set; }
            public string ProductCat { get; set; }
            //public string ProductClass { get; set; }
            public string ProductName { get; set; }
            public int? ProductCombo { get; set; }
            public string ComboName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Amount { get; set; }
            public decimal ServiceChargeAmount { get; set; } = 0;
            public decimal ServiceChargePct { get; set; } = 0;

            public int ItemNo { get; set; } // optional
            public virtual Product Product { get; set; }
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
                OfficeList = await GetUserOfficesAsync();
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
                .Where(c => (c.FirstName + " " + c.LastName).Contains(term) ||  // Full name
                    c.FirstName.Contains(term) || c.LastName.Contains(term) || c.CustCode.Contains(term) ||
                    c.MobileNo.Contains(term) || c.Email.Contains(term))
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
