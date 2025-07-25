using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Services;

namespace NLI_POS.Pages.Utilities.PaymentMethods
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<PaymentMethod> PaymentMethods { get;set; } = default!;

        public async Task OnGetAsync()
        {
            PaymentMethods = await _context.PaymentMethods.ToListAsync();
            await AuditHelpers.LogAsync(HttpContext, _context, User, "Viewed Payment Methods List");
        }
    }
}
