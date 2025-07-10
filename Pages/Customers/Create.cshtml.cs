using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

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

            //ViewData["CustClass"] = new SelectList(_context.CustClass, "Id", "Name");
            try
            {
                ViewData["CustClass"] = new SelectList(await _context.CustClass.ToListAsync(), "Id", "Name");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load CustClass", ex);
            }


            //ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            OfficeList = await GetUserOfficesAsync();
            ViewData["Country"] = new SelectList(_context.Country, "Code", "Name");
            return Page();
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["CustClass"] = new SelectList(_context.CustClass, "Id", "Name");
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            ViewData["Country"] = new SelectList(_context.Country, "Code", "Name");

            ModelState.Remove("Customer.OfficeCountries");
            ModelState.Remove("Customer.CustClasses");

            //-- Generate Customer ID
            //int lastID = 0;
            //if (_context.Customer.Count() > 0)
            //{
            //     lastID = _context.Customer.Max(p => p.Id) + 1;
            //}
            //else
            //{
            //     lastID = 1;
            //}
            //string lastIdPadded = lastID.ToString().PadLeft(6, '0');
            //var off = Customer.OfficeId;

            //Customer.CustCode = Customer.Country + off + DateTime.UtcNow.AddHours(8).ToString("yy") + lastIdPadded;

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

            _context.Customer.Add(Customer);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
