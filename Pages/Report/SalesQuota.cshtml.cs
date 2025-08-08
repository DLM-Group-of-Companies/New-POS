using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
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

        public List<SalesQuota> Quotas { get; set; } = new();

        public class SalesPersonInfo
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string UserName { get; set; }
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

        public async Task OnGetAsync()
        {
            Quotas = await _context.SalesQuotas
                .Include(q => q.SalesPerson)
                .OrderByDescending(q => q.QuotaDate)
                .ToListAsync();

            SalesPeople = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new SalesPersonInfo
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName
                })
                .ToListAsync();


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

                existing.SalesPersonId = Input.SalesPersonId;
                existing.SalesPersonUserName = Input.SalesPersonUserName;
                existing.QuotaDate = Input.QuotaDate;
                existing.QuotaAmount = Input.QuotaAmount;
            }
            else
            {
                // Add
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
