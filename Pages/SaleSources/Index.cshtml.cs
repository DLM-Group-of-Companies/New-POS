using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Migrations;
using NLI_POS.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NLI_POS.Pages.SaleSources
{
    public class IndexModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public IndexModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<SalesSources> SalesSources { get;set; } = default!;

        public async Task OnGetAsync()
        {
            SalesSources = await _context.SalesSources.ToListAsync();
        }

        public async Task<PartialViewResult> OnGetCreateModal()
        {
            return Partial("_CreateEditSaleSource", new SalesSources());
        }

        public async Task<PartialViewResult> OnGetEditModal(int id)
        {
            var source = await _context.SalesSources.FindAsync(id);
            return Partial("_CreateEditSaleSource", source);
        }

        public async Task<IActionResult> OnPostSaveAsync(SalesSources model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the list so the view can render it
                SalesSources = await _context.SalesSources.ToListAsync();
                //return Page();
                return new JsonResult(new { success = false, message = "All fields are required." });
            }

            if (model.id == 0)
                _context.SalesSources.Add(model);
            else
                _context.SalesSources.Update(model);

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Sale source added successfully!" });
        
        //return RedirectToPage(); // reload index
    }

    }
}
