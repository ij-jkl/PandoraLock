namespace Application.Common.Models;

public class AuditLogEntry
{
    public string Action { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public DateTime Timestamp { get; set; }
    public object? BeforeValue { get; set; }
    public object? AfterValue { get; set; }
    public string? IpAddress { get; set; }
    public string? AdditionalInfo { get; set; }
}
