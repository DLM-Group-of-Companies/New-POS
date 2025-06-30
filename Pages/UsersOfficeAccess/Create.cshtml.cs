using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Models;

namespace NLI_POS.Pages.UsersOfficeAccess
{
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string? userId)
        {
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.AppUsers.Where(u=>u.Id== userId)
                .Select(u => new
                {
                    u.Id,
                    DisplayName = u.UserName + " - " + u.FullName
                }), "Id", "DisplayName");

            // Optional: pre-select UserId if passed
            if (!string.IsNullOrEmpty(userId))
            {
                UserOfficeAccess = new UserOfficeAccess { UserId = userId };
            }

            return Page();
        }


        [BindProperty]
        public UserOfficeAccess UserOfficeAccess { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.UserOfficesAccess.Add(UserOfficeAccess);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
