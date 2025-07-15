using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Utilities.AuditLogs
{
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<AuditLog> AuditLog { get;set; } = default!;

        public async Task OnGetAsync()
        {
            AuditLog = await _context.AuditLogs.OrderByDescending(a=>a.Timestamp)
                .Include(a => a.User).ToListAsync();
        }
    }
}
