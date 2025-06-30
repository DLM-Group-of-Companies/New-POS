using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.UsersOfficeAccess
{
    public class EditModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public EditModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UserOfficeAccess UserOfficeAccess { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userofficeaccess =  await _context.UserOfficesAccess.FirstOrDefaultAsync(m => m.Id == id);
            if (userofficeaccess == null)
            {
                return NotFound();
            }
            UserOfficeAccess = userofficeaccess;
           ViewData["OfficeId"] = new SelectList(_context.OfficeCountry, "Id", "Name");
           ViewData["UserId"] = new SelectList(_context.AppUsers, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(UserOfficeAccess).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserOfficeAccessExists(UserOfficeAccess.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool UserOfficeAccessExists(int id)
        {
            return _context.UserOfficesAccess.Any(e => e.Id == id);
        }
    }
}
