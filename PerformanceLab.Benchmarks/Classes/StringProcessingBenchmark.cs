using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace PerformanceLab.Benchmarks.Classes;

[MemoryDiagnoser] // Mede alocações de memória
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringProcessingBenchmark
{
    private const int Iterations = 10000;

    private BeforeAndAfter _beforeAndAfter = null!;

    [GlobalSetup]
    public void Setup()
    {
        _beforeAndAfter = new BeforeAndAfter();
    }

    [Benchmark(Baseline = true)]
    public string ProcessString_Bad() => _beforeAndAfter.ProcessLargeString_Bad(Iterations);

    [Benchmark]
    public string ProcessString_Good() => _beforeAndAfter.ProcessLargeString_Good(Iterations);

}