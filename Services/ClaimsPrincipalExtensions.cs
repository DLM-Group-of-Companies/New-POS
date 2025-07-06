using System.Security.Claims;

namespace NLI_POS.Services
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.IsInRole("Admin") || user.HasClaim("Permission", permission);
        }
    }
}
