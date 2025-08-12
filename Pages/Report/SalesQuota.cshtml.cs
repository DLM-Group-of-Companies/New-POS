using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Pages.Report
{
    public class SalesQuotaModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesQuotaModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<QuotaDisplayVM> Quotas { get; set; } = new();

        public class SalesPersonInfo
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string UserName { get; set; }
            public decimal SalesToday { get; set; }
            public decimal SalesMTD { get; set; }
        }


        public List<SalesPersonInfo> SalesPeople { get; set; }

        [BindProperty]
        public SalesQuotaInput Input { get; set; }

        public class SalesQuotaInput
        {
            public int? Id { get; set; }

            [Required]
            public string SalesPersonId { get; set; }

            public string SalesPersonUserName { get; set; }

            [DataType(DataType.Date)]
            public DateTime QuotaDate { get; set; }

            [Required]
            [Range(0, 9999999999.99)]
            public decimal QuotaAmount { get; set; }
        }

        public List<SelectListItem> OfficeList { get; set; } = new();
        public int? OfficeId { get; set; }

        public class QuotaDisplayVM
        {
            public int Id { get; set; }
            public string SalesPersonId { get; set; }
            public string SalesPersonName { get; set; }
            public DateTime QuotaDate { get; set; }
            public decimal QuotaAmount { get; set; }

            public decimal SalesToday { get; set; }
            public decimal SalesMTD { get; set; }
        }


        public async Task OnGetAsync(int? officeId = null)
        {
            var office = _context.OfficeCountry
    .Include(o => o.Country).FirstOrDefault(o => o.Id == officeId);

            var timeZone = office?.Country.TimeZone ?? "Asia/Manila";
            var localNow = AuditHelpers.GetLocalTime(timeZone); // or from OfficeTimeZone
            var today = localNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            //List<OfficeCountry> Offices = _context.OfficeCountry.Where(o=>o.isActive).ToList();

            //ViewData["Offices"] = Offices.Select(o => new SelectListItem
            //{
            //    Value = o.Id.ToString(),
            //    Text = o.Name
            //}).ToList();

            ViewData["Offices"] = _context.OfficeCountry
    .Where(o => o.isActive)
    .Select(o => new SelectListItem
    {
        Value = o.Id.ToString(),
        Text = o.Name
    })
    .ToList();


            // Get today's sales per user
            var salesToday = await _context.Orders
                .Where(o => !o.IsVoided && o.OrderDate.Date == today)
                .GroupBy(o => o.SalesBy)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSalesToday = g.Sum(o => o.TotAmount)
                })
                .ToListAsync();

            // Get month-to-date sales per user
            var salesMTD = await _context.Orders
                .Where(o => !o.IsVoided && o.OrderDate >= monthStart && o.OrderDate <= today)
                .GroupBy(o => o.SalesBy)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSalesMTD = g.Sum(o => o.TotAmount)
                })
                .ToListAsync();

            // Load quotas
            Quotas = await _context.SalesQuotas
                .Include(q => q.SalesPerson)
                .Where(q => q.SalesPerson.OfficeId == officeId)
                .OrderByDescending(q => q.QuotaDate)
                .Select(q => new QuotaDisplayVM
                {
                    Id = q.Id,
                    SalesPersonId = q.SalesPersonId,
                    SalesPersonName = q.SalesPerson.FullName,
                    QuotaDate = q.QuotaDate,
                    QuotaAmount = q.QuotaAmount,
                    SalesToday = _context.Orders
                .Where(o => !o.IsVoided && o.SalesBy == q.SalesPersonUserName &&
                            o.OrderDate >= today && o.OrderDate < today.AddDays(1))
                .Sum(o => (decimal?)o.TotAmount) ?? 0,
                    SalesMTD = _context.Orders
                .Where(o => !o.IsVoided && o.SalesBy == q.SalesPersonUserName &&
                            o.OrderDate >= monthStart && o.OrderDate < today.AddDays(1))
                .Sum(o => (decimal?)o.TotAmount) ?? 0
                })
                .ToListAsync();


            // Load salespeople
            SalesPeople = await (from u in _context.Users
                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where r.Name == "Sales"
                                 orderby u.UserName
                                 select new SalesPersonInfo
                                 {
                                     Id = u.Id,
                                     FullName = u.FullName,
                                     UserName = u.UserName
                                 })
                                .ToListAsync();

            // Merge results into SalesPeople list for display
            foreach (var sp in SalesPeople)
            {
                sp.SalesToday = salesToday.FirstOrDefault(s => s.UserId == sp.Id)?.TotalSalesToday ?? 0;
                sp.SalesMTD = salesMTD.FirstOrDefault(s => s.UserId == sp.Id)?.TotalSalesMTD ?? 0;
            }
        }


        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "There's an error saving record.";
                await OnGetAsync();
                return Page();
            }

            if (Input.Id.HasValue)
            {
                // Edit
                var existing = await _context.SalesQuotas.FindAsync(Input.Id.Value);
                if (existing == null) return NotFound();

                // Check if another record with same SalesPerson and Date exists
                bool duplicate = await _context.SalesQuotas
                    .AnyAsync(q => q.SalesPersonId == Input.SalesPersonId
                                && q.QuotaDate == Input.QuotaDate
                                && q.Id != Input.Id.Value); // exclude current record when editing

                if (duplicate)
                {
                    TempData["ErrorMessage"] = "A quota for this sales person and date already exists.";
                    await OnGetAsync();
                    return Page();
                }

                existing.SalesPersonId = Input.SalesPersonId;
                existing.SalesPersonUserName = Input.SalesPersonUserName;
                existing.QuotaDate = Input.QuotaDate;
                existing.QuotaAmount = Input.QuotaAmount;
            }
            else
            {
                // Check if record already exists before adding
                bool exists = await _context.SalesQuotas
                    .AnyAsync(q => q.SalesPersonId == Input.SalesPersonId
                                && q.QuotaDate == Input.QuotaDate);

                if (exists)
                {
                    TempData["ErrorMessage"] = "A quota for this sales person and date already exists.";
                    await OnGetAsync();
                    return Page();
                }

                var quota = new SalesQuota
                {
                    SalesPersonId = Input.SalesPersonId,
                    SalesPersonUserName = Input.SalesPersonUserName,
                    QuotaDate = Input.QuotaDate,
                    QuotaAmount = Input.QuotaAmount
                };
                _context.SalesQuotas.Add(quota);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var quota = await _context.SalesQuotas.FindAsync(id);
            if (quota != null)
            {
                _context.SalesQuotas.Remove(quota);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }

}
