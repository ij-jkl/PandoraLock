using Application.Common.Interfaces;
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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetMetrics()
    {
        var metrics = _metricsService.GetMetrics();
        return Ok(metrics);
    }

    [HttpPost("reset")]
    [Authorize(Roles = "Admin")]
    public IActionResult ResetMetrics()
    {
        _metricsService.ResetMetrics();
        return Ok(new { message = "Metrics reset successfully" });
    }
}
