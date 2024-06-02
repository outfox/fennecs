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
    private Stream<int, string> _stream = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000, 1_000_000, 10_000_000, 100_000_000)]
    public int entityCount { get; set; } = 1_000_000;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World(entityCount);
        _stream = _world.Query<int, string>().Stream();

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
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "non-blittable blit")]
    public int NonBlittable()
    {
        _stream.Blit("not blittable");
        return _world.Count;
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "non-blittable job")]
    public int NonBlittableJob()
    {
        _stream.Job("not blittable",
            (string uniform, ref int _, ref string str) => { str = uniform; });
        return _world.Count;
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "blittable blit")]
    public int Blittable()
    {
        _stream.Blit(123456);
        return _world.Count;
    }

    [BenchmarkCategory("blit")]
    [Benchmark(Description = "blittable job")]
    public int BlittableJob()
    {
        _stream.Job(
            uniform: 123456,
            action: (int uniform, ref int val, ref string _) =>
            {
                val = uniform;
            }
        );
        return _world.Count;
    }
}
