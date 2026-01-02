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
        }

        public async Task<JsonResult> OnGetTopKpisAsync(int? officeId)
        {
            //var today = DateTime.UtcNow.Date;
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

            var totalSales = todayOrders.Sum(o => o.TotAmount);
            var orderCount = todayOrders.Count;
            var averageOrderValue = orderCount > 0 ? totalSales / orderCount : 0;

            var result = new TopKpiDto
            {
                TotalSales = totalSales,
                OrderCount = orderCount,
                AverageOrderValue = averageOrderValue
            };

            return new JsonResult(result);
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
                .GroupBy(od => new {
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
    }
}
