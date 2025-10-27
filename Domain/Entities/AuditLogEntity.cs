namespace Domain.Entities;

public class AuditLogEntity
{
    public int Id { get; set; }
    public string Action { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public DateTime Timestamp { get; set; }
    public string? BeforeValue { get; set; }
    public string? AfterValue { get; set; }
    public string? IpAddress { get; set; }
    public string? AdditionalInfo { get; set; }
}
