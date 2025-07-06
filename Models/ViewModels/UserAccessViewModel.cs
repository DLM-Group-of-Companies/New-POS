using System.Security.Claims;

namespace NLI_POS.Models.ViewModels
{
    // /ViewModels/UserAccessViewModel.cs
    public class UserAccessViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }

        public List<string> AvailableRoles { get; set; } = new();
        public List<string> AssignedRoles { get; set; } = new();

        public List<Claim> AvailableClaims { get; set; } = new();
        public List<Claim> AssignedClaims { get; set; } = new();
    }


}
