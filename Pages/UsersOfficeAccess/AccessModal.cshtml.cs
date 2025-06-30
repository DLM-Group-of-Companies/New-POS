using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.UsersOfficeAccess
{
    public class AccessModalModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUser User { get; set; }
        public List<UserOfficeAccess> AccessList { get; set; } = [];

        public AccessModalModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            User = await _context.AppUsers.FindAsync(userId);
            if (User == null) return NotFound();

            AccessList = await _context.UserOfficesAccess
                .Include(x => x.OfficeCountry)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return Page();
        }
    }

}
