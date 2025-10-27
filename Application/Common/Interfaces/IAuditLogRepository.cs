using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLogEntity> CreateAsync(AuditLogEntity auditLog);
    Task<IEnumerable<AuditLogEntity>> GetByEntityAsync(string entityName, string entityId);
    Task<IEnumerable<AuditLogEntity>> GetByUserIdAsync(int userId);
    Task<IEnumerable<AuditLogEntity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
