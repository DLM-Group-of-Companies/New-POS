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
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserOfficeAccess> UserOfficeAccess { get;set; } = default!;

        public async Task OnGetAsync()
        {
            UserOfficeAccess = await _context.UserOfficesAccess
                .Include(u => u.OfficeCountry)
                .Include(u => u.User).ToListAsync();
        }
    }
}
