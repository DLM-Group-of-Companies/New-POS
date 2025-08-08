using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;

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

            string[] columnNames = { "custCode", "fullName", "mobile", "email", "city", "className", "Id" };

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
                    country = c.Country ?? "",
                    className = c.CustClasses != null ? c.CustClasses.Name : ""
                });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(c =>
                c.custCode.Contains(searchValue.Trim()) ||
                    c.fullName.Contains(searchValue.Trim()) ||
                    c.email.Contains(searchValue.Trim()) ||
                    c.mobile.Contains(searchValue.Trim()) ||
                    c.landline.Contains(searchValue.Trim()) ||
                    c.city.Contains(searchValue.Trim()) ||
                    c.className.Contains(searchValue.Trim()));
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

        public IActionResult OnGetExportAllCustomers(string country)
        {
            var query = _context.Customer.AsQueryable();

            if (!string.IsNullOrEmpty(country))
                query = query.Where(c => c.Country == country);

            var customers = query
                .OrderByDescending(c => c.Id)
                .Select(c => new
                {
                    c.CustCode,
                    FullName = c.FirstName + " " + c.LastName,
                    c.MobileNo,
                    c.Email,
                    c.City,
                    ClassName = c.CustClasses.Name
                })
                .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Customers");

            ws.Cell(1, 1).InsertTable(customers, "Customers", true);

            // ✅ Do NOT dispose the stream here
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }




    }
}