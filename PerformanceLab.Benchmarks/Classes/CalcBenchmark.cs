using BenchmarkDotNet.Attributes;
namespace PerformanceLab.Benchmark.Classes;

[MemoryDiagnoser]
public class CalcBenchmark
{
    private int[] _numbers = null!;
    private BeforeAndAfter _beforeAndAfter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _beforeAndAfter = new BeforeAndAfter();

        var rand = new Random(42);
        _numbers = Enumerable.Range(1, 1000000).Select(_ => rand.Next(1000)).ToArray();
    }

    [Benchmark(Baseline = true)]
    public decimal CalculateAverage_Bad() =>
        _beforeAndAfter.CalculateAverage_Bad(_numbers);

    [Benchmark]
    public decimal CalculateAverage_Good() =>
        _beforeAndAfter.CalculateAverage_Good(_numbers);
}