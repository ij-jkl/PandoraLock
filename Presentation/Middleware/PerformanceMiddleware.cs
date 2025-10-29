using System.Diagnostics;
using Application.Common.Interfaces;

namespace Presentation.Middleware;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IPerformanceMetricsService metricsService)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        try
        {
            await _next(context);

            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;
            var isSuccess = statusCode >= 200 && statusCode < 400;

            metricsService.RecordRequest($"{requestMethod} {requestPath}", elapsedMilliseconds, isSuccess);

            if (elapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow HTTP Request : {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "HTTP Request : {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metricsService.RecordRequest($"{requestMethod} {requestPath}", stopwatch.ElapsedMilliseconds, false);
            
            _logger.LogError(ex,
                "HTTP Request : {Method} {Path} failed after {ElapsedMilliseconds} ms",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
