using Application.AuditLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityName, string entityId)
    {
        var query = new GetAuditLogsByEntityQuery(entityName, entityId);
        var result = await _mediator.Send(query);
        return StatusCode(result.Code, result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var query = new GetAuditLogsByUserQuery(userId);
        var result = await _mediator.Send(query);
        return StatusCode(result.Code, result);
    }

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
