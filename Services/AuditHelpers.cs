using NLI_POS.Data;
using NLI_POS.Models;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;

namespace NLI_POS.Services
{
    public static class AuditHelpers
    {
        public static DateTime GetLocalTime(string timeZoneId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch
            {
                return DateTime.UtcNow; // fallback to UTC
            }
        }

        public static (DateTime StartUtc, DateTime EndUtc) GetUtcDayRange(string timeZoneId, DateTime? dateLocal = null)
        {
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // If no date given, use today in that time zone
            var localDate = dateLocal ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzInfo).Date;
            var startLocal = localDate.Date;
            var endLocal = startLocal.AddDays(1);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);

            return (startUtc, endUtc);
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static async Task LogAsync(HttpContext httpContext, ApplicationDbContext dbContext, ClaimsPrincipal user, string activity)
        {
            dbContext ??= httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var path = httpContext.Request.Path;
            //var localTZ = httpContext.Session.GetString("localTimeZone") ?? "UTC";
            var ip = httpContext.Session.GetString("IP") ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Activity = activity,
                Timestamp = DateTime.UtcNow,
                Path = path,
                IP = ip
            });

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Optionally log the error somewhere
                Console.WriteLine($"Audit log save failed: {ex.Message}");
            }
        }

        public class DatabaseBackupService
        {
            private readonly IWebHostEnvironment _env;

            public DatabaseBackupService(IWebHostEnvironment env)
            {
                _env = env;
            }

            public async Task<string> BackupAsync(string backupFileName = null)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = backupFileName ?? $"backup_{timestamp}.sql";
                string backupFolder = Path.Combine(_env.ContentRootPath, "Backups");

                Directory.CreateDirectory(backupFolder);
                string filePath = Path.Combine(backupFolder, fileName);

                // Update these with your actual DB credentials
                string server = "MYSQL8010.site4now.net";
                string database = "db_a70373_nlipos";
                string user = "a70373_nlipos";
                string password = "#04-69LuckyPlaza";

                var args = $"-h {server} -u {user} -p{password} {database}";

                var filename = @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe";

                var startInfo = new ProcessStartInfo
                {
                    FileName = filename, /*"mysqldump",*/
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }; 

                using var process = Process.Start(startInfo);
                using var reader = process.StandardOutput;
                string result = await reader.ReadToEndAsync();

                await File.WriteAllTextAsync(filePath, result);

                return filePath;
            }
        }

        //for Audit on Products
        public static bool TryGetChanges<T>(T original, T current, out List<string> changes, params string[] ignoreProps)
        {             
            changes = new List<string>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (ignoreProps.Contains(prop.Name)) continue;

                var originalValue = prop.GetValue(original)?.ToString() ?? "null";
                var currentValue = prop.GetValue(current)?.ToString() ?? "null";

                if (originalValue != currentValue)
                {
                    changes.Add($"{prop.Name}: '{originalValue}' → '{currentValue}'");
                }
            }

            return changes.Any();
        }

        public static List<string> CompareProductPrice(ProductPrice oldPrice, ProductPrice newPrice)
        {
            var changes = new List<string>();

            void Compare(string name, decimal? oldVal, decimal? newVal)
            {
                if (!Equals(oldVal, newVal))
                    changes.Add($"{name}: '{oldVal}' → '{newVal}'");
            }

            Compare("UnitCost", oldPrice.UnitCost, newPrice.UnitCost);
            Compare("RegPrice", oldPrice.RegPrice, newPrice.RegPrice);
            Compare("DistPrice", oldPrice.DistPrice, newPrice.DistPrice);
            Compare("StaffPrice", oldPrice.StaffPrice, newPrice.StaffPrice);
            Compare("BPPPrice", oldPrice.BPPPrice, newPrice.BPPPrice);
            Compare("MedPackPrice", oldPrice.MedPackPrice, newPrice.MedPackPrice);
            Compare("CorpAccPrice", oldPrice.CorpAccPrice, newPrice.CorpAccPrice);
            Compare("NaturoPrice", oldPrice.NaturoPrice, newPrice.NaturoPrice);

            return changes;
        }
    }

}
