using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            Users = _context.Users
                .Include(u => u.OfficeCountry)
                .ToList();

            //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Users List");
        }

        public async Task<IActionResult> OnPostToggleUserActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new JsonResult(new { message = "User not found" }) { StatusCode = 404 };

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new JsonResult(new { message = "Unable to update user" }) { StatusCode = 500 };

            return new JsonResult(new { message = user.IsActive ? "User activated" : "User deactivated" });

        }

    }

}
