using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Dashboard
{
    public class DashboardModel : BasePageModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int? OfficeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public List<OfficeSelectItem> OfficeList { get; set; } = new();

        public class StaffCustomerProductDto
        {
            public string CustomerName { get; set; }
            public string ProductName { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalAmount { get; set; }
        }


        public async Task OnGetAsync(int? officeId)
        {
            var office = _context.OfficeCountry
            .Include(o => o.Country).FirstOrDefault(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";
            var localNow = AuditHelpers.GetLocalTime(timeZone);
            var today = localNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            OfficeList = await GetUserOfficesAsync();

            StartDate = monthStart;
            EndDate = localNow;
        }

        public class TopKpiDto
        {
            public decimal TotalSales { get; set; }
            public int OrderCount { get; set; }
            public decimal AverageOrderValue { get; set; }
            public int NewCustomersCount { get; set; }
        }

        public async Task<JsonResult> OnGetTopKpisAsync(int? officeId)
        {
            var office = await _context.OfficeCountry
                        .Include(o => o.Country)
                        .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";
            var (startOfDayUtc, endOfDayUtc) = AuditHelpers.GetUtcDayRange(timeZone);

            // Query in UTC
            var todayOrders = await _context.Orders
                .Where(o => !o.IsVoided &&
                            o.OrderDate >= startOfDayUtc &&
                            o.OrderDate < endOfDayUtc && o.OfficeId == officeId)
                .ToListAsync();

            // Get new customers today (using EncodeDate from AuditableEntity)
            var newCustomersCount = await _context.Customer
                .Where(c => c.EncodeDate >= startOfDayUtc &&
                            c.EncodeDate < endOfDayUtc &&
                            c.OfficeId == officeId)
                .CountAsync();

            var totalSales = todayOrders.Sum(o => o.TotAmount);
            var orderCount = todayOrders.Count;
            var averageOrderValue = orderCount > 0 ? totalSales / orderCount : 0;

            var result = new TopKpiDto
            {
                TotalSales = totalSales,
                OrderCount = orderCount,
                AverageOrderValue = averageOrderValue,
                NewCustomersCount = newCustomersCount
            };

            return new JsonResult(result);
        }

        public class RecentOrderDto
        {
            public int Id { get; set; }
            public string? CustomerName { get; set; }
            public string? OfficeName { get; set; }
            public decimal Amount { get; set; }
            public string? PaymentMethod { get; set; }
            public string Status { get; set; }
            public DateTime Date { get; set; }
        }

        public async Task<JsonResult> OnGetRecentOrdersAsync(int? officeId)
        {
            var recentOrders = await _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.Payments)
                .Where(o => !o.IsVoided && o.OfficeId == officeId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var recentOrdersDto = recentOrders.Select(o => new RecentOrderDto
            {
                Id = o.Id,
                CustomerName = o.Customers != null ? o.Customers.FirstName + " " + o.Customers.LastName : "Walk-in",
                OfficeName = o.Office != null ? o.Office.Name : "",
                Amount = o.TotAmount,
                PaymentMethod = o.Payments.FirstOrDefault()?.PayMethod,
                Status = o.IsVoided ? "Cancelled" : "Completed",
                Date = o.OrderDate
            }).ToList();

            return new JsonResult(recentOrdersDto);
        }

        public class LowStockDto
        {
            public string ProductName { get; set; }
            public int? CurrentStock { get; set; }
            public int ReorderLevel { get; set; }
        }

        public async Task<JsonResult> OnGetLowStockAsync(int? officeId)
        {
            var locationsForOffice = await _context.InventoryLocations
                .Where(l => l.OfficeId == officeId && l.IsActive)
                .Select(l => l.Id)
                .ToListAsync();

            // Get warehouse (central) min levels (warehouse assumed to be LocationId = 1)
            var warehouseMinLevels = await _context.InventoryStocks
                .Where(i => i.LocationId == 1)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, MinLevel = g.First().MinLevel })
                .ToDictionaryAsync(x => x.ProductId, x => x.MinLevel);

            // Aggregate stock for the office across its locations
            var stocks = await _context.InventoryStocks
                .Include(s => s.Product)
                .Where(s => locationsForOffice.Contains(s.LocationId) && s.Product != null && s.Product.IsActive)
                .GroupBy(s => s.ProductId)
                .Select(g => new
                {
                    Product = g.First().Product,
                    ProductId = g.Key,
                    TotalStock = g.Sum(s => s.StockQty)
                    ,
                    MinLevel = g.First().MinLevel
                })
                .ToListAsync();

            var lowStock = stocks
                .Where(x =>
                {
                    if (warehouseMinLevels.TryGetValue(x.ProductId, out var min))
                    {
                        return x.TotalStock <= min;
                    }
                    // Fallback: if no warehouse min level defined, use the MinLevel found on the office stock row
                    return x.TotalStock <= x.MinLevel;
                })
                .OrderBy(x => x.TotalStock)
                .Take(10)
                .Select(x => new LowStockDto
                {
                    ProductName = x.Product.ProductName,
                    CurrentStock = x.TotalStock,
                    ReorderLevel = warehouseMinLevels.ContainsKey(x.ProductId) ? warehouseMinLevels[x.ProductId] : x.MinLevel
                })
                .ToList();

            return new JsonResult(lowStock);
        }

        public async Task<JsonResult> OnGetSalesByOfficeAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";

            DateTime startUtc, endUtc;
            if (startDate.HasValue && endDate.HasValue)
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var startLocal = startDate.Value.Date;
                var endLocal = endDate.Value.Date.AddDays(1);

                startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
            }
            else
            {
                (startUtc, endUtc) = AuditHelpers.GetUtcDayRange(timeZone);
            }

            var salesByOffice = await _context.Orders
                .Include(o => o.Office)
                .Where(o => !o.IsVoided &&
                            o.OrderDate >= startUtc &&
                            o.OrderDate < endUtc)
                .GroupBy(o => new { o.OfficeId, o.Office.Name })
                .Select(g => new
                {
                    OfficeName = g.Key.Name,
                    TotalSales = g.Sum(x => x.TotAmount)
                })
                .ToListAsync();

            return new JsonResult(salesByOffice);
        }

        public async Task<JsonResult> OnGetPaymentMethodsAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";

            DateTime startUtc, endUtc;
            if (startDate.HasValue && endDate.HasValue)
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var startLocal = startDate.Value.Date;
                var endLocal = endDate.Value.Date.AddDays(1);

                startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
            }
            else
            {
                (startUtc, endUtc) = AuditHelpers.GetUtcDayRange(timeZone);
            }

            var paymentMethods = await _context.OrderPayments
                .Include(p => p.Order)
                .Where(p => !p.Order.IsVoided &&
                            p.Order.OfficeId == officeId &&
                            p.Order.OrderDate >= startUtc &&
                            p.Order.OrderDate < endUtc)
                .GroupBy(p => p.PayMethod)
                .Select(g => new
                {
                    PaymentMethod = g.Key ?? "Unknown",
                    Count = g.Select(p => p.OrderId).Distinct().Count(),
                    Total = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            return new JsonResult(paymentMethods);
        }

        public async Task<JsonResult> OnGetOrderStatusAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";

            DateTime startUtc, endUtc;
            if (startDate.HasValue && endDate.HasValue)
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var startLocal = startDate.Value.Date;
                var endLocal = endDate.Value.Date.AddDays(1);

                startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
            }
            else
            {
                (startUtc, endUtc) = AuditHelpers.GetUtcDayRange(timeZone);
            }

            var completedCount = await _context.Orders
                .Where(o => !o.IsVoided &&
                            o.OfficeId == officeId &&
                            o.OrderDate >= startUtc &&
                            o.OrderDate < endUtc)
                .CountAsync();

            var voidedCount = await _context.Orders
                .Where(o => o.IsVoided &&
                            o.OfficeId == officeId &&
                            o.OrderDate >= startUtc &&
                            o.OrderDate < endUtc)
                .CountAsync();

            return new JsonResult(new[]
            {
                new { Status = "Completed", Count = completedCount },
                new { Status = "Cancelled", Count = voidedCount }
            });
        }

        public async Task<JsonResult> OnGetThisWeekVsLastWeekAsync(int? officeId)
        {
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";
            var localNow = AuditHelpers.GetLocalTime(timeZone);
            var today = localNow.Date;

            // Get start of this week (Monday)
            var daysToMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysToMonday < 0) daysToMonday += 7;
            var thisWeekStart = today.AddDays(-daysToMonday);
            var thisWeekEnd = thisWeekStart.AddDays(7);

            // Last week
            var lastWeekStart = thisWeekStart.AddDays(-7);
            var lastWeekEnd = thisWeekStart;

            // Convert to UTC
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var thisWeekStartUtc = TimeZoneInfo.ConvertTimeToUtc(thisWeekStart, tzInfo);
            var thisWeekEndUtc = TimeZoneInfo.ConvertTimeToUtc(thisWeekEnd, tzInfo);
            var lastWeekStartUtc = TimeZoneInfo.ConvertTimeToUtc(lastWeekStart, tzInfo);
            var lastWeekEndUtc = TimeZoneInfo.ConvertTimeToUtc(lastWeekEnd, tzInfo);

            // Get sales for both weeks
            var thisWeekSales = await _context.Orders
                .Where(o => !o.IsVoided &&
                            o.OfficeId == officeId &&
                            o.OrderDate >= thisWeekStartUtc &&
                            o.OrderDate < thisWeekEndUtc)
                .SumAsync(o => o.TotAmount);

            var lastWeekSales = await _context.Orders
                .Where(o => !o.IsVoided &&
                            o.OfficeId == officeId &&
                            o.OrderDate >= lastWeekStartUtc &&
                            o.OrderDate < lastWeekEndUtc)
                .SumAsync(o => o.TotAmount);

            return new JsonResult(new
            {
                ThisWeek = thisWeekSales,
                LastWeek = lastWeekSales,
                Change = lastWeekSales > 0 ? ((thisWeekSales - lastWeekSales) / lastWeekSales * 100) : 0
            });
        }

        public async Task<JsonResult> OnGetTopCustomersAsync(int? officeId)
        {
            var topCustomers = await _context.Orders
                .Include(o => o.Customers)
                .Where(o => !o.IsVoided && o.CustomerId != null && o.OfficeId == officeId)
                .GroupBy(o => new { o.CustomerId, o.Customers.FirstName, o.Customers.LastName })
                .Select(g => new
                {
                    CustomerName = g.Key.FirstName + " " + g.Key.LastName,
                    TotalSpent = g.Sum(x => x.TotAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            return new JsonResult(topCustomers);
        }


        public async Task<JsonResult> OnGetRepeatCustomersAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            // 1. Get the office time zone
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            //var timeZone = office?.Country.TimeZone ?? "Asia/Manila";

            //// 2. If no date filters passed, default to today in local time zone
            //DateTime startUtc, endUtc;
            //if (startDate.HasValue && endDate.HasValue)
            //{
            //    // Convert provided local dates to UTC
            //    var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            //    var startLocal = startDate.Value.Date;
            //    var endLocal = endDate.Value.Date.AddDays(1);

            //    startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
            //    endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
            //}
            //else
            //{
            //    // Use helper for today's range
            //    (startUtc, endUtc) = AuditHelpers.GetUtcDayRange(timeZone);
            //}

            // 3. Build query in UTC
            var query = _context.Orders
                .Include(o => o.Customers)
                .Where(o => !o.IsVoided &&
                            o.OfficeId == officeId &&
                            o.TotAmount > 0 &&
                            o.CustomerId != null &&
                            o.Customers != null);
            //&&
            //o.OrderDate >= startUtc &&
            //o.OrderDate < endUtc);

            // 4. Group and project
            var repeatCustomers = await query
                .GroupBy(o => new { o.CustomerId, o.Customers.FirstName, o.Customers.LastName })
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    CustomerName = g.Key.FirstName + " " + g.Key.LastName,
                    OrderCount = g.Count(),
                    LastOrder = g.Max(o => o.OrderDate)
                })
                .OrderByDescending(g => g.OrderCount)
                .ToListAsync();

            return new JsonResult(repeatCustomers);
        }


        public async Task<JsonResult> OnGetSalesChartAsync(string mode, DateTime? startDate, DateTime? endDate, int? officeId)
        {
            // 1. Get office time zone
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";

            // 2. Convert to UTC for querying
            DateTime startUtc, endUtc;

            if (startDate.HasValue && endDate.HasValue)
            {
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var startLocal = startDate.Value.Date;
                var endLocal = endDate.Value.Date.AddDays(1);

                startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
            }
            else
            {
                (startUtc, endUtc) = AuditHelpers.GetUtcDayRange(timeZone);
            }

            // 3. Base query
            var query = _context.OrderDetails
                .Include(od => od.Products)
                .Include(od => od.Order)
                .Include(od => od.ProductCombos)
                .Where(od => od.Order != null &&
                             !od.Order.IsVoided &&
                            od.Order.OfficeId == officeId &&
                             od.Order.OrderDate >= startUtc &&
                             od.Order.OrderDate < endUtc);

            if (officeId.HasValue)
                query = query.Where(od => od.Order.OfficeId == officeId);

            var rawOrderDetails = await query.ToListAsync();

            // 4. Aggregate product sales
            var productSales = new Dictionary<string, decimal>();
            List<object> data = new();

            if (mode == "regular")
            {
                foreach (var od in rawOrderDetails)
                {
                    if (od.ProductId != null)
                    {
                        var product = od.Products;
                        if (product != null && product.IsActive && product.ProductCategory != "Package")
                        {
                            var name = product.ProductName;
                            if (!productSales.ContainsKey(name))
                                productSales[name] = 0;

                            productSales[name] += od.TotalPrice;
                        }
                    }
                    else if (od.ComboId != null && od.ProductCombos?.IsActive == true)
                    {
                        var combo = od.ProductCombos;
                        if (!string.IsNullOrEmpty(combo.ProductIdList))
                        {
                            var productIds = combo.ProductIdList
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => int.Parse(id.Trim()))
                                .ToList();

                            var products = await _context.Products
                                .Where(p => productIds.Contains(p.Id) &&
                                            p.IsActive &&
                                            p.ProductCategory != "Package")
                                .ToListAsync();

                            if (products.Count > 0)
                            {
                                var perProductPrice = od.TotalPrice / products.Count;
                                foreach (var p in products)
                                {
                                    var name = p.ProductName;
                                    if (!productSales.ContainsKey(name))
                                        productSales[name] = 0;

                                    productSales[name] += perProductPrice;
                                }
                            }
                        }
                    }
                }
                data = productSales
               .Select(kv => new { ProductName = kv.Key, TotalSales = kv.Value })
               .OrderByDescending(x => x.TotalSales)
               .Cast<object>()
               .ToList();
            }
            else //Promo/Package
            {
                var packageSales = await _context.OrderDetails
                    .Where(od => od.ProductCategory == "Package" &&
                             !od.Order.IsVoided &&
                            od.Order.OfficeId == officeId &&
                             od.Order.OrderDate >= startUtc &&
                             od.Order.OrderDate < endUtc)
                    .GroupBy(od => od.Products.ProductName)
                    .Select(g => new
                    {
                        ProductName = g.Key,
                        TotalSales = g.Sum(od => od.Price)
                    })
                    .Cast<object>()
                    .ToListAsync();

                data = packageSales;
            }


            return new JsonResult(data);
        }

        public JsonResult OnGetTopSalespeople(int? officeId, DateTime? endDate, string? type)
        {
            if (!endDate.HasValue)
                return new JsonResult(new { error = "End Date required" });

            List<object> topSalespeople;

            if (type == "YearToDate")
            {
                topSalespeople = _context.Orders
                    .Where(o => o.OfficeId == officeId &&
                                o.OrderDate.Year == endDate.Value.Year)
                    .GroupBy(o => o.SalesBy)
                    .Select(g => new
                    {
                        Salesperson = g.Key,
                        TotalSales = g.Sum(o => o.TotAmount)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .Take(10)
                    .ToList<object>();
            }
            else
            {
                topSalespeople = _context.Orders
                    .Where(o => o.OfficeId == officeId &&
                                o.OrderDate.Year == endDate.Value.Year &&
                                o.OrderDate.Month == endDate.Value.Month)
                    .GroupBy(o => o.SalesBy)
                    .Select(g => new
                    {
                        Salesperson = g.Key,
                        TotalSales = g.Sum(o => o.TotAmount)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .Take(10)
                    .ToList<object>();
            }

            return new JsonResult(topSalespeople);
        }


        //public List<string> Labels { get; set; } = new(); // months like ["2025-06", "2025-07"]
        //public List<object> Datasets { get; set; } = new();

        public async Task<JsonResult> OnGetCountPerProductAsync(int year, string? type)
        {
            var query = _context.OrderDetails
                .Where(od => !od.Order.IsVoided && od.ProductId != null && od.Order.OrderDate.Year == year);

            if (!string.IsNullOrEmpty(type))
            {
                if (type == "Regular")
                    query = query.Where(od => od.Products.ProductCategory == "Regular");
                else if (type == "Promo")
                    query = query.Where(od => od.Products.ProductCategory.Contains("Package"));
            }

            var raw = await query
                .GroupBy(od => new
                {
                    od.Order.OrderDate.Year,
                    od.Order.OrderDate.Month,
                    od.ProductId,
                    od.Products.ProductName
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return new JsonResult(raw);
        }



        public async Task<JsonResult> OnGetSalesTrendAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            // 1. Get the office time zone
            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";

            // 2. If no date filters passed, default to today in local time zone
            DateTime startUtc, endUtc;
            if (startDate.HasValue && endDate.HasValue)
            {
                // Convert provided local dates to UTC
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var startLocal = startDate.Value.Date;
                var endLocal = endDate.Value.Date.AddDays(1);

                startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
            }
            else
            {
                // Use helper for today's range
                (startUtc, endUtc) = AuditHelpers.GetUtcDayRange(timeZone);
            }

            var query = _context.Orders
                .Where(o => !o.IsVoided && o.OfficeId == officeId);

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endUtc);
            }

            var salesByDayRaw = await query
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(o => o.TotAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Format Date to string AFTER pulling from DB
            var salesByDay = salesByDayRaw.Select(x => new
            {
                Date = x.Date.ToString("MM-dd"),
                x.Total
            });

            return new JsonResult(salesByDay);
        }

        public async Task<JsonResult> OnGetStaffCustomerProductsAsync(
    DateTime? startDate,
    DateTime? endDate,
    int? officeId)
        {
            if (!startDate.HasValue || !endDate.HasValue)
                return new JsonResult(new List<object>());

            var office = await _context.OfficeCountry
                .Include(o => o.Country)
                .FirstOrDefaultAsync(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startDate.Value.Date, tzInfo);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endDate.Value.Date.AddDays(1), tzInfo);

            var data = await _context.OrderDetails
     .Where(od =>
         !od.Order.IsVoided &&
         od.Order.OfficeId == officeId &&
         od.Order.OrderDate >= startUtc &&
         od.Order.OrderDate < endUtc &&
         od.Order.Customers != null &&
         od.ProductCategory != "Freebie" &&
         od.Order.Customers.CustClasses.Name == "Staff")
     .Select(od => new
     {
         CustomerId = od.Order.CustomerId,
         CustomerName = od.Order.Customers.FirstName + " " + od.Order.Customers.LastName,
         ProductName = od.Products.ProductName,
         Quantity = od.Quantity,
         LineTotal = od.Quantity * od.Price
     })
     .GroupBy(x => new
     {
         x.CustomerId,
         x.CustomerName,
         x.ProductName
     })
     .Select(g => new StaffCustomerProductDto
     {
         CustomerName = g.Key.CustomerName,
         ProductName = g.Key.ProductName,
         TotalQuantity = g.Sum(x => x.Quantity),
         TotalAmount = g.Sum(x => x.LineTotal)
     })
     .OrderBy(x => x.CustomerName).ThenBy(x => x.ProductName)
     .ToListAsync();



            return new JsonResult(data);
        }

    }
}
