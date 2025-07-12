using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Users = _context.Users
                .Include(u => u.OfficeCountry)
                .ToList();

            await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Users List");
        }
    }

}
