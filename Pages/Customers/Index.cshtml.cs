using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using System.Linq.Dynamic.Core;

namespace NLI_POS.Pages.Customers
{
    [Authorize(Roles = "Admin,CS")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Customer> Customer { get; set; } = default!;
        //public List<Country> Countries { get; set; } = new();

        public void OnGet()
        {
            var countries = _context.Country.Where(c => c.IsActive)
                .Select(o => new SelectListItem
                {
                    Value = o.Code,
                    Text = o.Name

                })
                .ToList();

            ViewData["Countries"] = countries;
        }

        public async Task<IActionResult> OnGetDataAsync()
        {
            var requestForm = Request.Query;

            var draw = requestForm["draw"].FirstOrDefault();
            var start = Convert.ToInt32(requestForm["start"]);
            var length = Convert.ToInt32(requestForm["length"]);
            var sortColumnIndex = Convert.ToInt32(requestForm["order[0][column]"]);
            var sortDirection = requestForm["order[0][dir]"].FirstOrDefault();
            var searchValue = requestForm["search[value]"].FirstOrDefault();

            string[] columnNames = { "custCode", "fullName", "email", "mobile", "landline", "city", "className" };

            var country = requestForm["country"].FirstOrDefault();

            var query = _context.Customer
                .Include(c => c.CustClasses)
                .AsNoTracking()
                .Select(c => new
                {
                    id = c.Id,
                    custCode = c.CustCode,
                    fullName = c.FirstName + " " + c.LastName,
                    email = c.Email,
                    mobile = c.MobileNo,
                    landline = c.LandlineNo,
                    city = c.City,           
                    country = c.Country,
                    className = c.CustClasses != null ? c.CustClasses.Name : ""
                });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(c =>
                c.custCode.Contains(searchValue) ||
                    c.fullName.Contains(searchValue) ||
                    c.email.Contains(searchValue) ||
                    c.mobile.Contains(searchValue) ||
                    c.landline.Contains(searchValue) ||
                    c.city.Contains(searchValue) ||
                    c.className.Contains(searchValue));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Where(c => c.country == country); //Filters by code
            }

            var totalRecords = await query.CountAsync();

            var sortedColumn = columnNames[sortColumnIndex];
            var data = await query
                .OrderBy($"{sortedColumn} {sortDirection}")
                .Skip(start)
                .Take(length)
                .ToListAsync();

            //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Customer List");


            return new JsonResult(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });

        }

        public async Task<IActionResult> OnPostDeleteAsync(int? id)
        {
            if (!User.HasPermission("Delete"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage();
            }

            if (id == null)
            {
                return new JsonResult(new { success = false, message = "Invalid ID" }) { StatusCode = 400 };
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return new JsonResult(new { success = false, message = "Customer not found" }) { StatusCode = 404 };
            }

            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
            if (hasOrders)
            {
                return new JsonResult(new { success = false, message = "Cannot delete: customer has transaction(s)." }) { StatusCode = 400 };
            }

            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Deleted {customer.CustCode}: {customer.FirstName} {customer.LastName}");

            return new JsonResult(new { success = true, message = "Customer deleted successfully." });
        }

    }
}