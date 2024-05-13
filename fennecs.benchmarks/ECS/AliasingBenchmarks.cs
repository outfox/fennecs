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
//[AllCategoriesFilter("Blit")]
public class AliasingBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [Params(1_000_000)] public int entityCount { get; set; } = 100_000;

    private static readonly Random RandomV = new(1337);
    private static readonly Random RandomF = new(1337);

    private World _world = null!;

    private Query<Vector4> _queryV4 = null!;
    private Query<FoxVector4> _queryF4 = null!;
    private Query<FoxVector4Simd> _query128 = null!;

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
        
        _world = new(entityCount * 3);

        for (var i = 0; i < entityCount; i++)
        {
            _world.Spawn().Add<Vector4>(new(RandomV.NextSingle(), RandomV.NextSingle(), RandomV.NextSingle(), RandomV.NextSingle()));
            _world.Spawn().Add<FoxVector4>(new Vector4(RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle()));
            _world.Spawn().Add<FoxVector4Simd>(new Vector4(RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle()));
        }

        _queryV4 = _world.Query<Vector4>().Build();
        _queryV4.Warmup();

        _queryF4 = _world.Query<FoxVector4>().Build();
        _queryF4.Warmup();

        _query128 = _world.Query<FoxVector4Simd>().Build();
        _query128.Warmup();
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _queryV4 = null!;
        _world.Dispose();
        _world = null!;
    }

    private static readonly Vector4 UniformConstantVector = new(3, 4, 5, 6);


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
        _queryV4.Raw(static (v, uniform) =>
        {
            using var mem1 = v.Pin();
            unsafe
            {
                var p1 = (float*)mem1.Pointer;

                var uHalf = uniform.AsVector128();

                var u256 = Vector256.Create(uHalf, uHalf);

                var vectorSize = Vector256<float>.Count;

                var vectorEnd = v.Length / vectorSize * vectorSize;
                
                for (var i = 0; i < v.Length; i += vectorSize)
                {
                    var addr = p1 + i;
                    Avx.Store(addr, u256);
                }

                for (var i = vectorEnd; i < v.Length; i += 3) // remaining elements
                {
                    p1[i + 0] = uniform.X;
                    p1[i + 1] = uniform.Y;
                    p1[i + 2] = uniform.Z;
                    p1[i + 3] = uniform.W;
                }
            }
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit", "Write")]
    public void FoxVector4_Write_SIMD()
    {
        _queryF4.Raw(static (v, uniform) =>
        {
            using var mem1 = v.Pin();
            unsafe
            {
                var p1 = (float*)mem1.Pointer;
                
                var uHalf = uniform.Value.AsVector128();
                
                var u256 = Vector256.Create(uHalf, uHalf);

                var vectorSize = Vector256<float>.Count;

                var vectorEnd = v.Length / vectorSize * vectorSize;
                
                for (var i = 0; i < v.Length; i += vectorSize)
                {
                    var addr = p1 + i;
                    Avx.Store(addr, u256);
                }

                for (var i = vectorEnd; i < v.Length; i += 3) // remaining elements
                {
                    p1[i + 0] = uniform.Value.X;
                    p1[i + 1] = uniform.Value.Y;
                    p1[i + 2] = uniform.Value.Z;
                    p1[i + 3] = uniform.Value.W;
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
