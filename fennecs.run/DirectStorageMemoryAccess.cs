using BenchmarkDotNet.Attributes;

namespace fennecs.run;

public class MemStore<T>(int size) where T : notnull
{
    private readonly T[] _data = new T[size];
    
    public ref T this[int index] => ref _data[index];
    
    public ReadOnlySpan<T> ReadOnlySpan => _data.AsSpan();
    public Span<T> Span => _data.AsSpan();
}


public class DirectStorageMemoryAccess
{
    [Params(10_000)]
    public int count { get; set; }

    private MemStore<int> _mem = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(69);
        _mem = new(count);
        for (var i = 0; i < count; i++) _mem.Span[i] = random.Next();
    }
    
    [Benchmark]
    public int SpanCached()
    {
        var sum = 0;
        var ros = _mem.Span;
        for (var i = 0; i < count; i++)
        {
            sum += ros[i];
        }
        return sum;
    }

    [Benchmark]
    public int ReadOnlySpanCached()
    {
        var sum = 0;
        var ros = _mem.ReadOnlySpan;
        for (var i = 0; i < count; i++)
        {
            sum += ros[i];
        }
        return sum;
    }

    [Benchmark]
    public int ReadOnlySpanDirect()
    {
        var sum = 0;
        for (var i = 0; i < count; i++)
        {
            sum += _mem.ReadOnlySpan[i];
        }
        return sum;
    }

    [Benchmark]
    public int SpanDirect()
    {
        var sum = 0;
        for (var i = 0; i < count; i++)
        {
            sum += _mem.Span[i];
        }
        return sum;
    }

    [Benchmark]
    public int ReadIndexer()
    {
        var sum = 0;
        for (var i = 0; i < count; i++)
        {
            sum += _mem[i];
        }
        return sum;
    }
}
