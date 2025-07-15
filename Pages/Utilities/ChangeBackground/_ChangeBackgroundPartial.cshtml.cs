using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace NLI_POS.Pages.Utilities.ChangeBackground
{
    public class ChangeModel : PageModel
    {
        private readonly NLI_POS.Data.ApplicationDbContext _context;

        public ChangeModel(NLI_POS.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public List<string> ImagePaths { get; set; }

        public void OnGet()
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/backgrounds");
            ImagePaths = Directory.GetFiles(folderPath)
                                  .Select(f => "/images/backgrounds/" + Path.GetFileName(f))
                                  .ToList();
        }

        [IgnoreAntiforgeryToken] 
        public async Task<IActionResult> OnPostSaveBackgroundAsync([FromBody] SaveBackgroundRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SelectedBackground))
                return BadRequest();

            var selectedPath = request.SelectedBackground;

            // Normalize in case the client sends a full URL
            if (selectedPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(selectedPath);
                selectedPath = uri.PathAndQuery; // sample... /images/backgrounds/main4.jpg
            }

            var sysParam = await _context.SysParams.FirstOrDefaultAsync(); // or by ID
            if (sysParam != null)
            {
                sysParam.Background = request.SelectedBackground;
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { success = true });
        }

        public class SaveBackgroundRequest
        {
            public string SelectedBackground { get; set; }
        }

    }
}
