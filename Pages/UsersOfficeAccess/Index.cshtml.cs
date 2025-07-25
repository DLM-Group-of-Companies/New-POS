using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;

namespace NLI_POS.Pages.UsersOfficeAccess
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserOfficeAccess> UserOfficeAccess { get;set; } = default!;

        public async Task OnGetAsync()
        {
            UserOfficeAccess = await _context.UserOfficesAccess
                .Include(u => u.OfficeCountry)
                .Include(u => u.User).ToListAsync();
        }
    }
}
