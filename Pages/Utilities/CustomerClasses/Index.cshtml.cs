using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;

namespace NLI_POS.Pages.Utilities.CustomerClass
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CustClass> CustClass { get;set; } = default!;

        public async Task OnGetAsync()
        {
            CustClass = await _context.CustClass.ToListAsync();
            //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed CustClass List");
        }
    }
}
