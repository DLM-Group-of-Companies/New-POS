using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;
using System.Globalization;

namespace NLI_POS.Pages.Customers
{
    public class CreateModel : BasePageModel
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(NLI_POS.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public List<OfficeSelectItem> OfficeList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.HasPermission("Add"))
            {
                TempData["ErrorMessage"] = "You are not authorized to Add.";
                return RedirectToPage("./Index");
            }

            try
            {
                ViewData["CustClass"] = new SelectList(await _context.CustClass.Where(c=>c.IsActive).ToListAsync(), "Id", "Name");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load CustClass", ex);
            }

            OfficeList = await GetUserOfficesAsync();
            ViewData["Country"] = new SelectList(_context.Country, "Code", "Name");
            return Page();
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public static string ToProperCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["CustClass"] = new SelectList(_context.CustClass, "Id", "Name");
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            ViewData["Country"] = new SelectList(_context.Country, "Code", "Name");

            ModelState.Remove("Customer.OfficeCountries");
            ModelState.Remove("Customer.CustClasses");

            // Generate base code
            int lastID = _context.Customer.Any() ? _context.Customer.Max(p => p.Id) + 1 : 1;
            string off = Customer.OfficeId.ToString();
            string baseCode = Customer.Country + off + DateTime.UtcNow.AddHours(8).ToString("yy");

            string custCode = null;
            bool isUnique = false;
            int maxAttempts = 5;

            for (int i = 0; i < maxAttempts; i++)
            {
                string paddedId = lastID.ToString().PadLeft(6, '0');
                custCode = baseCode + paddedId;

                bool exists = await _context.Customer.AnyAsync(c => c.CustCode == custCode);
                if (!exists)
                {
                    isUnique = true;
                    break;
                }

                lastID++; // try next ID
            }

            // Check after retry attempts
            if (!isUnique)
            {
                ModelState.AddModelError(string.Empty, "Unable to generate a unique Customer Code. Please try again.");
                return Page();
            }

            Customer.CustCode = custCode;


            if (!ModelState.IsValid)
            {
                return Page();
            }

            Customer.FirstName = ToProperCase(Customer.FirstName);
            if (Customer.MiddleName!=null ) Customer.MiddleName = ToProperCase(Customer.MiddleName);
            Customer.LastName = ToProperCase(Customer.LastName);

            _context.Customer.Add(Customer);
            await _context.SaveChangesAsync();

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Added Customer: {Customer.CustCode} | {Customer.FirstName} {Customer.LastName}");
            return RedirectToPage("./Index");
        }
    }
}
