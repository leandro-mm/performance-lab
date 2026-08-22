using PerformanceLab.Web.Models;

public class BenchmarkResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool CanCancel { get; set; }
    public bool IsRunning { get; set; }
    public DateTime LastRun { get; set; }
    public bool HasResults { get; set; }
    public BenchmarkResultDto? Results { get; set; }

}