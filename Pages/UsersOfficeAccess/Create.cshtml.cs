using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> OnGetAsync(string? userId)
        {
            var availableOffices = await _context.OfficeCountry
    .Where(o => !_context.UserOfficesAccess
        .Any(uoa => uoa.UserId == userId && uoa.OfficeId == o.Id))
    .ToListAsync();

            //ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            ViewData["OfficeId"] = new SelectList(availableOffices, "Id", "Name");

            ViewData["UserId"] = new SelectList(_context.Users.Where(u => u.Id == userId)
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
