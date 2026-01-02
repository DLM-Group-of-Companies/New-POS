using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data; // your namespace
using NLI_POS.Models; // your namespace

namespace NLI_POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/salesapi/today
        [HttpGet("today")]
        public async Task<IActionResult> GetTodaySales()
        {
            var today = DateTime.UtcNow.Date;

            var total = await _context.Orders
                .Where(o => o.OrderDate >= today &&
                            o.OrderDate < today.AddDays(1) &&
                            !o.IsVoided)
                .SumAsync(o => (decimal?)o.TotAmount) ?? 0;

            return Ok(new
            {
                date = today,
                totalSales = total
            });
        }
    }
}
