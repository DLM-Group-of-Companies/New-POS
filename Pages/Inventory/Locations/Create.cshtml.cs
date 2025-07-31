using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using NLI_POS.Data;
using NLI_POS.Models;
using Microsoft.EntityFrameworkCore;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public InventoryLocation InventoryLocation { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["Offices"] = new SelectList(await _context.OfficeCountry.ToListAsync(), "Id", "Name");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ViewData["Offices"] = new SelectList(await _context.OfficeCountry.ToListAsync(), "Id", "Name");
            return Page();
        }

        _context.InventoryLocations.Add(InventoryLocation);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
