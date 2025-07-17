using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Utilities.CustomerClass
{
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CustClass> CustClass { get;set; } = default!;

        public async Task OnGetAsync()
        {
            CustClass = await _context.CustClass.ToListAsync();
            //await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed CustClass List");
        }
    }
}
