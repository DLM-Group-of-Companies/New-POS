using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages
{
    [Authorize]
    public class TodaySalesModel : BasePageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public TodaySalesModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int Office { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        public List<OfficeSelectItem> OfficeList { get; set; }

        public decimal TotalSales { get; set; }

        public async Task<IActionResult> OnGetPartialAsync(int? Office)
        {
            Date = DateTime.UtcNow;
            OfficeList = await GetUserOfficesAsync();

            // Default to the first assigned office if none is selected
            if (!Office.HasValue || Office.Value == 0)
            {
                Office = OfficeList.FirstOrDefault()?.Value is string id && int.TryParse(id, out var officeId)
                    ? officeId
                    : 0;
            }

            TotalSales = await _context.Orders
                .Include(o => o.Office)
                .Where(o => o.Office.Id == Office && o.OrderDate.Date == Date.Value.Date && !o.IsVoided)
                .SumAsync(o => (decimal?)o.TotAmount) ?? 0;

            this.Office = Office.Value; // update model's Office value

            return Partial("_TodaySalesPartial", this);
        }



    }
}
