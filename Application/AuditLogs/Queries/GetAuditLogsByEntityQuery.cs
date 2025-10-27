using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.AuditLogs.Queries;

public class GetAuditLogsByEntityQuery : IRequest<ResponseObjectJsonDto>
{
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;

    public GetAuditLogsByEntityQuery(string entityName, string entityId)
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

public class GetAuditLogsByEntityQueryHandler : IRequestHandler<GetAuditLogsByEntityQuery, ResponseObjectJsonDto>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsByEntityQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetAuditLogsByEntityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var auditLogs = await _auditLogRepository.GetByEntityAsync(request.EntityName, request.EntityId);

            var auditLogDtos = auditLogs.Select(a => new AuditLogDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                UserId = a.UserId,
                Username = a.Username,
                Timestamp = a.Timestamp,
                BeforeValue = a.BeforeValue,
                AfterValue = a.AfterValue,
                IpAddress = a.IpAddress,
                AdditionalInfo = a.AdditionalInfo
            }).ToList();

            return new ResponseObjectJsonDto
            {
                Message = $"Audit logs for {request.EntityName} {request.EntityId}",
                Code = 200,
                Response = auditLogDtos
            };
        }
        catch (Exception ex)
        {
            return new ResponseObjectJsonDto
            {
                Code = 500,
                Message = "Exception while retrieving audit logs: " + ex.Message,
                Response = null
            };
        }
    }
}
