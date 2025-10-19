namespace Application.Common.Interfaces;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTime ConvertToArgentinaTime(DateTime utcDateTime);
    DateTime ConvertToUtc(DateTime argentinaDateTime);
}
