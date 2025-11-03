// NOTE: This controller is included for demonstration purposes to showcase monitoring capabilities.
// In a production microservice architecture, these health checks and metrics would typically be handled
// by a dedicated external service, as this application's core responsibility is file management.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Retrieves the current health status of the application and its dependencies.
    /// </summary>
    /// <response code="200">Application is healthy.</response>
    /// <response code="503">Application or one of its dependencies is unhealthy.</response>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHealth()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            status = healthReport.Status.ToString(),
            totalDuration = healthReport.TotalDuration.TotalMilliseconds,
            checks = healthReport.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message
            })
        };

        return healthReport.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
    }
}
