using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

[ShortRunJob]
[MemoryDiagnoser]
public class V3Benchmarks
{
    [Params(1_000, 1_000_000)] 
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int entityCount { get; set; }

    private static readonly Random random = new(1337);

    private Vector3[] _input = null!;
    private float[] _output = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _input = Enumerable.Range(0, entityCount).Select(_ => new Vector3(random.Next(), random.Next(), random.Next())).ToArray();
        _output = new float[entityCount];

        _incrementDelegate = VectorIncrement;
    }

    [Benchmark]
    public void PerItemDot()
    {
        for (var i = 0; i < entityCount; i++)
        {
            _output[i] = Vector3.Dot(_input[i], new Vector3(1, 2, 3));
        }
    }

    [Benchmark]
    public void PerItemIncrementArray()
    {
        var lim = _input.Length;
        for (var i = 0; i < lim; i++)
        {
            _input[i] += new Vector3(1, 2, 3);
        }
    }

    [Benchmark]
    public void PerItemIncrementSpan()
    {
        var span = _input.AsSpan();
        var lim = span.Length;
        for (var i = 0; i < lim; i++)
        {
            span[i] += new Vector3(1, 2, 3);
        }
    }

    [Benchmark]
    public void PerItemIncrementSpanRef()
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            v += new Vector3(1, 2, 3);
        }
    }

    private void VectorIncrement(ref Vector3 val)
    {
        val += new Vector3(1, 2, 3);   
    }

    [Benchmark]
    public void PerItemIncrementSpanCall()
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            VectorIncrement(ref v);
        }
    }

    private delegate void VectorIncrementDelegate<T>(ref T val);

    private VectorIncrementDelegate<Vector3> _incrementDelegate = null!;
    
    [Benchmark]
    public void PerItemIncrementSpanDelegate()
    {
        PerItemIncrementSpanDelegateImpl(_incrementDelegate);
    }

    [Benchmark]
    public void PerItemIncrementSpanLambda()
    {
        PerItemIncrementSpanDelegateImpl((ref Vector3 val) => { val += new Vector3(1, 2, 3); });
    }
    
    [Benchmark]
    public void PerItemIncrementSpanLocalDelegate()
    {
        VectorIncrementDelegate<Vector3> del = (ref Vector3 val) => { val += new Vector3(1, 2, 3); };
        PerItemIncrementSpanDelegateImpl(del);
    }

    [Benchmark]
    public void PerItemIncrementSpanLocalFunction()
    {
        PerItemIncrementSpanDelegateImpl(Del);
        return;

        void Del(ref Vector3 val)
        {
            val += new Vector3(1, 2, 3);
        }
    }


    private void PerItemIncrementSpanDelegateImpl(VectorIncrementDelegate<Vector3> del)
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            del(ref v);
        }
    }

    [Benchmark]
    public void PerItemDotParallel()
    {
        Parallel.For(0, entityCount, i => { _output[i] = Vector3.Dot(_input[i], new Vector3(1, 2, 3)); });
    }

    [Benchmark]
    public void PerItemDotSpan()
    {
        var va = new Vector3(1, 2, 3);
        var input = _input.AsSpan();
        var output = _output.AsSpan();
        for (var i = 0; i < entityCount; i++)
        {
            output[i] = Vector3.Dot(input[i], va);
        }
    }
}