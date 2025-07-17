using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Offices
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<OfficeCountry> OfficeCountry { get;set; } = default!;

        public async Task OnGetAsync()
        {
            OfficeCountry = await _context.OfficeCountry
                .Include(o => o.Country).ToListAsync();



                //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Office List");

        }
    }
}
