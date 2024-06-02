using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark.ECS;

[ShortRunJob]
[ThreadingDiagnoser]
[MemoryDiagnoser]
//[HardwareCounters(HardwareCounter.CacheMisses)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[AllCategoriesFilter("Blit")]
[StructLayout(LayoutKind.Auto, Pack = 32)]
public class AliasingBenchmarks
{
    private Vector4[] _testArray = null!;
    
    private static readonly Vector4 UniformConstantVector = new(3, 4, 5, 6);


    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [Params(1_000_000)] public int entityCount { get; set; } = 400_000;

    private static readonly Random RandomV = new(1337);
    private static readonly Random RandomF = new(1337);

    private World _world = null!;

    private Stream<Vector4> _queryV4 = null!;
    private Stream<FoxVector4> _queryF4 = null!;
    private Stream<FoxVector4Simd> _query128 = null!;

    private struct FoxVector4(Vector4 v) : Fox<Vector4>
    {
        public Vector4 Value { get; set; } = v;

        public static implicit operator FoxVector4(Vector4 v) => new(v);

        public static implicit operator Vector4(FoxVector4 v) => v.Value;
    }

    private struct FoxVector3(Vector3 v) : Fox<Vector3>
    {
        public Vector3 Value { get; set; } = v;

        public static implicit operator FoxVector3(Vector3 v) => new(v);

        public static implicit operator Vector3(FoxVector3 v) => v.Value;
    }

    
    [StructLayout(LayoutKind.Explicit)]
    public struct FoxVector4Simd(Vector4 v) : Fox<Vector4>, Fox128<Vector4>
    {

        [field: FieldOffset(0)]
        Vector4 Fox<Vector4>.Value { get; set; } = v;

        [field: FieldOffset(0)]
        Vector128<Vector4> Fox<Vector128<Vector4>>.Value { get; set; }

        public static implicit operator FoxVector4Simd(Vector4 v) => new(v);
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct FoxVector3Simd(Vector3 v) : Fox<Vector3>, Fox128<Vector3>
    {

        [field: FieldOffset(0)]
        Vector3 Fox<Vector3>.Value { get; set; } = v;

        [field: FieldOffset(0)]
        Vector128<Vector3> Fox<Vector128<Vector3>>.Value { get; set; }

        public static implicit operator FoxVector3Simd(Vector3 v) => new(v);
    }


    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine($"F4 {Unsafe.SizeOf<FoxVector4>()}, F3 {Unsafe.SizeOf<FoxVector3>()}, S4 {Unsafe.SizeOf<FoxVector4Simd>()}, S3 {Unsafe.SizeOf<FoxVector3Simd>()}, V4 {Unsafe.SizeOf<Vector4>()}, V3 {Unsafe.SizeOf<Vector3>()}");

        if (RuntimeHelpers.IsReferenceOrContainsReferences<Vector4>()) Console.WriteLine("Vector4 is reference");
        if (!Vector.IsHardwareAccelerated) Console.WriteLine("Vectorization is not supported");
        if (Unsafe.SizeOf<Vector4>() > Vector<byte>.Count) Console.WriteLine("FoxVector4Simd is too large for SIMD");
        if (!BitOperations.IsPow2(Unsafe.SizeOf<Vector4>())) Console.WriteLine("Vector4 is not power of 2");
        Console.WriteLine("...");
        
        _world = new(entityCount * 3);

        _testArray = new Vector4[entityCount];

        for (var i = 0; i < entityCount; i++)
        {
            _world.Spawn().Add(new Vector4(RandomV.NextSingle(), RandomV.NextSingle(), RandomV.NextSingle(), RandomV.NextSingle()));
            _world.Spawn().Add<FoxVector4>(new Vector4(RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle()));
            _world.Spawn().Add<FoxVector4Simd>(new Vector4(RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle()));
        }

        _queryV4 = _world.Query<Vector4>().Stream();
        _queryV4.Query.Warmup();

        _queryF4 = _world.Query<FoxVector4>().Stream();
        _queryF4.Query.Warmup();

        _query128 = _world.Query<FoxVector4Simd>().Stream();
        _query128.Query.Warmup();
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _queryV4 = null!;
        _world.Dispose();
        _world = null!;
    }
    
    #region Write (Store) Vector

