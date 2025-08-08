using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Report
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? OfficeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public List<OfficeCountry> OfficeList { get; set; } = new();


        public void OnGet()
        {
        }

        public class TopKpiDto
        {
            public decimal TotalSales { get; set; }
            public int OrderCount { get; set; }
            public decimal AverageOrderValue { get; set; }
        }

        public async Task<JsonResult> OnGetTopKpisAsync()
        {
            var today = DateTime.UtcNow.Date;

            var todayOrders = await _context.Orders
                .Where(o => !o.IsVoided && o.OrderDate.Date == today)
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

        public async Task<JsonResult> OnGetTopCustomersAsync()
        {
            var topCustomers = await _context.Orders
                .Include(o => o.Customers)
                .Where(o => !o.IsVoided && o.CustomerId != null)
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


        public async Task<JsonResult> OnGetRepeatCustomersAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Include(o => o.Customers)
                .Where(o => !o.IsVoided && o.TotAmount>0 && o.CustomerId != null && o.Customers != null);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

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

        public async Task<JsonResult> OnGetSalesChartAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            var query = _context.OrderDetails
                .Include(od => od.Products)
                .Include(od => od.Order)
                .Include(od => od.ProductCombos)
                .Where(od => od.Order != null && !od.Order.IsVoided);

            if (startDate.HasValue)
                query = query.Where(od => od.Order.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(od => od.Order.OrderDate < endDate.Value.AddDays(1));

            if (officeId.HasValue)
                query = query.Where(od => od.Order.OfficeId == officeId);

            var rawOrderDetails = await query.ToListAsync();

            // We'll collect product sales here
            var productSales = new Dictionary<string, decimal>();

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
                else if (od.ComboId != null && od.ProductCombos != null && od.ProductCombos.IsActive)
                {
                    var combo = od.ProductCombos;

                    if (!string.IsNullOrEmpty(combo.ProductIdList))
                    {
                        var productIds = combo.ProductIdList
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => int.Parse(id.Trim()))
                            .ToList();

                        // Fetch only regular products
                        var products = await _context.Products
                            .Where(p => productIds.Contains(p.Id) && p.IsActive && p.ProductCategory != "Package")
                            .ToListAsync();

                        // Distribute price evenly across regular products only
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

            var data = productSales
                .Select(kv => new { ProductName = kv.Key, TotalSales = kv.Value })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return new JsonResult(data);
        }

        public JsonResult OnGetTopSalespeople()
        {
            var topSalespeople = _context.Orders
                .GroupBy(o => o.SalesBy)
                .Select(g => new
                {
                    Salesperson = g.Key,
                    TotalSales = g.Sum(o => o.TotAmount)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(10) // Top 10 performers
                .ToList();

            return new JsonResult(topSalespeople);
        }

    }
}
