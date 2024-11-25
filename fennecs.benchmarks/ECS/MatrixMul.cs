using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using fennecs;
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
public class MatrixMul
{
    private Stream<Matrix4x4> _query = null!;
    private World _world = null!;
    
    private Matrix4x4 _transform = Matrix4x4.Identity;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000)] public int EntityCount { get; set; } = 0;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World();
        _query = _world.Query<Matrix4x4>().Stream();

        var rnd = new Random(1337);
        _transform = Matrix4x4.CreateBillboard(new Vector3(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle()), new Vector3(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle()), Vector3.UnitY, Vector3.UnitZ);
        
        for (var i = 0; i < EntityCount; ++i)
        {
            var matrix = new Matrix4x4
            {
                M11 = rnd.NextSingle(),
                M12 = rnd.NextSingle(),
                M13 = rnd.NextSingle(),
                M14 = rnd.NextSingle(),
                M21 = rnd.NextSingle(),
                M22 = rnd.NextSingle(),
                M23 = rnd.NextSingle(),
                M24 = rnd.NextSingle(),
                M31 = rnd.NextSingle(),
                M32 = rnd.NextSingle(),
                M33 = rnd.NextSingle(),
                M34 = rnd.NextSingle(),
                M41 = rnd.NextSingle(),
                M42 = rnd.NextSingle(),
                M43 = rnd.NextSingle(),
                M44 = rnd.NextSingle()
            };
            _world.Spawn().Add(matrix);
        }

        fennecs_Raw();
        _query.Job(_transform, Workload);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (For)", Baseline = true)]
    public void fennecs_For()
    {
        _query.For(_transform, Workload);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Job)")]
    public void fennecs_Job()
    {
        _query.Job(_transform, Workload);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Raw)")]
    public void fennecs_Raw()
    {
        _query.Raw(_transform, Workload_Raw);
    }
    
    private static void Workload(Matrix4x4 transform, RW<Matrix4x4> self)
    {
        self.write *= transform;
    }

    private static void Workload_Raw(Matrix4x4 transform, Span<Matrix4x4> self)
    {
        for (var i = 0; i < self.Length; i++) self[i] *= transform;
    }
}