using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Common.Behaviors;

public class PerformanceMonitoringBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceMonitoringBehavior(ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var requestName = typeof(TRequest).Name;

        try
        {
            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "Long Running Request : {RequestName} ({ElapsedMilliseconds} milliseconds)",
                    requestName,
                    elapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "Request : {RequestName} completed in {ElapsedMilliseconds} milliseconds",
                    requestName,
                    elapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            _timer.Stop();
            _logger.LogError(ex,
                "Request : {RequestName} failed after {ElapsedMilliseconds} milliseconds",
                requestName,
                _timer.ElapsedMilliseconds);
            throw;
        }
    }
}
