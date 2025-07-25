using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Models.ViewModels;
using NLI_POS.Services;
using System.Security.Claims;

namespace NLI_POS.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class UserAccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAccessModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
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

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Viewed User Access for {user.UserName}");
            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync(string userId, List<string> roles, List<string> claims)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Take snapshots of current roles and claims
            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentClaims = await _userManager.GetClaimsAsync(user);

            // ----- ROLES -----
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, roles ?? new());

            // ----- CLAIMS -----
            foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
                await _userManager.RemoveClaimAsync(user, claim);

            foreach (var value in claims ?? new())
                await _userManager.AddClaimAsync(user, new Claim("Permission", value));

            // ----- LOG CHANGES -----
            var changedFields = new List<string>();

            if (currentRoles.Except(roles).Any() || roles.Except(currentRoles).Any())
            {
                changedFields.Add("Original Roles: " + string.Join(", ", currentRoles));
                changedFields.Add("Updated Roles: " + string.Join(", ", roles));
            }

            var currentClaimValues = currentClaims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            if (currentClaimValues.Except(claims).Any() || claims.Except(currentClaimValues).Any())
            {
                changedFields.Add("Original Claims: " + string.Join(", ", currentClaimValues));
                changedFields.Add("Updated Claims: " + string.Join(", ", claims));
            }

            var summary = changedFields.Any()
                ? $"Access updated for user {user.UserName}:\n" + string.Join("\n", changedFields)
                : $"Access updated for user {user.UserName}, no permission changes detected.";

            await AuditHelpers.LogAsync(HttpContext, _context, User, summary);

            return new JsonResult(new { success = true });
        }

    }
}


//namespace NLI_POS.Pages.Users
//{
//    public class UserAccessModel : PageModel
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;

//        public UserAccessModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
//        {
//            _userManager = userManager;
//            _roleManager = roleManager;
//        }

//        public UserAccessViewModel AccessViewModel { get; set; }

//        public async Task<IActionResult> OnGetAsync(string userId)
//        {
//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null) return NotFound();

//            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
//            var claims = new List<Claim>
//        {
//            new Claim("Permission", "Add"),
//            new Claim("Permission", "Edit"),
//            new Claim("Permission", "Delete"),
//            new Claim("Permission", "View")
//        };

//            var userRoles = await _userManager.GetRolesAsync(user);
//            var userClaims = await _userManager.GetClaimsAsync(user);

//            AccessViewModel = new UserAccessViewModel
//            {
//                UserId = user.Id,
//                UserName = user.UserName,
//                Email = user.Email,
//                FullName = user.FullName,
//                AssignedRoles = userRoles.ToList(),
//                AssignedClaims = userClaims.ToList(),
//                AvailableRoles = roles,
//                AvailableClaims = claims
//            };

//            await AuditHelpers.LogAsync(HttpContext, null, User, $"Viewer User Access");
//            return Page();
//        }

//        public async Task<IActionResult> OnPostAssignAsync(string userId, List<string> roles, List<string> claims)
//        {
//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null) return NotFound();

//            var currentRoles = await _userManager.GetRolesAsync(user);
//            await _userManager.RemoveFromRolesAsync(user, currentRoles);
//            await _userManager.AddToRolesAsync(user, roles ?? new());

//            var currentClaims = await _userManager.GetClaimsAsync(user);
//            foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
//                await _userManager.RemoveClaimAsync(user, claim);

//            foreach (var value in claims ?? new())
//                await _userManager.AddClaimAsync(user, new Claim("Permission", value));

//            return new JsonResult(new { success = true });
//        }
//    }

//}
