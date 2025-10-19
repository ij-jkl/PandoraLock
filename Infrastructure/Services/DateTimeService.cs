using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    private static readonly TimeZoneInfo ArgentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone);

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime ConvertToArgentinaTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime must be in UTC", nameof(utcDateTime));
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, ArgentinaTimeZone);
    }

    public DateTime ConvertToUtc(DateTime argentinaDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(argentinaDateTime, ArgentinaTimeZone);
    }
}
