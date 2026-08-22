namespace PerformanceLab.Web.Models;

public class BenchmarkResultDto
{
    public List<BenchmarkReportDto> Reports { get; set; } = new();
    public BenchmarkRecommendationsDto? Recommendations { get; set; }
    public long DurationMs { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int TotalBenchmarks { get; set; }
    public int SuccessfulBenchmarks { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsRunning { get; set; }
    public bool HasResults { get; set; }
}
