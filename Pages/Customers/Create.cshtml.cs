using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Customers
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
            ViewData["CustClass"] = new SelectList(_context.CustClass, "Id", "Name");
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
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

            ModelState.Remove("Customer.OfficeCountry");
            ModelState.Remove("Customer.CustClasses");

            //-- Generate Customer ID
            int lastID = _context.Customer.Max(p => p.Id) + 1;
            string lastIdPadded = lastID.ToString().PadLeft(6, '0');
            var off = Customer.OfficeId;

            Customer.CustCode = Customer.Country + off + DateTime.UtcNow.AddHours(8).ToString("yy") + lastIdPadded;

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
