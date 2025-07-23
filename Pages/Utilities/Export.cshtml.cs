using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Text;

namespace NLI_POS.Pages.Utilities
{
    public class ExportModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ExportModel(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        [BindProperty]
        public string SelectedTable { get; set; } = "";

        public List<string> TableList { get; set; } = new();

        public async Task OnGetAsync()
        {
            TableList = await LoadTableListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            TableList = await LoadTableListAsync();

            if (string.IsNullOrWhiteSpace(SelectedTable))
            {
                TempData["Error"] = "Please select a table to export.";
                return Page();
            }

            try
            {
                var exportPath = Path.Combine(_env.WebRootPath, "backups");
                Directory.CreateDirectory(exportPath);

                await ExportTableToCsvAsync(SelectedTable, exportPath);
                TempData["Success"] = $"Table '{SelectedTable}' exported successfully to /backups folder.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Export failed: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task<List<string>> LoadTableListAsync()
        {
            var tables = new List<string>();
            using var conn = new MySqlConnection(_config.GetConnectionString("NLPOSLiveConn"));
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SHOW TABLES", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));

            return tables;
        }

        private async Task ExportTableToCsvAsync(string tableName, string exportFolder)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("NLPOSLiveConn"));
            await conn.OpenAsync();

            using var cmd = new MySqlCommand($"SELECT * FROM `{tableName}`", conn);
            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

            var filePath = Path.Combine(exportFolder, $"{tableName}.csv");
            await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Write headers
            var headers = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i));
            await writer.WriteLineAsync(string.Join(",", headers));

            // Write rows
            while (await reader.ReadAsync())
            {
                var fields = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString()?.Replace("\"", "\"\"");
                    fields.Add($"\"{value}\"");
                }

                await writer.WriteLineAsync(string.Join(",", fields));
            }
        }
    }
}
