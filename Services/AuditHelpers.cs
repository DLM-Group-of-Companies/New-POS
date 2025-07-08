namespace NLI_POS.Services
{
    public class AuditHelpers
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
    }
}
