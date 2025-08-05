using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;
using static NLI_POS.Services.BasePageModel;
using TimeZoneConverter;

namespace NLI_POS.Pages.Orders
{
    [Authorize(Roles = "Admin,CS")]
    public class IndexModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Order> Order { get; set; } = default!;
        public List<OfficeSelectItem> OfficeList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            OfficeList = await GetUserOfficesAsync();
            return Page();
        }

        //public async Task<JsonResult> OnGetOrdersAsync(string officeId, string locale)
        //{

        //    var query = _context.Orders.OrderByDescending(o=>o.OrderDate)
        //           .Include(o => o.Customers)
        //           .Include(o => o.Office)
        //           .Include(o => o.ProductItems)
        //           .AsQueryable();

        //    if (!string.IsNullOrEmpty(officeId))
        //    {
        //        if (int.TryParse(officeId, out int officeIdInt))
        //        {
        //            query = query.Where(o => o.OfficeId == officeIdInt);
        //        }
        //    }

        //    var orders = await query
        //        .Select(o => new
        //        {
        //            o.OrderNo,
        //            OrderDate = DateTime.SpecifyKind(o.OrderDate, DateTimeKind.Utc),
        //            CustomerName = o.Customers.FirstName + " " + o.Customers.LastName,
        //            Office = o.Office.Name,
        //            o.IsVoided,
        //            TotAmount = o.TotAmount
        //        })
        //        .ToListAsync();

        //    //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Order List");

        //    return new JsonResult(new { data = orders });
        //}      

        public async Task<IActionResult> OnGetOrdersAsync()
        {
            var requestForm = Request.Query;

            var draw = requestForm["draw"].FirstOrDefault();
            var start = Convert.ToInt32(requestForm["start"]);
            var length = Convert.ToInt32(requestForm["length"]);
            var sortColumnIndex = Convert.ToInt32(requestForm["order[0][column]"]);
            var sortDirection = requestForm["order[0][dir]"].FirstOrDefault();
            var searchValue = requestForm["search[value]"].FirstOrDefault();

            var officeId = requestForm["officeId"].FirstOrDefault();

            var query = _context.Orders
                .Include(o => o.Customers)
                .Include(o => o.Office)
                .Include(o => o.ProductItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(officeId) && int.TryParse(officeId, out int officeIdInt))
            {
                query = query.Where(o => o.OfficeId == officeIdInt);
            }

            // Total before filtering
            var recordsTotal = await query.CountAsync();

            // Search
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(o =>
                    o.Customers.FirstName.Contains(searchValue) ||
                    o.Customers.LastName.Contains(searchValue) ||
                    o.OrderNo.Contains(searchValue));
            }

            // Total after filtering
            var recordsFiltered = await query.CountAsync();

            // Ordering
            switch (sortColumnIndex)
            {
                case 0: // OrderNo
                    query = sortDirection == "asc" ? query.OrderBy(o => o.OrderNo) : query.OrderByDescending(o => o.OrderNo);
                    break;
                case 1: // OrderDate
                    query = sortDirection == "asc" ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate);
                    break;
                case 2: // CustomerName
                    query = sortDirection == "asc"
                        ? query.OrderBy(o => o.Customers.FirstName)
                        : query.OrderByDescending(o => o.Customers.FirstName);
                    break;
                case 3: // Office
                    query = sortDirection == "asc"
                        ? query.OrderBy(o => o.Office.Name)
                        : query.OrderByDescending(o => o.Office.Name);
                    break;
                case 4: // TotAmount
                    query = sortDirection == "asc" ? query.OrderBy(o => o.TotAmount) : query.OrderByDescending(o => o.TotAmount);
                    break;
            }

            // Paging
            var data = await query
                .Skip(start)
                .Take(length)
                .Select(o => new
                {
                    o.OrderNo,
                    OrderDate = DateTime.SpecifyKind(o.OrderDate, DateTimeKind.Utc),
                    CustomerName = o.Customers.FirstName + " " + o.Customers.LastName,
                    Office = o.Office.Name,
                    o.IsVoided,
                    TotAmount = o.TotAmount
                })
                .ToListAsync();

            return new JsonResult(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

    }
}
