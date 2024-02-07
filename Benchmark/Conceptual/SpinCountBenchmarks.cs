using BenchmarkDotNet.Attributes;

namespace Benchmark;


[ShortRunJob]
public class SpinCountBenchmarks
{
    [Params(1, 100, 1_000, 10_000, 100_000, 1_000_000, 10_0000)] 
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int spinCount { get; set; }

    [Benchmark]
    public void Thread_SpinWait()
    {
        Thread.SpinWait(spinCount);
    }

}