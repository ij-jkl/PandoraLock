using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.AuditLogs.Queries;

public class GetAuditLogsByUserQuery : IRequest<ResponseObjectJsonDto>
{
    public int UserId { get; set; }

    public GetAuditLogsByUserQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAuditLogsByUserQueryHandler : IRequestHandler<GetAuditLogsByUserQuery, ResponseObjectJsonDto>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsByUserQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetAuditLogsByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var auditLogs = await _auditLogRepository.GetByUserIdAsync(request.UserId);

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
                Message = $"Audit logs for user {request.UserId}",
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
