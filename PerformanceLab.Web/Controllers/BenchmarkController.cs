using Microsoft.AspNetCore.Mvc;
using BenchmarkDotNet.Running;
using PerformanceLab.Benchmarks.Classes;
using BenchmarkDotNet.Reports;
using PerformanceLab.Web.Models;
using BenchmarkDotNet.Configs;
using System.Diagnostics;

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
    private static CancellationTokenSource? _cancellationTokenSource;

    public BenchmarkController(ILogger<BenchmarkController> logger)
        => _logger = logger;


    [HttpPost("run")]
    public async Task<IActionResult> RunBenchmark()
    {
        try
        {
            if (_isRunning)
            {
                return Ok(new BenchmarkResponseDto
                {
                    Status = "Executando",
                    Message = "Um benchmark já está em execução. Aguarde.",
                    CanCancel = true
                });
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _ = Task.Run(async () => await RunBenchmarkInBackground(_cancellationTokenSource.Token));

            return Ok(new BenchmarkResponseDto
            {
                Status = "Iniciado",
                Message = "Benchmark iniciado em background.",
                CanCancel = true
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no benchmark");
            return StatusCode(500, new BenchmarkResponseDto { Status = "Undefined", Message = ex.Message });
        }
    }

    [HttpGet("results")]
    public IActionResult GetBenchmarkResults()
    {
        if (_lastResult == null)
        {
            if (_isRunning)
            {
                return Ok(new BenchmarkResultDto
                {
                    IsRunning = true,
                    IsCancelled = false
                });
            }

            return Ok(new BenchmarkResultDto
            {
                IsRunning = false,
                IsCancelled = false,
                HasResults = false
            });
        }

        return Ok(_lastResult);
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new BenchmarkResponseDto
        {
            IsRunning = _isRunning,
            LastRun = _lastRunTime,
            HasResults = _lastResult != null,
            CanCancel = _isRunning && _cancellationTokenSource != null
        });
    }

    private async Task RunBenchmarkInBackground(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _isRunning = true;
            _logger.LogInformation("🔄 Iniciando benchmark...");

            //compiles and runs the benchmark in the current process using Emit (Reflection.Emit)
            var config = new DebugInProcessConfig();

            var summary = await RunBenchmarkWithCancellationAsync(config, cancellationToken);

            stopwatch.Stop();

            if (cancellationToken.IsCancellationRequested)
            {
                _lastResult = ObterBenchMarkObjetoVazio(stopwatch);
            }
            else
            {
                _lastResult = ObterBenchMarkObjetoComDados(stopwatch, summary);
                _logger.LogInformation($"✅ Benchmark concluído em {stopwatch.ElapsedMilliseconds}ms");
            }

            _lastRunTime = DateTime.Now;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Benchmark cancelado via OperationCanceledException.");
            _lastResult = ObterBenchMarkObjetoVazio(stopwatch);
            _lastRunTime = DateTime.Now;
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

    private BenchmarkResultDto? ObterBenchMarkObjetoComDados(Stopwatch stopwatch, Summary summary)
    {
        return new BenchmarkResultDto
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
    }


    private BenchmarkResultDto? ObterBenchMarkObjetoVazio(Stopwatch stopwatch)
    {
        return new BenchmarkResultDto
        {
            Reports = new List<BenchmarkReportDto>(),
            Recommendations = new BenchmarkRecommendationsDto
            {
                Tip = "Execução cancelada pelo usuário."
            },
            DurationMs = stopwatch.ElapsedMilliseconds,
            ExecutionTime = DateTime.Now,
            TotalBenchmarks = 0,
            SuccessfulBenchmarks = 0,
            IsCancelled = true
        };
    }

    private async Task<Summary> RunBenchmarkWithCancellationAsync(IConfig config, CancellationToken cancellationToken)

    {
        // Iniciar a tarefa do benchmark
        var benchmarkTask = Task.Run(() => BenchmarkRunner.Run<StringProcessingBenchmark>(config));

        // Aguardar o benchmark ou o cancelamento
        var cancellationTask = Task.Delay(-1, cancellationToken);

        var completedTask = await Task.WhenAny(benchmarkTask, cancellationTask);

        if (completedTask == benchmarkTask)
        {
            return await benchmarkTask;
        }
        else
        {
            // Cancellation was requested
            throw new OperationCanceledException("Benchmark cancelado pelo usuário.", cancellationToken);
        }
    }

    [HttpPost("cancel")]
    public IActionResult CancelBenchmark()
    {
        try
        {
            if (!_isRunning)
            {
                return Ok(new BenchmarkResponseDto
                {
                    Status = "Parado",
                    Message = "Nenhum benchmark em execução para cancelar."
                });
            }

            if (_cancellationTokenSource == null)
            {
                return Ok(new BenchmarkResponseDto
                {
                    Status = "Erro",
                    Message = "Token de cancelamento não disponível."
                });
            }

            // ✅ Solicitar cancelamento
            _cancellationTokenSource.Cancel();

            _logger.LogInformation("🛑 Cancelamento solicitado para o benchmark em execução.");

            return Ok(new
            {
                Status = "Cancelando",
                Message = "Cancelamento solicitado. O benchmark será interrompido em breve."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar benchmark");
            return StatusCode(500, ex.Message);
        }
    }
}