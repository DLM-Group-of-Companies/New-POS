using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var exportPath = Path.Combine(_env.WebRootPath, "backups");
                Directory.CreateDirectory(exportPath);

                await ExportAllTablesToCsvAsync(exportPath);

                TempData["Success"] = "Backup completed successfully. Files saved in /backups.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Backup failed: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task ExportAllTablesToCsvAsync(string exportFolder)
        {
            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            var tables = new List<string>();

            using (var cmd = new MySqlCommand("SHOW TABLES", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    tables.Add(reader.GetString(0));
            }

            foreach (var table in tables)
            {
                using var cmd = new MySqlCommand($"SELECT * FROM `{table}`", conn)
                {
                    CommandTimeout = 120
                };

                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                var filePath = Path.Combine(exportFolder, $"{table}.csv");
                await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                // Write column headers
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

}
