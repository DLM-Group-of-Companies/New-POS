using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using static NLI_POS.Services.BasePageModel;

namespace NLI_POS.Pages.Report
{

    public class SalesPerProductModel : BasePageModel
    {
        private readonly Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public SalesPerProductModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<OfficeSelectItem> OfficeList { get; set; }

        public async Task OnGetAsync()
        {
            OfficeList = await GetUserOfficesAsync();
        }

        public async Task<JsonResult> OnGetSalesChartAsync(DateTime? startDate, DateTime? endDate, int? officeId)
        {
            var query = _context.OrderDetails
                .Include(od => od.Products)
                .Include(od => od.Order)
                .Where(od => od.Products != null &&
                             od.Products.IsActive &&
                             od.Order != null &&
                             !od.Order.IsVoided);

            if (startDate.HasValue)
                query = query.Where(od => od.Order.OrderDate >= startDate.Value);

            if (endDate.HasValue)
            {
                // Make endDate inclusive by adding one day
                var nextDay = endDate.Value.AddDays(1);
                query = query.Where(od => od.Order.OrderDate < nextDay);
            }

            if (officeId.HasValue)
                query = query.Where(od => od.Order.OfficeId == officeId);

            // Move to memory before using TotalPrice (which is likely a calculated property)
            var data = query.AsEnumerable()
                .GroupBy(od => od.Products.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSales = g.Sum(x => x.TotalPrice) // Safe now because it's in memory
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return new JsonResult(data);
        }


    }
}
