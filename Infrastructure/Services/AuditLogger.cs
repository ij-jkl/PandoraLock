using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using System.Text.Json;

namespace Infrastructure.Services;

public class AuditLogger : IAuditLogger
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeService _dateTimeService;

    public AuditLogger(IAuditLogRepository auditLogRepository, IDateTimeService dateTimeService)
    {
        _auditLogRepository = auditLogRepository;
        _dateTimeService = dateTimeService;
    }

    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLogEntity
        {
            Action = entry.Action,
            EntityName = entry.EntityName,
            EntityId = entry.EntityId,
            UserId = entry.UserId,
            Username = entry.Username,
            Timestamp = entry.Timestamp,
            BeforeValue = entry.BeforeValue != null ? JsonSerializer.Serialize(entry.BeforeValue) : null,
            AfterValue = entry.AfterValue != null ? JsonSerializer.Serialize(entry.AfterValue) : null,
            IpAddress = entry.IpAddress,
            AdditionalInfo = entry.AdditionalInfo
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    public async Task LogCreateAsync(string entityName, string entityId, int? userId, string? username, object createdValue, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry
        {
            Action = "Create",
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            Username = username,
            Timestamp = _dateTimeService.Now,
            AfterValue = createdValue,
            IpAddress = ipAddress,
            AdditionalInfo = additionalInfo
        };

        await LogAsync(entry, cancellationToken);
    }

    public async Task LogUpdateAsync(string entityName, string entityId, int? userId, string? username, object? beforeValue, object afterValue, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry
        {
            Action = "Update",
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            Username = username,
            Timestamp = _dateTimeService.Now,
            BeforeValue = beforeValue,
            AfterValue = afterValue,
            IpAddress = ipAddress,
            AdditionalInfo = additionalInfo
        };

        await LogAsync(entry, cancellationToken);
    }

    public async Task LogDeleteAsync(string entityName, string entityId, int? userId, string? username, object deletedValue, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry
        {
            Action = "Delete",
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            Username = username,
            Timestamp = _dateTimeService.Now,
            BeforeValue = deletedValue,
            IpAddress = ipAddress,
            AdditionalInfo = additionalInfo
        };

        await LogAsync(entry, cancellationToken);
    }

    public async Task LogActionAsync(string action, string entityName, string entityId, int? userId, string? username, string? ipAddress = null, string? additionalInfo = null, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            Username = username,
            Timestamp = _dateTimeService.Now,
            IpAddress = ipAddress,
            AdditionalInfo = additionalInfo
        };

        await LogAsync(entry, cancellationToken);
    }
}
