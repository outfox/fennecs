using System.Diagnostics;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using fennecs;
using fennecs_Components;
using fennecs.storage;

namespace Benchmark.ECS;

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
    private Stream<Component1, Component2, Component3> _query = null!;
    private World _world = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000)] public int EntityCount { get; set; } = 0;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World();
        _query = _world.Query<Component1, Component2, Component3>().Stream();
        
        for (var i = 0; i < EntityCount; ++i)
        {
            _world.Spawn().Add<Component1>()
                .Add(new Component2 {Value = 1})
                .Add(new Component3 {Value = 1});
        }

        fennecs_Raw();
        fennecs_Raw_Tensor_Generic();
        _query.Job(Workload);
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
    private static void Workload(RW<Component1> c1, R<Component2> c2, R<Component3> c3)
    {
        c1.write.Value = c1.write.Value + c2.read.Value + c3.read.Value;
    }

    [BenchmarkCategory("fennecs")]
    //[Benchmark(Description = "fennecs (For SD)")]
    public void fennecs_ForSD()
    {
        _query.For(static delegate(RW<Component1> c1, R<Component2> c2, R<Component3> c3)
        {
            c1.write.Value += c2.read.Value + c3.read.Value;
        });
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (For)", Baseline = true)]
    public void fennecs_For()
    {
        _query.For(static (c1, c2, c3) => { c1.write.Value += c2.write.Value + c3.write.Value; });
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
        _query.Job(static delegate(RW<Component1> c1, R<Component2> c2, R<Component3> c3)
        {
            c1.write.Value = c1.read.Value + c2.read.Value + c3.read.Value;
        });
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Raw)")]
    public void fennecs_Raw()
    {
        _query.Raw(Raw_Workload_Unoptimized);
    }

    [BenchmarkCategory("fennecs", nameof(Avx2))]
    [Benchmark(Description = "fennecs (Mem AVX2)")]
    public void fennecs_Mem_AVX2()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using AVX2
        _query.Mem(Mem_Workload_AVX2);
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


    [BenchmarkCategory("fennecs", "Tensor")]
    [Benchmark(Description = "fennecs (Raw Tensor)")]
    public void fennecs_Raw_Tensor()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using AVX2
        _query.Raw(Raw_Workload_Tensor);
    }

    

    [BenchmarkCategory("fennecs", "Tensor")]
    [Benchmark(Description = "fennecs (Raw Tensor Unsafe)")]
    public void fennecs_Raw_Tensor_Unsafe()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using AVX2
        _query.Raw(Raw_Workload_Tensor_Unsafe);
    }

    [BenchmarkCategory("fennecs", "Tensor")]
    [Benchmark(Description = "fennecs (Raw Tensor Generic)")]
    public void fennecs_Raw_Tensor_Generic()
    {
        // fennecs guarantees contiguous memory access in the form of Query<>.Raw(MemoryAction<>)
        // Raw runners are intended to process data or transfer it via the fastest available means,
        // Example use cases:
        //  - transfer buffers to/from GPUs or Game Engines
        //  - Disk, Database, or Network I/O
        //  - SIMD calculations
        //  - snapshotting / copying / rollback / compression / decompression / diffing / permutation

        // As example / reference & benchmark, we vectorized our calculation here using AVX2
        _query.Raw(Raw_Workload_Tensor_Generic<int, Component1, Component2, Component3>);
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

    private static void Raw_Workload_Unoptimized(Span<Component1> c1S, ReadOnlySpan<Component2> c2S,
        ReadOnlySpan<Component3> c3S)
    {
        for (var i = 0; i < c1S.Length; i++)
        {
            c1S[i].Value = c1S[i].Value + c2S[i].Value + c3S[i].Value;
        }
    }


    private static void Raw_Workload_AVX2(Span<Component1> c1V, ReadOnlySpan<Component2> c2V, ReadOnlySpan<Component3> c3V)
    {
        var count = c1V.Length;

        unsafe
        {
            fixed (Component1* c1 = c1V)
            fixed (Component2* c2 = c2V)
            fixed (Component3* c3 = c3V)
            {
                var p1 = (int*) c1;
                var p2 = (int*) c2;
                var p3 = (int*) c3;

                var vectorSize = Vector256<int>.Count;
                var vectorEnd = count - count % vectorSize;
                for (var i = 0; i < vectorEnd; i += vectorSize)
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
    }


    private static void Mem_Workload_AVX2(MemoryRW<Component1> c1V, MemoryR<Component2> c2V, MemoryR<Component3> c3V)
    {
        var count = c1V.Length;

        var c1 = c1V.Memory.Pin();
        var c2 = c2V.ReadOnlyMemory.Pin();
        var c3 = c3V.ReadOnlyMemory.Pin();

        unsafe
        {
            var p1 = (int*) c1.Pointer;
            var p2 = (int*) c2.Pointer;
            var p3 = (int*) c3.Pointer;

            var vectorSize = Vector256<int>.Count;
            var vectorEnd = count - count % vectorSize;
            for (var i = 0; i < vectorEnd; i += vectorSize)
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

    private static void Raw_Workload_Tensor<T1, T2, T3>(Span<T1> c1V, ReadOnlySpan<T2> c2V, ReadOnlySpan<T3> c3V)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        var c1I = MemoryMarshal.Cast<T1, int>(c1V);
        var c2I = MemoryMarshal.Cast<T2, int>(c2V);
        var c3I = MemoryMarshal.Cast<T3, int>(c3V);

        TensorPrimitives.Add(c2I, c1I, c1I);
        TensorPrimitives.Add(c3I, c1I, c1I);
    }

    private static void Raw_Workload_Tensor_Unsafe<T1, T2, T3>(Span<T1> c1V, ReadOnlySpan<T2> c2V, ReadOnlySpan<T3> c3V)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        unsafe
        {
            fixed (T1* c1 = c1V)
            fixed (T2* c2 = c2V)
            fixed (T3* c3 = c3V)
            {
                var c1I = new Span<int>(c1, c1V.Length * sizeof(T1) / sizeof(int));
                var c2I = new ReadOnlySpan<int>(c2, c2V.Length * sizeof(T2) / sizeof(int));
                var c3I = new ReadOnlySpan<int>(c3, c3V.Length * sizeof(T3) / sizeof(int));
                
                TensorPrimitives.Add(c2I, c1I, c1I);
                TensorPrimitives.Add(c3I, c1I, c1I);
            }
        }

    }

    private static void Raw_Workload_Tensor_Generic<PRIM, T1, T2, T3>(Span<T1> c1V, ReadOnlySpan<T2> c2V, ReadOnlySpan<T3> c3V)
        where T1 : unmanaged, Fox<PRIM>
        where T2 : unmanaged, Fox<PRIM>
        where T3 : unmanaged, Fox<PRIM>
        where PRIM : unmanaged, IAdditionOperators<PRIM,PRIM,PRIM>, IAdditiveIdentity<PRIM,PRIM>
    {
        Debug.Assert(c1V.Length == c2V.Length, "c1V and c2V must have the same length");
        Debug.Assert(c1V.Length == c3V.Length, "c1V and c3V must have the same length");
        
        unsafe
        {
            Debug.Assert(sizeof(T1) == sizeof(PRIM), "T1 is not memory size congruent with PRIM");
            Debug.Assert(sizeof(T2) == sizeof(PRIM), "T2 is not memory size congruent with PRIM");
            Debug.Assert(sizeof(T3) == sizeof(PRIM), "T3 is not memory size congruent with PRIM");
            
            fixed (T1* c1 = c1V)
            fixed (T2* c2 = c2V)
            fixed (T3* c3 = c3V)
            {
                var c1I = new Span<PRIM>(c1, c1V.Length * sizeof(T1) / sizeof(PRIM));
                var c2I = new ReadOnlySpan<PRIM>(c2, c2V.Length * sizeof(T2) / sizeof(PRIM));
                var c3I = new ReadOnlySpan<PRIM>(c3, c3V.Length * sizeof(T3) / sizeof(PRIM));
                
                TensorPrimitives.Add(c2I, c1I, c1I);
                TensorPrimitives.Add(c3I, c1I, c1I);
            }
        }
    }


    private static void Raw_Workload_SSE2(Span<Component1> c1V, ReadOnlySpan<Component2> c2V, ReadOnlySpan<Component3> c3V)
    {
        (int Item1, int Item2) range = (0, c1V.Length);

        unsafe
        {
            fixed (Component1* c1 = c1V)
            fixed (Component2* c2 = c2V)
            fixed (Component3* c3 = c3V)
            {
                var p1 = (int*) c1;
                var p2 = (int*) c2;
                var p3 = (int*) c3;

                var vectorSize = Vector128<int>.Count;
                var i = range.Item1;
                var vectorEnd = range.Item2 - vectorSize;
                for (; i < vectorEnd; i += vectorSize)
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
    }

    private static void Raw_Workload_AdvSIMD(Span<Component1> c1V, ReadOnlySpan<Component2> c2V, ReadOnlySpan<Component3> c3V)
    {
        (int Item1, int Item2) range = (0, c1V.Length);

        unsafe
        {
            fixed (Component1* c1 = c1V)
            fixed (Component2* c2 = c2V)
            fixed (Component3* c3 = c3V)
            {
                var p1 = (int*) c1;
                var p2 = (int*) c2;
                var p3 = (int*) c3;

                var vectorSize = Vector128<int>.Count;
                var i = range.Item1;
                var vectorEnd = range.Item2 - vectorSize;
                for (; i < vectorEnd; i += vectorSize)
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
}