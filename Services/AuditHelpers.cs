using NLI_POS.Data;
using NLI_POS.Models;
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

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static async Task LogAsync(HttpContext httpContext, ApplicationDbContext dbContext, ClaimsPrincipal user, string activity)
        {
            dbContext ??= httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var path = httpContext.Request.Path;
            var localTZ = httpContext.Session.GetString("localTimeZone") ?? "UTC";
            var ip = httpContext.Session.GetString("IP") ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Activity = activity,
                Timestamp = GetLocalTime(localTZ),
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

    }

}
