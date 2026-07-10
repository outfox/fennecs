using BenchmarkDotNet.Attributes;

namespace Benchmark.Conceptual;

[ShortRunJob]
public class SimpleArrayBenchmarks
{
    [Params(1_000, 1_000_000)] 
    public int entityCount { get; set; }

    private static readonly Random random = new(1337);

    private int[] _input = null!;
    private int[] _output = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _input = Enumerable.Range(0, entityCount).Select(_ => random.Next()).ToArray();
        _output = new int[entityCount];
    }

    [Benchmark]
    public void BulkCopy()
    {
        Array.Copy(_input, _output, entityCount);
    }

    [Benchmark]
    public void PerItemCopy()
    {
        for (var i = 0; i < entityCount; i++)
        {
            _output[i] = _input[i];
        }
    }

    [Benchmark]
    public void PerItemCopyReadonlySpan()
    {
        var input = new ReadOnlySpan<int>(_input);
        var output = _output.AsSpan();
        for (var i = 0; i < entityCount; i++)
        {
            output[i] = input[i];
        }
    }

    [Benchmark]
    public void PerItemCopySpan()
    {
        var input = _input.AsSpan();
        var output = _output.AsSpan();
        for (var i = 0; i < entityCount; i++)
        {
            output[i] = input[i];
        }
    }

    
    
    [Benchmark]
    public void PerItemCopyParallel()
    {
        Parallel.For((long) 0, entityCount, i =>
        {
            _output[i] = _input[i];
        });
    }

    [Benchmark]
    public void LocalModulus()
    {
        for (var i = 0; i < entityCount; i++)
        {
            _input[i] %= 100;
        }
    }

    [Benchmark]
    public void RollingModulus()
    {
        for (var i = 0; i < entityCount - 1; i++)
        {
            _input[i + 1] = _input[i] % 100;
        }
    }

    [Benchmark]
    public void RollingModulusArray()
    {
        var input = _input;
        var output = _output;
        for (var i = 0; i < entityCount - 1; i++)
        {
            output[i] = input[i] % 100;
        }
    }

    [Benchmark]
    public void RollingModulusSpan()
    {
        var input = _input.AsSpan();
        var output = _output.AsSpan();
        for (var i = 0; i < entityCount - 1; i++)
        {
            output[i] = input[i] % 100;
        }
    }

}