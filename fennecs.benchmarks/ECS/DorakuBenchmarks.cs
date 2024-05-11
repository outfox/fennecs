using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Order;
using fennecs;
using fennecs_Components;
using fennecs.pools;

namespace Benchmark.ECS;

[ShortRunJob]
//[TailCallDiagnoser]
[ThreadingDiagnoser]
[MemoryDiagnoser]
//[InliningDiagnoser(true, true)]
//[HardwareCounters(HardwareCounter.CacheMisses)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SystemWithThreeComponents
{
    private Query<Component1, Component2, Component3> _query = null!;
    private World _world = null!;

    [Params(100_000)] public int entityCount { get; set; } = 100_000;
    [Params(10)] public int entityPadding { get; set; } = 10;

    [GlobalSetup]
    public void Setup()
    {
        PooledList<UniformWork<Component1, Component2, Component3>>.Rent().Dispose();

        _world = new World();
        _query = _world.Query<Component1, Component2, Component3>().Build();
        for (var i = 0; i < entityCount; ++i)
        {
            for (var j = 0; j < entityPadding; ++j)
            {
                var padding = _world.Spawn();
                switch (j % 3)
                {
                    case 0:
                        padding.Add<Component1>();
                        break;
                    case 1:
                        padding.Add<Component2>();
                        break;
                    case 2:
                        padding.Add<Component3>();
                        break;
                }
            }

            _world.Spawn().Add<Component1>()
                .Add(new Component2 {Value = 1})
                .Add(new Component3 {Value = 1});
        }

        _query.Warmup();
    }
    
    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }

    /// <summary>
    /// This could be a static anonymous delegate, but this way, we don't need to repeat ourselves
    /// and reduce the risk of errors when refactoring or unit testing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void Workload(ref Component1 c1, ref Component2 c2, ref Component3 c3)
    {
        c1.Value += c2.Value + c3.Value;
    }

    //[BenchmarkCategory("fennecs2")]
    [Benchmark(Description = "fennecs (For)", Baseline = true)]
    public void fennecs_For()
    {
        _query.For(
            static delegate (ref Component1 c1, ref Component2 c2, ref Component3 c3)
            {
                c1.Value += c2.Value + c3.Value;
            });
    }


    [BenchmarkCategory("fennecs2")]
    [Benchmark(Description = "fennecs (For WL)")]
    public void fennecs_For_WL()
    {
        _query.For(Workload);
    }


    [BenchmarkCategory("fennecs2")]
    [Benchmark(Description = $"fennecs (Job)")]
    public void fennecs_Job()
    {
        _query.Job(static (ref Component1 c1, ref Component2 c2, ref Component3 c3) => { c1.Value += c2.Value + c3.Value; });
    }

    [BenchmarkCategory("fennecs3")]
    [Benchmark(Description = "fennecs (Raw AVX2)")]
    public void fennecs_Raw_AVX2()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using AVX2
        if (Avx2.IsSupported)
        {
            _query.Raw(Raw_Workload_AVX2);
        }
    }

    [BenchmarkCategory("fennecs3")]
    [Benchmark(Description = "fennecs (Raw SSE2)")]
    public void fennecs_Raw_SSE2()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using SSE2
        if (Sse2.IsSupported)
        {
            _query.Raw(Raw_Workload_SSE2);
        }
    }

    private static void Raw_Workload_AVX2(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
    {
        (int Item1, int Item2) range = (0, c1V.Length);

        using var mem1 = c1V.Pin();
        using var mem2 = c2V.Pin();
        using var mem3 = c3V.Pin();

        unsafe
        {
            var p1 = (int*) mem1.Pointer;
            var p2 = (int*) mem2.Pointer;
            var p3 = (int*) mem3.Pointer;

            var vectorSize = Vector256<int>.Count;
            var i = range.Item1;
            for (; i <= range.Item2 - vectorSize; i += vectorSize)
            {
                var v1 = Avx.LoadVector256(p1 + i);
                var v2 = Avx.LoadVector256(p2 + i);
                var v3 = Avx.LoadVector256(p3 + i);
                var sum = Avx2.Add(v1, Avx2.Add(v2, v3));

                Avx.Store(p1 + i, sum);
            }

            for (; i < range.Item2; i++) // remaining elements
            {
                p1[i] += p2[i] + p3[i];
            }
        }
    }

    private static void Raw_Workload_SSE2(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
    {
        (int Item1, int Item2) range = (0, c1V.Length);

        using var mem1 = c1V.Pin();
        using var mem2 = c2V.Pin();
        using var mem3 = c3V.Pin();

        unsafe
        {
            var p1 = (int*) mem1.Pointer;
            var p2 = (int*) mem2.Pointer;
            var p3 = (int*) mem3.Pointer;

            var vectorSize = Vector128<int>.Count;
            var i = range.Item1;
            for (; i <= range.Item2 - vectorSize; i += vectorSize)
            {
                var v1 = Sse2.LoadVector128(p1 + i);
                var v2 = Sse2.LoadVector128(p2 + i);
                var v3 = Sse2.LoadVector128(p3 + i);
                var sum = Sse2.Add(v1, Sse2.Add(v2, v3));

                Sse2.Store(p1 + i, sum);
            }

            for (; i < range.Item2; i++) // remaining elements
            {
                p1[i] += p2[i] + p3[i];
            }
        }
    }

}