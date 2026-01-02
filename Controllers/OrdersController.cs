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

            // Get office and its timezone
            var country = await _context.Country.FirstOrDefaultAsync(c => c.Code == cntryCd);
            if (country == null) return BadRequest("Invalid country");

            DateTime? localStart = null;
            DateTime? localEnd = null;

            var officeTimeZone = TimeZoneInfo.FindSystemTimeZoneById(country.TimeZone);
            if (month.HasValue)
            {
                localStart = new DateTime(year, month.Value, 1);
                localEnd = localStart.Value.AddMonths(1);
                    
            }
            else
            {
                localStart = new DateTime(year, 1, 1);
                localEnd = localStart.Value.AddYears(1);
            }

            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart.Value, officeTimeZone);
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd.Value, officeTimeZone);

            var user = await _userManager.FindByEmailAsync(salesEmail);

            var orders = await _context.Orders
                .Where(o => o.SalesBy == user.UserName &&
                            o.OrderDate >= utcStart &&
                            o.OrderDate < utcEnd)
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
                    Amount = o.TotAmount
                })
                .ToListAsync();

            return Ok(orders);
        }

    }
}
