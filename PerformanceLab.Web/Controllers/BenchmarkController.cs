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
}