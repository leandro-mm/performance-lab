using Microsoft.AspNetCore.Mvc;
using BenchmarkDotNet.Running;
using PerformanceLab.Benchmark.Classes;
using BenchmarkDotNet.Reports;  // Add this using statement

namespace PerformanceLab.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BenchmarkController : ControllerBase
{
    private readonly ILogger<BenchmarkController> _logger;

    public BenchmarkController(ILogger<BenchmarkController> logger)
        => _logger = logger;


    [HttpPost("run")]
    public async Task<IActionResult> RunBenchmark()
    {
        try
        {
            // Executa benchmark em background
            var summary = BenchmarkRunner.Run<StringProcessingBenchmark>();

            var result = new
            {
                Reports = summary.Reports.Select(r => new
                {
                    r.BenchmarkCase.DisplayInfo,
                    r.ResultStatistics?.Mean,
                    r.ResultStatistics?.StandardDeviation,
                    AllocatedMemory = r.Metrics?.FirstOrDefault(m => m.Key == "Allocated Memory").Value?.Value
                }),
                Recommendations = GetRecommendations(summary)
            };

            return Ok(result);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no benchmark");
            return StatusCode(500, ex.Message);
        }
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
    [HttpPost("simulate-leak")]
    public IActionResult SimulateMemoryLeak([FromQuery] int mb = 5)
    {
        try
        {
            // Allocate memory to simulate a leak
            var memoryToAllocate = mb * 1024 * 1024; // Convert MB to bytes
            var data = new byte[memoryToAllocate];

            // Fill with random data to prevent optimization
            new Random().NextBytes(data);

            // Store in static list to prevent garbage collection
            _memoryLeakStorage.Add(data);

            Console.WriteLine($"💣 Memory leak simulated: {mb}MB allocated. Total leaked: {_memoryLeakStorage.Sum(x => x.Length) / (1024 * 1024)}MB");

            return Ok(new
            {
                Message = $"Memory leak of {mb}MB simulated successfully",
                AllocatedMB = mb,
                TotalLeakedMB = _memoryLeakStorage.Sum(x => x.Length) / (1024 * 1024)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("clear-leak")]
    public IActionResult ClearMemoryLeak()
    {
        var totalBefore = _memoryLeakStorage.Sum(x => x.Length) / (1024 * 1024);
        _memoryLeakStorage.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        return Ok(new
        {
            Message = "Memory leak cleared",
            FreedMB = totalBefore
        });
    }
    private static readonly List<byte[]> _memoryLeakStorage = new();
}