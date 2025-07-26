using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;

namespace NLI_POS.Pages.Inventory.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InventoryTransaction> Transactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            Transactions = await _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t=>t.SourceLocation)
                .Include(t => t.DestinationLocation)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }

}
