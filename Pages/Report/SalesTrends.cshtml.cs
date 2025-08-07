using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;

namespace NLI_POS.Pages.Report
{
    public class SalesTrendsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SalesTrendsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JsonResult> OnGetSalesTrendAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Where(o => !o.IsVoided);

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
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
                Date = x.Date.ToString("yyyy-MM-dd"),
                x.Total
            });

            return new JsonResult(salesByDay);
        }
    }

}
