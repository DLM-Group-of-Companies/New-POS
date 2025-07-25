using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.ViewModels;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models.ViewModels;

public class TransferModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public TransferModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public TransferInventoryViewModel Input { get; set; }

    public SelectList LocationList { get; set; }
    public SelectList ProductList { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var fromStock = await _context.InventoryStocks
            .FirstOrDefaultAsync(s => s.LocationId == Input.FromLocationId && s.ProductId == Input.ProductId);

        if (fromStock == null || fromStock.StockQty < Input.Quantity)
        {
            ModelState.AddModelError(string.Empty, "Not enough stock at the source location.");
            await LoadDropdownsAsync();
            return Page();
        }

        fromStock.StockQty -= Input.Quantity;

        var toStock = await _context.InventoryStocks
            .FirstOrDefaultAsync(s => s.LocationId == Input.ToLocationId && s.ProductId == Input.ProductId);

        if (toStock == null)
        {
            toStock = new InventoryStock
            {
                LocationId = Input.ToLocationId,
                ProductId = Input.ProductId,
                StockQty = 0
            };
            _context.InventoryStocks.Add(toStock);
        }

        toStock.StockQty += Input.Quantity;

        // Optional: Add inventory audit record here

        await _context.SaveChangesAsync();
        TempData["Success"] = "Transfer completed successfully.";
        return RedirectToPage("/Inventory/Transfer");
    }

    private async Task LoadDropdownsAsync()
    {
        var locations = await _context.InventoryLocations.ToListAsync();
        var products = await _context.Products.OrderBy(p => p.ProductName).ToListAsync();

        LocationList = new SelectList(locations, "Id", "Name");
        ProductList = new SelectList(products, "Id", "ProductName");
    }
}
