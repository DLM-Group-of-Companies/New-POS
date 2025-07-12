using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public EditModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!User.HasPermission("Edit"))
            {
                TempData["ErrorMessage"] = "You are not authorized to Edit.";
                return RedirectToPage("./Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            Customer = customer;
            ViewData["CustClass"] = new SelectList(_context.CustClass, "Id", "Name");
            ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
            ViewData["Country"] = new SelectList(_context.Country, "Code", "Name");
            return Page();
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    ModelState.Remove("Customer.OfficeCountries");
        //    ModelState.Remove("Customer.CustClasses");

        //    if (!ModelState.IsValid)
        //    {
        //        return Page();
        //    }

        //    _context.Attach(Customer).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!CustomerExists(Customer.Id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    await AuditHelpers.LogAsync(HttpContext, _context, User, $"Edited Customer ${Customer.CustCode}");
        //    return RedirectToPage("./Index");
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Customer.OfficeCountries");
            ModelState.Remove("Customer.CustClasses");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Step 1: Load the original record from the database
            var existingCustomer = await _context.Customer.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Customer.Id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Step 2: Track the current model for changes
            _context.Attach(Customer).State = EntityState.Modified;

            // Step 3: Compare original vs modified fields
            var entry = _context.Entry(Customer);
            var changedFields = new List<string>();

            foreach (var prop in entry.Properties)
            {
                var originalValue = existingCustomer.GetType().GetProperty(prop.Metadata.Name)?.GetValue(existingCustomer)?.ToString();
                var currentValue = prop.CurrentValue?.ToString();

                if (originalValue != currentValue)
                {
                    changedFields.Add($"{prop.Metadata.Name}: '{originalValue}' → '{currentValue}'");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(Customer.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Step 4: Audit log with changed fields
            var changesSummary = changedFields.Any()
                ? "Changed fields:\n" + string.Join("\n", changedFields)
                : "No fields changed.";

            await AuditHelpers.LogAsync(HttpContext, _context, User, $"Edited Customer {Customer.CustCode}\n{changesSummary}");

            return RedirectToPage("./Index");
        }


        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.Id == id);
        }
    }
}
