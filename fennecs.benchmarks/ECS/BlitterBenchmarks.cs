using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using fennecs;
using fennecs_Components;
using fennecs.pools;

namespace Benchmark.ECS;

[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[AnyCategoriesFilter("blit")]
// ReSharper disable once IdentifierTypo
public class BlitterBenchmarks
{
    private World _world = null!;
    private Query<int, string> _query = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000, 1_000_000, 10_000_000, 100_000_000)] public int entityCount { get; set; } = 1_000_000;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World(entityCount);
        _query = _world.Query<int, string>().Compile();

        _world.Entity()
            .Add(1337)
            .Add("string string")
            .Add(MathF.E)
            .Spawn(entityCount);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
        _query.Dispose();
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "non-blittable blit")]
    public int NonBlittable()
    {
        _query.Blit("not blittable");
        return _world.Count;
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "non-blittable job")]
    public int NonBlittableJob()
    {
        _query.Job((ref int _, ref string str, string uniform) => { str = uniform;}, "not blittable");
        return _world.Count;
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "blittable blit")]
    public int Blittable()
    {
        _query.Blit(123456);
        return _world.Count;
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "blittable job")]
    public int BlittableJob()
    {
        _query.Job((ref int val, ref string _, int uniform) => { val = uniform;}, 123456);
        return _world.Count;
    }
}