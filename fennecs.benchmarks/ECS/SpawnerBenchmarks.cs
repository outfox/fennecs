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
[AnyCategoriesFilter("spawn")]
// ReSharper disable once IdentifierTypo
public class SpawnerBenchmarks
{
    private World _world = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000, 1_000_000)] public int entityCount { get; set; } = 100_000;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World(entityCount);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }

    [BenchmarkCategory("spawn")]
    [Benchmark(Description = "Spawn 1 by 1")]
    public int Spawn_1by1()
    {
        for (var i = 0; i < entityCount; i++)
        {
            var entity = _world.Spawn();
            entity.Add(1337);
            entity.Add("string string");
            entity.Add(MathF.E);
        }

        return _world.Count;
    }

    [BenchmarkCategory("spawn")]
    [Benchmark(Description = "Spawn via Spawner")]
    public int Spawn_Spawner()
    {
        _world.Entity()
            .Add(1337)
            .Add("string string")
            .Add(MathF.E)
            .Spawn(entityCount);

        return _world.Count;
    }
}