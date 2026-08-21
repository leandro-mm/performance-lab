using Microsoft.AspNetCore.Mvc;
using BenchmarkDotNet.Running;
using PerformanceLab.Benchmarks.Classes;
using BenchmarkDotNet.Reports;
using PerformanceLab.Web.Models;
using BenchmarkDotNet.Configs;

namespace PerformanceLab.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BenchmarkController : ControllerBase
{
    private readonly ILogger<BenchmarkController> _logger;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static BenchmarkResultDto? _lastResult;
    private static DateTime _lastRunTime;
    private static bool _isRunning;
    public BenchmarkController(ILogger<BenchmarkController> logger)
        => _logger = logger;


    [HttpPost("run")]
    public async Task<IActionResult> RunBenchmark()
    {
        try
        {
            if (_isRunning)
            {
                return Ok(new
                {
                    Status = "Executando",
                    Message = "Um benchmark já está em execução. Aguarde."
                });
            }

            _ = Task.Run(async () => await RunBenchmarkInBackground());

            return Ok(new
            {
                Status = "Iniciado",
                Message = "Benchmark iniciado em background."
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no benchmark");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("results")]
    public IActionResult GetBenchmarkResults()
    {
        if (_lastResult == null)
        {
            return Ok(new { HasResults = false, Message = "Nenhum benchmark executado ainda." });
        }

        return Ok(new
        {
            HasResults = true,
            LastRun = _lastRunTime,
            Results = _lastResult
        });
    }
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            IsRunning = _isRunning,
            LastRun = _lastRunTime,
            HasResults = _lastResult != null
        });
    }
    private object GetRecommendations(Summary summary)
    {
        // Lógica para sugerir melhorias baseado nos resultados
        return new
        {
            Tip = "Considere usar StringBuilder para concatenações em loops",
            summary.Reports.FirstOrDefault()?.GcStats.Gen0Collections
        };

    }
    private async Task RunBenchmarkInBackground()
    {
        await _semaphore.WaitAsync();
        try
        {
            _isRunning = true;
            _logger.LogInformation("🔄 Iniciando benchmark...");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Configuração para executar in-process (dentro do mesmo processo)
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            // Executar o benchmark
            var summary = BenchmarkRunner.Run<StringProcessingBenchmark>(config);

            stopwatch.Stop();

            // Processar resultados
            _lastResult = new BenchmarkResultDto
            {
                Reports = summary.Reports.Select(r => new BenchmarkReportDto
                {
                    DisplayInfo = r.BenchmarkCase.DisplayInfo,
                    Mean = r.ResultStatistics?.Mean,
                    StandardDeviation = r.ResultStatistics?.StandardDeviation,
                    AllocatedMemory = r.Metrics?.FirstOrDefault(m => m.Key == "Allocated Memory").Value?.Value,
                    Gen0 = r.GcStats.Gen0Collections,
                    Gen1 = r.GcStats.Gen1Collections,
                    Gen2 = r.GcStats.Gen2Collections
                }).ToList(),
                Recommendations = new BenchmarkRecommendationsDto
                {
                    Tip = "Considere usar StringBuilder para concatenações em loops",
                    Gen0Collections = summary.Reports.FirstOrDefault()?.GcStats.Gen0Collections
                },
                DurationMs = stopwatch.ElapsedMilliseconds,
                ExecutionTime = DateTime.Now,
                TotalBenchmarks = summary.Reports.Count(),
                SuccessfulBenchmarks = summary.Reports.Count(r => r.ResultStatistics != null)
            };

            _lastRunTime = DateTime.Now;
            _logger.LogInformation($"✅ Benchmark concluído em {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante execução do benchmark: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
        }
        finally
        {
            _isRunning = false;
            _semaphore.Release();
        }
    }
}