    [Benchmark]
    [BenchmarkCategory("Write")]
    public void Vector4_Write()
    {
        _queryV4.For(static (ref Vector4 v, Vector4 uniform) =>
        {
            v = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Write")]
    public void Vector4_Write_Job()
    {
        _queryV4.Job(static (ref Vector4 v, Vector4 uniform) =>
        {
            v = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Write")]
    public void FoxVector4_Write_Explicit_Ref()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v.Value = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Write")]
    public void FoxVector4_Write_Explicit_New()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v = new(uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Write")]
    public void FoxVector4_Write_Implicit_New()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Write")]
    public void FoxVector4_Write_Explicit_New_Job()
    {
        _queryF4.Job(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v = new(uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void Vector4_Write_Blit()
    {
        _queryV4.Blit(UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void FoxVector4_Write_Blit()
    {
        _queryF4.Blit(UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void Generic_Span_Fill_Control()
    {
        var span = _testArray.AsSpan();
        span.Fill(UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void Vector4_Write_Raw()
    {
        _queryV4.Raw(static (v, uniform) =>
        {
            v.Span.Fill(uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void FoxVector4_Write_Raw()
    {
        _queryF4.Raw(static (v, uniform) =>
        {
            v.Span.Fill(uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void Vector4_Write_SIMD()
    {
        _queryV4.Raw(static (mem, uniform) =>
        {
            using var handle = mem.Pin();
            unsafe
            {
                var length = mem.Length * sizeof(Vector4) / sizeof(float);
            
                var p1 = (float*)handle.Pointer;

                var uHalf = uniform.AsVector128();
                var u256 = Vector256.Create(uHalf, uHalf);

                var vectorSize = Vector256<float>.Count;
                var vectorEnd = length / vectorSize * vectorSize;

                for (var i = 0; i < length; i += vectorSize)
                {
                    var addr = p1 + i;
                    Avx.Store(addr, u256);
                }

                for (var i = vectorEnd; i < length; i++) // remaining elements
                {
                    p1[i] = uniform[i % 4];
                }
            }
        }, UniformConstantVector);
    }

    
    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void Vector4_Write_SIMD128()
    {
        _queryV4.Raw(static (mem, uniform) =>
        {
            using var handle = mem.Pin();
            unsafe
            {
                var length = mem.Length * sizeof(Vector4) / sizeof(float);
            
                var p1 = (float*)handle.Pointer;

                var u128 = uniform.AsVector128();

                var vectorSize = Vector128<float>.Count;
                var vectorEnd = length / vectorSize * vectorSize;

                for (var i = 0; i < length; i += vectorSize)
                {
                    var addr = p1 + i;
                    Sse.Store(addr, u128);
                }

                for (var i = vectorEnd; i < length; i++) // remaining elements
                {
                    p1[i] = uniform[i % 4];
                }
            }
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void FoxVector4_Write_SIMD()
    {
        _queryF4.Raw(static (mem, uniform) =>
        {
            using var handle = mem.Pin();
            unsafe
            {
                var length = mem.Length * sizeof(FoxVector4) / sizeof(float);
            
                var p1 = (float*)handle.Pointer;

                var uHalf = uniform.Value.AsVector128();
                var u256 = Vector256.Create(uHalf, uHalf);

                var vectorSize = Vector256<float>.Count;
                var vectorEnd = length / vectorSize * vectorSize;

                for (var i = 0; i < length; i += vectorSize)
                {
                    var addr = p1 + i;
                    Avx.Store(addr, u256);
                }

                for (var i = vectorEnd; i < length; i++) // remaining elements
                {
                    p1[i] = uniform.Value[i % 4];
                }
            }
        }, new FoxVector4(UniformConstantVector));
    }

    #endregion


    #region Calculate (SubtractProduct) Vector with Uniform

    [Benchmark]
    public void Vector4_Subtract()
    {
        _queryV4.For(static (ref Vector4 v, Vector4 uniform) =>
        {
            v = Vector4.Subtract(v, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector4_Subtract_Explicit_New()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v = new(Vector4.Subtract(v.Value, uniform));
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector4_Subtract_Explicit_Ref()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v.Value = Vector4.Subtract(v.Value, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector4_Subtract_Implicit_Ref()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v.Value = Vector4.Subtract(v.Value, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector4_Subtract_Implicit_New()
    {
        _queryF4.For(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v = Vector4.Subtract(v, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void Vector4_Subtract_Job()
    {
        _queryV4.Job(static (ref Vector4 v, Vector4 uniform) =>
        {
            v = Vector4.Subtract(v, uniform);
        }, UniformConstantVector);
    }


    [Benchmark]
    public void FoxVector4_Subtract_Explicit_New_Job()
    {
        _queryF4.Job(static (ref FoxVector4 v, Vector4 uniform) =>
        {
            v = new(Vector4.Subtract(v.Value, uniform));
        }, UniformConstantVector);
    }

    #endregion
}
