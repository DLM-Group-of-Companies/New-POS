using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Models;

namespace NLI_POS.Pages.Offices
{
    public class CreateModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CountryId"] = new SelectList(_context.Country.Where(c=>c.IsActive==true), "Id", "Name");
            return Page();
        }

        [BindProperty]
        public OfficeCountry OfficeCountry { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["CountryId"] = new SelectList(_context.Country.Where(c => c.IsActive == true), "Id", "Name");
            ModelState.Remove("OfficeCountry.Country");
            ModelState.Remove("OfficeCountry.Country");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.OfficeCountry.Add(OfficeCountry);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
