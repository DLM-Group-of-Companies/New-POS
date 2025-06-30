using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.UsersOfficeAccess
{
    public class DetailsModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public DetailsModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public UserOfficeAccess UserOfficeAccess { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userofficeaccess = await _context.UserOfficesAccess.FirstOrDefaultAsync(m => m.Id == id);
            if (userofficeaccess == null)
            {
                return NotFound();
            }
            else
            {
                UserOfficeAccess = userofficeaccess;
            }
            return Page();
        }
    }
}
