using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;

namespace PerformanceLab.Benchmarks.Classes;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SimpleJob(RunStrategy.Monitoring, invocationCount: 1, warmupCount: 0)]//cada benchmark executa apenas uma vez
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