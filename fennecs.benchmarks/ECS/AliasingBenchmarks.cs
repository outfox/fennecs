using System.Numerics;
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
    [Params(1_000_000)] public int entityCount { get; set; } = 1_000_000;

    private static readonly Random RandomV = new(1337);
    private static readonly Random RandomF = new(1337);

    private World _world = null!;

    private Query<Vector3> _queryV3 = null!;
    private Query<FoxVector> _queryF3 = null!;

    private struct FoxVector(Vector3 v) : Fox<Vector3>
    {
        public Vector3 Value { get; set; } = v;

        public static implicit operator FoxVector(Vector3 v) => new(v);

        public static implicit operator Vector3(FoxVector v) => v.Value;
    }

    [GlobalSetup]
    public void Setup()
    {
        _world = new(entityCount * 3);

        for (var i = 0; i < entityCount; i++)
        {
            _world.Spawn().Add<Vector3>(new(RandomV.NextSingle(), RandomV.NextSingle(), RandomV.NextSingle()));
            _world.Spawn().Add<FoxVector>(new Vector3(RandomF.NextSingle(), RandomF.NextSingle(), RandomF.NextSingle()));
        }

        _queryV3 = _world.Query<Vector3>().Build();
        _queryV3.Warmup();

        _queryF3 = _world.Query<FoxVector>().Build();
        _queryF3.Warmup();
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _queryV3 = null!;
        _world.Dispose();
        _world = null!;
    }

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);


    #region Write (Store) Vector

    [Benchmark]
    public void Vector3_Write()
    {
        _queryV3.For(static (ref Vector3 v, Vector3 uniform) =>
        {
            v = uniform;
        }, UniformConstantVector);
    }
    
    [Benchmark]
    public void Vector3_Write_Job()
    {
        _queryV3.Job(static (ref Vector3 v, Vector3 uniform) =>
        {
            v = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Write_Explicit_Ref()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v.Value = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Write_Explicit_New()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v = new(uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Write_Implicit_New()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v = uniform;
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Write_Explicit_New_Job()
    {
        _queryF3.Job(static (ref FoxVector v, Vector3 uniform) =>
        {
            v = new(uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit")]
    public void Vector3_Write_Blit()
    {
        _queryV3.Blit(UniformConstantVector);
    }

    [Benchmark]
    [BenchmarkCategory("Blit")]
    public void FoxVector3_Write_Blit()
    {
        _queryF3.Blit(UniformConstantVector);
    }
    #endregion


    #region Calculate (CrossProduct) Vector with Uniform

    [Benchmark]
    public void Vector3_Cross()
    {
        _queryV3.For(static (ref Vector3 v, Vector3 uniform) =>
        {
            v = Vector3.Cross(v, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Cross_Explicit_New()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v = new(Vector3.Cross(v.Value, uniform));
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Cross_Explicit_Ref()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v.Value = Vector3.Cross(v.Value, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Cross_Implicit_Ref()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v.Value = Vector3.Cross(v.Value, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void FoxVector3_Cross_Implicit_New()
    {
        _queryF3.For(static (ref FoxVector v, Vector3 uniform) =>
        {
            v = Vector3.Cross(v, uniform);
        }, UniformConstantVector);
    }

    [Benchmark]
    public void Vector3_Cross_Job()
    {
        _queryV3.Job(static (ref Vector3 v, Vector3 uniform) =>
        {
            v = Vector3.Cross(v, uniform);
        }, UniformConstantVector);
    }


    [Benchmark]
    public void FoxVector3_Cross_Explicit_New_Job()
    {
        _queryF3.Job(static (ref FoxVector v, Vector3 uniform) =>
        {
            v = new(Vector3.Cross(v.Value, uniform));
        }, UniformConstantVector);
    }

    #endregion
}
