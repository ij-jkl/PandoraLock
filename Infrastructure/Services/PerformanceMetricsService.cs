using Application.Common.Interfaces;
using Application.Common.Models;

namespace Infrastructure.Services;

public class PerformanceMetricsService : IPerformanceMetricsService
{
    private long _totalRequests;
    private double _totalDuration;
    private long _slowRequests;
    private long _failedRequests;
    private DateTime _lastResetTime;
    private readonly Dictionary<string, long> _endpointHitCount;
    private readonly object _lock = new();

    public PerformanceMetricsService()
    {
        _totalRequests = 0;
        _totalDuration = 0;
        _slowRequests = 0;
        _failedRequests = 0;
        _lastResetTime = DateTime.UtcNow;
        _endpointHitCount = new Dictionary<string, long>();
    }

    public void RecordRequest(string endpoint, long durationMs, bool isSuccess)
    {
        lock (_lock)
        {
            _totalRequests++;
            _totalDuration += durationMs;

            if (durationMs > 1000)
            {
                _slowRequests++;
            }

            if (!isSuccess)
            {
                _failedRequests++;
            }

            if (_endpointHitCount.ContainsKey(endpoint))
            {
                _endpointHitCount[endpoint]++;
            }
            else
            {
                _endpointHitCount[endpoint] = 1;
            }
        }
    }

    public PerformanceMetrics GetMetrics()
    {
        lock (_lock)
        {
            return new PerformanceMetrics
            {
                TotalRequests = _totalRequests,
                AverageResponseTime = _totalRequests > 0 ? _totalDuration / _totalRequests : 0,
                SlowRequests = _slowRequests,
                FailedRequests = _failedRequests,
                LastResetTime = _lastResetTime,
                EndpointHitCount = new Dictionary<string, long>(_endpointHitCount)
            };
        }
    }

    public void ResetMetrics()
    {
        lock (_lock)
        {
            _totalRequests = 0;
            _totalDuration = 0;
            _slowRequests = 0;
            _failedRequests = 0;
            _lastResetTime = DateTime.UtcNow;
            _endpointHitCount.Clear();
        }
    }
}
