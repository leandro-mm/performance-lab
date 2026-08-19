
using BenchmarkDotNet.Running;
using PerformanceLab.Benchmark.Classes;

Console.WriteLine("Hello, World!");

var summaryStringProcessing = BenchmarkRunner.Run<StringProcessingBenchmark>();
var summaryCalcProcessing = BenchmarkRunner.Run<CalcBenchmark>();

