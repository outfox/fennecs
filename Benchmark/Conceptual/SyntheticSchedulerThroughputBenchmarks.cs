using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Benchmark;


[DryJob]
[ThreadingDiagnoser]
[MemoryDiagnoser]
public class SyntheticSchedulerThroughputBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(10_000, 1_000_000, 100_000_000)] 
    public int entityCount { get; set; }

    private static readonly Random random = new(1337);

    private Vector3[] _vectorsRaw = null!;

    [GlobalSetup]
    public void Setup()
    {
        _vectorsRaw = new Vector3[entityCount];

        for (var i = 0; i < entityCount; i++)
        {
            _vectorsRaw[i] = new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle());
        }
    }

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);
    private static readonly ParallelOptions options = new() {MaxDegreeOfParallelism = 12};

    [Benchmark]
    public void Single_Direct_Array()
    {
        for (var i = 0; i < entityCount; i++)
        {
            _vectorsRaw[i] = Vector3.Cross(_vectorsRaw[i], UniformConstantVector);
        }
    }

    [Benchmark(Baseline = true)]
    public void Single_Direct_Span()
    {
        foreach (ref var v in _vectorsRaw.AsSpan())
        {
            v = Vector3.Cross(v, UniformConstantVector);
        }
    }

    [Benchmark]
    public void Parallel_Direct_Array()
    {
        Parallel.For(0, _vectorsRaw.Length, options,
            i => { _vectorsRaw[i] = Vector3.Cross(_vectorsRaw[i], UniformConstantVector); });
    }

    [Benchmark]
    public void Parallel2_Direct_Array()
    {
        var opts = new ParallelOptions {MaxDegreeOfParallelism = 2};
        Parallel.For(0, _vectorsRaw.Length, opts,
            i => { _vectorsRaw[i] = Vector3.Cross(_vectorsRaw[i], UniformConstantVector); });
    }

    [Benchmark]
    public void Parallel2_Partitioned_Array()
    {
        var slices = 2;
        var completed = 0;

        for (var i = 0; i < slices; i++)
        {
            ThreadPool.QueueUserWorkItem(iteration =>
            {
                foreach (ref var v in _vectorsRaw.AsSpan(iteration * entityCount / slices, entityCount / slices))
                {
                    v = Vector3.Cross(v, UniformConstantVector);
                }
                Interlocked.Increment(ref completed);
            }, i, preferLocal: true);
        }

        while (completed < slices) Thread.Yield();
    }

    [Benchmark]
    public void Parallel8_Partitioned_Array()
    {
        var slices = 8;
        var completed = 0;

        for (var i = 0; i < slices; i++)
        {
            ThreadPool.QueueUserWorkItem(iteration =>
            {
                foreach (ref var v in _vectorsRaw.AsSpan(iteration * entityCount / slices, entityCount / slices))
                {
                    v = Vector3.Cross(v, UniformConstantVector);
                }

                Interlocked.Increment(ref completed);
            }, i, preferLocal: true);
        }

        while (completed < slices) Thread.Yield();
    }

    [Benchmark]
    public void Parallel2_Partition_Unrolled()
    {
        var slices = 2;
        var completed = 0;

        for (var i = 1; i < slices; i++)
        {
            ThreadPool.QueueUserWorkItem(iteration =>
            {
                foreach (ref var v in _vectorsRaw.AsSpan(iteration * entityCount / slices, entityCount / slices))
                {
                    v = Vector3.Cross(v, UniformConstantVector);
                }

                Interlocked.Increment(ref completed);
            }, i, preferLocal: true);
        }

        foreach (ref var v in _vectorsRaw.AsSpan(0, entityCount / slices))
        {
            v = Vector3.Cross(v, UniformConstantVector);
        }

        while (completed < slices - 1) Thread.Yield();
    }


    [Benchmark]
    public void Parallel4_Partition_Unrolled()
    {
        var slices = 4;
        var completed = 0;

        for (var i = 1; i < slices; i++)
        {
            ThreadPool.QueueUserWorkItem(iteration =>
            {
                foreach (ref var v in _vectorsRaw.AsSpan(iteration * entityCount / slices, entityCount / slices))
                {
                    v = Vector3.Cross(v, UniformConstantVector);
                }

                Interlocked.Increment(ref completed);
            }, i, preferLocal: true);
        }

        foreach (ref var v in _vectorsRaw.AsSpan(0, entityCount / slices))
        {
            v = Vector3.Cross(v, UniformConstantVector);
        }

        while (completed < slices - 1) Thread.Yield();
    }


    [Benchmark]
    public void Parallel8_Partition_Unrolled()
    {
        var slices = 8;
        var completed = 0;

        for (var i = 1; i < slices; i++)
        {
            ThreadPool.QueueUserWorkItem(iteration =>
            {
                foreach (ref var v in _vectorsRaw.AsSpan(iteration * entityCount / slices, entityCount / slices))
                {
                    v = Vector3.Cross(v, UniformConstantVector);
                }

                Interlocked.Increment(ref completed);
            }, i, preferLocal: true);
        }

        foreach (ref var v in _vectorsRaw.AsSpan(0, entityCount / slices))
        {
            v = Vector3.Cross(v, UniformConstantVector);
        }

        while (completed < slices - 1) Thread.Yield();
    }


    [Benchmark]
    public void Parallel16_Partition_Unrolled()
    {
        var slices = 16;
        var completed = 0;

        for (var i = 1; i < slices; i++)
        {
            ThreadPool.QueueUserWorkItem(iteration =>
            {
                foreach (ref var v in _vectorsRaw.AsSpan(iteration * entityCount / slices, entityCount / slices))
                {
                    v = Vector3.Cross(v, UniformConstantVector);
                }

                Interlocked.Increment(ref completed);
            }, i, preferLocal: true);
        }

        foreach (ref var v in _vectorsRaw.AsSpan(0, entityCount / slices))
        {
            v = Vector3.Cross(v, UniformConstantVector);
        }

        while (completed < slices - 1) Thread.Yield();
    }


    [Benchmark]
    public void Parallel4_Direct_Array()
    {
        var opts = new ParallelOptions {MaxDegreeOfParallelism = 4};
        Parallel.For(0, _vectorsRaw.Length, opts,
            i => { _vectorsRaw[i] = Vector3.Cross(_vectorsRaw[i], UniformConstantVector); });
    }

    [Benchmark]
    public void Parallel10_Direct_Array()
    {
        var opts = new ParallelOptions {MaxDegreeOfParallelism = 10};
        Parallel.For(0, _vectorsRaw.Length, opts,
            i => { _vectorsRaw[i] = Vector3.Cross(_vectorsRaw[i], UniformConstantVector); });
    }

    [Benchmark]
    public void Parallel20_Direct_Array()
    {
        var opts = new ParallelOptions {MaxDegreeOfParallelism = 20};
        Parallel.For(0, _vectorsRaw.Length, opts,
            i => { _vectorsRaw[i] = Vector3.Cross(_vectorsRaw[i], UniformConstantVector); });
    }

}