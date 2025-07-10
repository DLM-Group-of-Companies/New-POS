using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task OnGetAsync()
        {
            //Customer = await _context.Customer
            //    .Include(c => c.CustClasses)
            //    .Include(c => c.OfficeCountry).ToListAsync();
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


            var query = _context.Customer
                .Include(c => c.CustClasses)
                .Include(c => c.OfficeCountry)
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
                    country = c.OfficeCountry != null ? c.OfficeCountry.Name : "",
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

            var totalRecords = await query.CountAsync();

            var sortedColumn = columnNames[sortColumnIndex];
            var data = await query
                .OrderBy($"{sortedColumn} {sortDirection}")
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return new JsonResult(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.HasPermission("Delete"))
            {
                TempData["ErrorMessage"] = "You are not authorized to perform this action.";
                return RedirectToPage();
            }
            var customer = await _context.Customer.FindAsync(id);
            if (customer != null)
            {
                _context.Customer.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Customer not found.";
            }

            return RedirectToPage(); // This triggers OnGetAsync again
        }

    }
}
