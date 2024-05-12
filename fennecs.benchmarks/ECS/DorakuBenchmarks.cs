using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using fennecs;
using fennecs_Components;
using fennecs.pools;

namespace Benchmark.ECS;

[SimpleJob]
//[TailCallDiagnoser]
[ThreadingDiagnoser]
[MemoryDiagnoser]
//[InliningDiagnoser(true, true)]
//[HardwareCounters(HardwareCounter.CacheMisses)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[AnyCategoriesFilter("fennecs", "fennecs2")]
// ReSharper disable once IdentifierTypo
public class DorakuBenchmarks
{
    private Query<Component1, Component2, Component3> _query = null!;
    private World _world = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000)] public int entityCount { get; set; } = 100_000;

    // ReSharper disable once MemberCanBePrivate.Global
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
    private static void Workload(ref Component1 c1, ref Component2 c2, ref Component3 c3)
    {
        c1.Value = c1.Value + c2.Value + c3.Value;
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (For)", Baseline = true)]
    public void fennecs_For()
    {
        _query.For(static delegate(ref Component1 c1, ref Component2 c2, ref Component3 c3) { c1.Value = c1.Value + c2.Value + c3.Value; });
    }


    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (For WL)")]
    public void fennecs_For_WL()
    {
        _query.For(Workload);
    }


    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = $"fennecs (Job)")]
    public void fennecs_Job()
    {
        _query.Job(static delegate (ref Component1 c1, ref Component2 c2, ref Component3 c3) { c1.Value = c1.Value + c2.Value + c3.Value; });
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Raw)")]
    public void fennecs_Raw()
    {
        _query.Raw(Raw_Workload_Unoptimized);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Raw U4)")]
    public void fennecs_Raw_Unroll4()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we calculate using an unrolled loop
        _query.Raw(Raw_Workload_Unroll4);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Raw U8)")]
    public void fennecs_Raw_Unroll8()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we calculate using an unrolled loop
        _query.Raw(Raw_Workload_Unroll8);
    }

    [BenchmarkCategory("fennecs", nameof(Avx2))]
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
        _query.Raw(Raw_Workload_AVX2);
    }

    [BenchmarkCategory("fennecs", nameof(Sse2))]
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
        _query.Raw(Raw_Workload_SSE2);
    }

    [BenchmarkCategory("fennecs", nameof(AdvSimd))]
    [Benchmark(Description = "fennecs (Raw AdvSIMD)")]
    public void fennecs_Raw_AdvSIMD()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using Arm AdvSIMD
        _query.Raw(Raw_Workload_AdvSIMD);
    }

    private static void Raw_Workload_Unoptimized(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
    {
        var c1S = c1V.Span;
        var c2S = c2V.Span;
        var c3S = c3V.Span;

        for (var i = 0; i < c1S.Length; i++)
        {
            c1S[i].Value = c1S[i].Value + c2S[i].Value + c3S[i].Value;
        }
    }

    private static void Raw_Workload_Unroll4(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
    {
        var c1 = c1V.Span;
        var c2 = c2V.Span;
        var c3 = c3V.Span;

        var i = 0;
        for (; i < c1.Length; i += 4)
        {
            var j = i + 1;
            var k = j + 1;
            var l = k + 1;
            c1[i].Value = c1[i].Value + c2[i].Value + c3[i].Value;
            c1[j].Value = c1[j].Value + c2[j].Value + c3[j].Value;
            c1[k].Value = c1[k].Value + c2[k].Value + c3[k].Value;
            c1[l].Value = c1[l].Value + c2[l].Value + c3[l].Value;
        }

        for (; i < c1.Length; i += 1)
        {
            c1[i].Value = c1[i].Value + c2[i].Value + c3[i].Value;
        }
    }

    private static void Raw_Workload_Unroll8(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
    {
        var c1 = c1V.Span;
        var c2 = c2V.Span;
        var c3 = c3V.Span;

        var i = 0;
        for (; i < c1.Length; i += 8)
        {
            var j = i + 1;
            var k = i + 2;
            var l = i + 3;
            var m = i + 4;
            var n = i + 5;
            var o = i + 6;
            var p = i + 7;

            c1[i].Value = c1[i].Value + c2[i].Value + c3[i].Value;
            c1[j].Value = c1[j].Value + c2[j].Value + c3[j].Value;
            c1[k].Value = c1[k].Value + c2[k].Value + c3[k].Value;
            c1[l].Value = c1[l].Value + c2[l].Value + c3[l].Value;
            c1[m].Value = c1[m].Value + c2[m].Value + c3[m].Value;
            c1[n].Value = c1[n].Value + c2[n].Value + c3[n].Value;
            c1[o].Value = c1[o].Value + c2[o].Value + c3[o].Value;
            c1[p].Value = c1[p].Value + c2[p].Value + c3[p].Value;
        }

        for (; i < c1.Length; i += 1)
        {
            c1[i].Value = c1[i].Value + c2[i].Value + c3[i].Value;
        }
    }

    private static void Raw_Workload_AVX2(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
    {
        var count = c1V.Length;

        using var mem1 = c1V.Pin();
        using var mem2 = c2V.Pin();
        using var mem3 = c3V.Pin();

        unsafe
        {
            var p1 = (int*) mem1.Pointer;
            var p2 = (int*) mem2.Pointer;
            var p3 = (int*) mem3.Pointer;

            var vectorSize = Vector256<int>.Count;
            var vectorEnd = count - count % vectorSize;
            for (var i = 0; i <= vectorEnd; i += vectorSize)
            {
                var v1 = Avx.LoadVector256(p1 + i);
                var v2 = Avx.LoadVector256(p2 + i);
                var v3 = Avx.LoadVector256(p3 + i);
                var sum = Avx2.Add(v1, Avx2.Add(v2, v3));

                Avx.Store(p1 + i, sum);
            }

            for (var i = vectorEnd; i < count; i++) // remaining elements
            {
                p1[i] = p1[i] + p2[i] + p3[i];
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
            var vectorEnd = range.Item2 - vectorSize;
            for (; i <= vectorEnd; i += vectorSize)
            {
                var v1 = Sse2.LoadVector128(p1 + i);
                var v2 = Sse2.LoadVector128(p2 + i);
                var v3 = Sse2.LoadVector128(p3 + i);
                var sum = Sse2.Add(v1, Sse2.Add(v2, v3));

                Sse2.Store(p1 + i, sum);
            }

            for (; i < range.Item2; i++) // remaining elements
            {
                p1[i] = p1[i] + p2[i] + p3[i];
            }
        }
    }

    private static void Raw_Workload_AdvSIMD(Memory<Component1> c1V, Memory<Component2> c2V, Memory<Component3> c3V)
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
            var vectorEnd = range.Item2 - vectorSize;
            for (; i <= vectorEnd; i += vectorSize)
            {
                var v1 = AdvSimd.LoadVector128(p1 + i);
                var v2 = AdvSimd.LoadVector128(p2 + i);
                var v3 = AdvSimd.LoadVector128(p3 + i);
                var sum = AdvSimd.Add(v1, AdvSimd.Add(v2, v3));

                AdvSimd.Store(p1 + i, sum);
            }

            for (; i < range.Item2; i++) // remaining elements
            {
                p1[i] = p1[i] + p2[i] + p3[i];
            }
        }
    }
}