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
                using var tableCmd = new MySqlCommand($"SELECT * FROM `{table}`", conn)
                {
                    CommandTimeout = 120 // seconds
                }; 
                using var adapter = new MySqlDataAdapter(tableCmd);
                adapter.SelectCommand.CommandTimeout = 120;
                var dt = new DataTable();
                adapter.Fill(dt);

                var csv = new StringBuilder();

                // Header
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                csv.AppendLine(string.Join(",", columnNames));

                // Rows
                foreach (DataRow row in dt.Rows)
                {
                    var fields = row.ItemArray.Select(f => $"\"{f?.ToString()?.Replace("\"", "\"\"")}\"");
                    csv.AppendLine(string.Join(",", fields));
                }

                var filePath = Path.Combine(exportFolder, $"{table}.csv");
                await System.IO.File.WriteAllTextAsync(filePath, csv.ToString());
            }
        }
    }

}
