// NOTE: This controller is included for demonstration purposes to showcase monitoring capabilities.
// In a production microservice architecture, these health checks and metrics would typically be handled
// by a dedicated external service, as this application's core responsibility is file management.

using Application.Common.Interfaces;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPerformanceMetricsService _metricsService;

    public MetricsController(IMediator mediator, IPerformanceMetricsService metricsService)
    {
        _mediator = mediator;
        _metricsService = metricsService;
    }

    /// <summary>
    /// Retrieves performance metrics for the application.
    /// </summary>
    /// <response code="200">Returns the current performance metrics.</response>
    /// <response code="403">Insufficient permissions (admin only).</response>
    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult GetMetrics()
    {
        var metrics = _metricsService.GetMetrics();
        return Ok(metrics);
    }

    /// <summary>
    /// Resets all performance metrics to their initial state.
    /// </summary>
    /// <response code="200">Metrics reset successfully.</response>
    /// <response code="403">Insufficient permissions (admin only).</response>
    [HttpPost("reset")]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult ResetMetrics()
    {
        _metricsService.ResetMetrics();
        return Ok(new { message = "Metrics reset successfully" });
    }
}
