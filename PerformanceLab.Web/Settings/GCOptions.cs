public class GCOptions
{
    public bool IsServerGC { get; set; } = true;
    public string LatencyMode { get; set; } = "Interactive";
    public bool ConcurrentGC { get; set; } = true;
    public int? HeapHardLimit { get; set; }
    public int? HeapHardLimitPercent { get; set; }
}