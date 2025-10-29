namespace Application.Common.Models;

public class PerformanceMetrics
{
    public long TotalRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public long SlowRequests { get; set; }
    public long FailedRequests { get; set; }
    public DateTime LastResetTime { get; set; }
    public Dictionary<string, long> EndpointHitCount { get; set; } = new();
}
