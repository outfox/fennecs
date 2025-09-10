using System.Numerics;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;

namespace Benchmark.Conceptual;

[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
//[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictions)]

public class LoopConventionBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(10_000)]
    public int EntityCount { get; set; }

    private static readonly Random Random = new(1337);

    private Vector3[] _vectorsRaw = null!;
    private int[] _intsRaw = null!;

    [GlobalSetup]
    public void Setup()
    {
        _vectorsRaw = new Vector3[EntityCount];
        _intsRaw = new int[EntityCount];

        for (var i = 0; i < EntityCount; i++)
        {
            _vectorsRaw[i] = new(Random.NextSingle(), Random.NextSingle(), Random.NextSingle());
            _intsRaw[i] = Random.Next() % 100;
        }
    }
    
    private const int Threshold = 20;

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);

    private delegate void RefAction<T1, T2>(ref T1 arg1, ref T2 arg2);
    private delegate bool RefFilter<T2>(ref T2 arg1);
    
    private bool FilterInt(ref int i)
    {
        return i >= Threshold;
    }
    
    private bool StaticFilterInt(ref int i)
    {
        return i >= Threshold;
    }
    
    private void Process(RefAction<Vector3, int> action)
    {
        var vectorsSpan = _vectorsRaw.AsSpan();
        var intsSpan = _intsRaw.AsSpan();
        
        for (var index = 0; index < EntityCount; index++)
        {
            action(ref vectorsSpan[index], ref intsSpan[index]);
        }
    }


    private void ProcessFiltered(RefFilter<int> filter, RefAction<Vector3, int> action)
    {
        var vectorsSpan = _vectorsRaw.AsSpan();
        var intsSpan = _intsRaw.AsSpan();
        
        for (var index = 0; index < EntityCount; index++)
        {
            if (filter(ref intsSpan[index])) action(ref vectorsSpan[index], ref intsSpan[index]);
        }
    }


    private void ProcessFilteredUnrolled(RefFilter<int> filter,RefAction<Vector3, int> action)
    {
        var vectorsSpan = _vectorsRaw.AsSpan();
        var intsSpan = _intsRaw.AsSpan();
        
        for (var index = 0; index < EntityCount; index+=8)
        {
            if (filter(ref intsSpan[index])) action(ref vectorsSpan[index], ref intsSpan[index]);
            if (filter(ref intsSpan[index+1])) action(ref vectorsSpan[index+1], ref intsSpan[index+1]);
            if (filter(ref intsSpan[index+2])) action(ref vectorsSpan[index+2], ref intsSpan[index+2]);
            if (filter(ref intsSpan[index+3])) action(ref vectorsSpan[index+3], ref intsSpan[index+3]);
            if (filter(ref intsSpan[index+4])) action(ref vectorsSpan[index+4], ref intsSpan[index+4]);
            if (filter(ref intsSpan[index+5])) action(ref vectorsSpan[index+5], ref intsSpan[index+5]);
            if (filter(ref intsSpan[index+6])) action(ref vectorsSpan[index+6], ref intsSpan[index+6]);
            if (filter(ref intsSpan[index+7])) action(ref vectorsSpan[index+7], ref intsSpan[index+7]);
        }
        
        for (var index = EntityCount - (EntityCount % 8); index < EntityCount; index++)
        {
            if (filter(ref intsSpan[index])) action(ref vectorsSpan[index], ref intsSpan[index]);
        }
    }

    

    private void ProcessUnrolled(RefAction<Vector3, int> action)
    {
        var vectorsSpan = _vectorsRaw.AsSpan();
        var intsSpan = _intsRaw.AsSpan();
        
        for (var index = 0; index < EntityCount; index+=8)
        {
            action(ref vectorsSpan[index], ref intsSpan[index]);
            action(ref vectorsSpan[index+1], ref intsSpan[index+1]);
            action(ref vectorsSpan[index+2], ref intsSpan[index+2]);
            action(ref vectorsSpan[index+3], ref intsSpan[index+3]);
            action(ref vectorsSpan[index+4], ref intsSpan[index+4]);
            action(ref vectorsSpan[index+5], ref intsSpan[index+5]);
            action(ref vectorsSpan[index+6], ref intsSpan[index+6]);
            action(ref vectorsSpan[index+7], ref intsSpan[index+7]);
        }
        
        for (var index = EntityCount - (EntityCount % 8); index < EntityCount; index++)
        {
            action(ref vectorsSpan[index], ref intsSpan[index]);
        }
    }


    [Benchmark]
    public int Lambda()
    {
        var count = 0;
        
        Process((ref Vector3 v, ref int i) =>
        {
            if (i >= Threshold) i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int LambdaFiltered()
    {
        var count = 0;
        
        ProcessFiltered(FilterInt, (ref Vector3 v, ref int i) =>
        {
            i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int LambdaFilteredUnrolled()
    {
        var count = 0;
        
        ProcessFilteredUnrolled(FilterInt, (ref Vector3 v, ref int i) =>
        {
            i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int LambdaFilteredDelegate()
    {
        var count = 0;
        
        ProcessFiltered(FilterInt, delegate (ref Vector3 v, ref int i)
        {
            i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int LambdaDelegate()
    {
        var count = 0;
        
        Process(delegate (ref Vector3 v, ref int i)
        {
            if (i >= Threshold) i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark(Baseline = true)]
    public int LambdaUnroll()
    {
        var count = 0;
        
        ProcessUnrolled((ref Vector3 v, ref int i) =>
        {
            if (i >= Threshold) i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int LambdaDelegateUnroll()
    {
        var count = 0;
        
        ProcessUnrolled(delegate (ref Vector3 v, ref int i)
        {
            if (i >= Threshold) i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int Method()
    {
        var count = 0;
        
        Process(MethodInt);
        
        return count;
    }

    [Benchmark]
    public int MethodUnroll()
    {
        var count = 0;
        
        ProcessUnrolled(MethodInt);
        
        return count;
    }

    private void MethodInt(ref Vector3 v, ref int i)
    {
        if (i >= Threshold) i = (int) Vector3.Dot(v, UniformConstantVector);
    }

    [Benchmark]
    public int Static()
    {
        var count = 0;
        
        Process(StaticInt);
        
        return count;
    }

    [Benchmark]
    public int StaticUnroll()
    {
        var count = 0;
        
        ProcessUnrolled(StaticInt);
        
        return count;
    }

    private static void StaticInt(ref Vector3 v, ref int i)
    {
        if (i >= Threshold) i = (int) Vector3.Dot(v, UniformConstantVector);
    }
}