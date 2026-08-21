namespace PerformanceLab.Web.Models;



public class BenchmarkReportDto
{
    public string DisplayInfo { get; set; } = "";
    public double? Mean { get; set; }
    public double? StandardDeviation { get; set; }
    public object? AllocatedMemory { get; set; }
    public int? Gen0 { get; set; }
    public int? Gen1 { get; set; }
    public int? Gen2 { get; set; }
}

