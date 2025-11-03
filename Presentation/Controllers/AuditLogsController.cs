using Application.AuditLogs.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.AuditLogs.Read)]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves audit logs for a specific entity.
    /// </summary>
    /// <response code="200">Returns the audit logs for the specified entity.</response>
    /// <response code="403">Insufficient permissions to read audit logs.</response>
    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityName, string entityId)
    {
        var query = new GetAuditLogsByEntityQuery(entityName, entityId);
        var result = await _mediator.Send(query);
        return StatusCode(result.Code, result);
    }

    /// <summary>
    /// Retrieves all audit logs associated with a specific user.
    /// </summary>
    /// <response code="200">Returns the audit logs for the specified user.</response>
    /// <response code="403">Insufficient permissions to read audit logs.</response>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var query = new GetAuditLogsByUserQuery(userId);
        var result = await _mediator.Send(query);
        return StatusCode(result.Code, result);
    }

    /// <summary>
    /// Retrieves audit logs within a specified date range.
    /// </summary>
    /// <response code="200">Returns the audit logs within the specified date range.</response>
    /// <response code="400">Invalid date range.</response>
    /// <response code="403">Insufficient permissions to read audit logs.</response>
    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery, SwaggerSchema(Format = "date-time")] DateTime startDate,
        [FromQuery, SwaggerSchema(Format = "date-time")] DateTime endDate)
    {
        var query = new GetAuditLogsByDateRangeQuery(startDate, endDate);
        var result = await _mediator.Send(query);
        return StatusCode(result.Code, result);
    }
}
