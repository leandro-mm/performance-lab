// Services/IDashboardService.cs
using Microsoft.AspNetCore.Components;
using PerformanceLab.Web.Models;

public interface IDashboardService
{
    event Action<PerformanceMetrics>? OnMetricsReceived;
    event Action<List<PerformanceMetrics>>? OnHistoryReceived;
    event Action<bool, string>? OnConnectionStatusChanged;
    event Action<bool>? OnBenchmarkRunningChanged;
    event Action<BenchmarkResultDto?>? OnBenchmarkResultReceived;

    PerformanceMetrics? CurrentMetrics { get; }
    List<PerformanceMetrics> History { get; }
    bool IsConnected { get; }
    bool IsBenchmarkRunning { get; }
    bool IsCleanButtonDisabled { get; set; }

    Task InitializeAsync(NavigationManager navigation);
    Task RunBenchmarkAsync();
    Task CancelBenchmarkAsync();
    Task SimulateMemoryLeakAsync(int mb = 5);
    Task CleanMemoryLeakAsync();
    Task CheckBenchmarkStatusAsync();
    Task LoadBenchmarkResultsAsync();
    ValueTask DisposeAsync();
}