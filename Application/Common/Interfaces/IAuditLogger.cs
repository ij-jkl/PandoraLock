using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    Task LogCreateAsync(string entityName, string entityId, int? userId, string? username, object createdValue, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default);
    Task LogUpdateAsync(string entityName, string entityId, int? userId, string? username, object? beforeValue, object afterValue, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default);
    Task LogDeleteAsync(string entityName, string entityId, int? userId, string? username, object deletedValue, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default);
    Task LogActionAsync(string action, string entityName, string entityId, int? userId, string? username, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default);
}
