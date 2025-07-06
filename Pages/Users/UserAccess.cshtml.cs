using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLI_POS.Models.ViewModels;
using NLI_POS.Models;
using System.Security.Claims;

namespace NLI_POS.Pages.Users
{
    public class UserAccessModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAccessModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public UserAccessViewModel AccessViewModel { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var claims = new List<Claim>
        {
            new Claim("Permission", "Add"),
            new Claim("Permission", "Edit"),
            new Claim("Permission", "Delete"),
            new Claim("Permission", "View")
        };

            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            AccessViewModel = new UserAccessViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                AssignedRoles = userRoles.ToList(),
                AssignedClaims = userClaims.ToList(),
                AvailableRoles = roles,
                AvailableClaims = claims
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync(string userId, List<string> roles, List<string> claims)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, roles ?? new());

            var currentClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
                await _userManager.RemoveClaimAsync(user, claim);

            foreach (var value in claims ?? new())
                await _userManager.AddClaimAsync(user, new Claim("Permission", value));

            return new JsonResult(new { success = true });
        }
    }

}
