using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Models.Dtos;

namespace NLI_POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET api/orders/salesby/{username}
        [HttpGet("salesby/{salesEmail}")]
        public async Task<IActionResult> GetOrdersBySales(
    string salesEmail,
    int year,
    int? month,
    string cntryCd)
        {
            if (string.IsNullOrEmpty(salesEmail))
                return BadRequest("Email is required");

            var user = await _userManager.FindByEmailAsync(salesEmail);
            if (user == null) return NotFound("User not found");

            var orders = await GetSalesData(year, month, cntryCd, user.UserName);
            if (orders == null) return BadRequest("Invalid country or date parameters");

            return Ok(orders);
        }

        // GET api/orders/salesby
        [HttpGet("salesby")]
        public async Task<IActionResult> GetSalesBy(
    int year,
    int? month,
    string cntryCd)
        {
            var orders = await GetSalesData(year, month, cntryCd);
            if (orders == null) return BadRequest("Invalid country or date parameters");

            return Ok(orders);
        }

        private async Task<List<OrderSummaryDto>?> GetSalesData(int year, int? month, string cntryCd, string? userName = null)
        {
            if (string.IsNullOrEmpty(cntryCd)) return null;

            // Get office and its timezone
            var country = await _context.Country.FirstOrDefaultAsync(c => c.Code == cntryCd);
            if (country == null) return null;

            DateTime localStart;
            DateTime localEnd;

            try
            {
                var officeTimeZone = TimeZoneInfo.FindSystemTimeZoneById(country.TimeZone);
                if (month.HasValue)
                {
                    localStart = new DateTime(year, month.Value, 1);
                    localEnd = localStart.AddMonths(1);
                }
                else
                {
                    localStart = new DateTime(year, 1, 1);
                    localEnd = localStart.AddYears(1);
                }

                var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, officeTimeZone);
                var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, officeTimeZone);

                var query = _context.Orders
                    .Where(o => o.OrderDate >= utcStart && o.OrderDate < utcEnd);

                if (!string.IsNullOrEmpty(userName))
                {
                    query = query.Where(o => o.SalesBy == userName);
                }

                return await query
                    .Include(o => o.Customers)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Products)
                    .Select(o => new OrderSummaryDto
                    {
                        OrderDate = o.OrderDate,
                        OrderNo = o.OrderNo,
                        ClientName = o.Customers.FirstName + " " + o.Customers.LastName,
                        MobileNumber = o.Customers.MobileNo,
                        OrderType = o.OrderType,
                        ProductPurchased = string.Join(", ",
                            o.OrderDetails.Select(oi => oi.Products.ProductName)),
                        Amount = o.TotAmount,
                        SalesSource = o.SalesSource,
                        SalesPersonEmail = _context.Users.Where(u => u.UserName == o.SalesBy).Select(u => u.Email).FirstOrDefault()
                    })
                    .ToListAsync();
            }
            catch
            {
                return null;
            }
        }

    }
}
