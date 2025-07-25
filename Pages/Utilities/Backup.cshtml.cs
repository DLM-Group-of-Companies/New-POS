using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static NLI_POS.Services.AuditHelpers;

namespace NLI_POS.Pages.Utilities
{
    [Authorize(Roles = "Admin")]
    public class BackupModel : PageModel
    {
        private readonly DatabaseBackupService _backupService;

        public BackupModel(DatabaseBackupService backupService)
        {
            _backupService = backupService;
        }

        [BindProperty]
        public string? BackupFilePath { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                BackupFilePath = await _backupService.BackupAsync();
                TempData["Message"] = "✅ Backup created successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"❌ Backup failed: {ex.Message}";
            }

            return Page();
        }
    }

}
