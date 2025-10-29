using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IPerformanceMetricsService
{
    void RecordRequest(string endpoint, long durationMs, bool isSuccess);
    PerformanceMetrics GetMetrics();
    void ResetMetrics();
}
