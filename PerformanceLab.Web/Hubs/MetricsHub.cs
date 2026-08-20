using Microsoft.AspNetCore.SignalR;

namespace PerformanceLab.Web.Hubs;

public class MetricsHub : Hub
{
    private static readonly List<PerformanceMetrics> _history = new();
    public async Task SendMetricsAsync(PerformanceMetrics metrics)
    {
        _history.Add(metrics);
        if (_history.Count > 100) _history.RemoveAt(0);

        await Clients.All.SendAsync("ReceiveMetrics", metrics);
        await Clients.All.SendAsync("ReceiveHistory", _history);
    }
}
