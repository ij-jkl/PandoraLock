using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using MediatR;

namespace Application.AuditLogs.Queries;

public class GetAuditLogsByDateRangeQuery : IRequest<ResponseObjectJsonDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GetAuditLogsByDateRangeQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

public class GetAuditLogsByDateRangeQueryHandler : IRequestHandler<GetAuditLogsByDateRangeQuery, ResponseObjectJsonDto>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsByDateRangeQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ResponseObjectJsonDto> Handle(GetAuditLogsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var auditLogs = await _auditLogRepository.GetByDateRangeAsync(request.StartDate, request.EndDate);

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
                Message = $"Audit logs from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}",
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
