using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using PerformanceLab.Web.Hubs;

public class MetricsCollector : BackgroundService
{
    private readonly ILogger<MetricsCollector> _logger;
    private readonly IHubContext<MetricsHub> _hubContext;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
    private readonly GCOptions _gcOptionsDirect;

    public MetricsCollector(
        ILogger<MetricsCollector> logger,
        IHubContext<MetricsHub> hub,
        GCOptions gcOptionsDirect)
    {
        _logger = logger;
        _hubContext = hub;
        _gcOptionsDirect = gcOptionsDirect;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var metrics = CollectMetrics();

                // ✅ Envia via IHubContext (NÃO usa SendMetricsAsync)
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveMetrics", // ← Nome do método no cliente
                    metrics,
                    cancellationToken: stoppingToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao coletar métricas");
            }

            await Task.Delay(_interval, stoppingToken);
        }

    }

    private PerformanceMetrics CollectMetrics()
    {
        var process = Process.GetCurrentProcess();

        // Métricas de Memória
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalMemory = GC.GetTotalMemory(false);

        // Métricas de CPU
        var cpuUsage = GetCpuUsage();

        // Métricas de GC
        var gcCollections = new int[3];
        for (int i = 0; i < 3; i++)
            gcCollections[i] = GC.CollectionCount(i);


        // configurações injetadas
        var isServerGC = AppContext.TryGetSwitch("System.GC.Server", out var serverGC) && serverGC;

        return new PerformanceMetrics
        {
            Timestamp = DateTime.UtcNow,
            MemoryUsedMB = totalMemory / (1024 * 1024),
            ManagedMemoryMB = process.WorkingSet64 / (1024 * 1024),
            PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
            CpuUsagePercent = cpuUsage,

            // GC Stats
            Gen0Collections = gcCollections[0],
            Gen1Collections = gcCollections[1],
            Gen2Collections = gcCollections[2],
            GcMode = isServerGC ? "Server GC" : "Workstation GC",
            GcLatencyMode = _gcOptionsDirect.LatencyMode ?? "Interactive",

            // Threads
            ThreadCount = process.Threads.Count,
            PendingFinalizers = 0 // Apenas ilustrativo
        };
    }

    private double GetCpuUsage()
    {
        // Implementação simplificada - em produção usar PerformanceCounter
        var process = Process.GetCurrentProcess();
        var totalProcessorTime = process.TotalProcessorTime;
        var startTime = DateTime.Now;

        Thread.Sleep(500); // Espera para medir delta

        var endTotalProcessorTime = process.TotalProcessorTime;
        var endTime = DateTime.Now;

        var cpuUsedMs = (endTotalProcessorTime - totalProcessorTime).TotalMilliseconds;
        var totalMs = (endTime - startTime).TotalMilliseconds;

        return Math.Round(cpuUsedMs / (Environment.ProcessorCount * totalMs) * 100, 2);
    }
}