public record PerformanceMetrics
{
    public DateTime Timestamp { get; init; }
    public long MemoryUsedMB { get; init; }
    public long ManagedMemoryMB { get; init; }
    public long PrivateMemoryMB { get; init; }
    public double CpuUsagePercent { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
    public string GcMode { get; init; } = string.Empty;
    public string GcLatencyMode { get; init; } = string.Empty;
    public int ThreadCount { get; init; }
    public int PendingFinalizers { get; init; }

}
