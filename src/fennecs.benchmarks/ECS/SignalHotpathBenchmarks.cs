using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// Splits the zero-subscriber paths into their parts (spawn, despawn, batch) at several sizes,
/// to tell fixed overhead from per-Entity overhead. Uses only API present on <c>main</c> too.
/// </summary>
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 20, invocationCount: 1)]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared)]
[HideColumns("Job", "Error", "RatioSD")]
public class SignalHotpathBenchmarks
{
    private record struct Position(float X, float Y);
    private record struct Tracked;
    private record struct Wave;

    [Params(1_000, 100_000)]
    public int EntityCount { get; set; }

    private World _world = null!;
    private Query _needPosition = null!;
    private Query _havePosition = null!;


    [GlobalSetup]
    public void Setup()
    {
        _world = new World(EntityCount * 2);
        for (var i = 0; i < EntityCount; i++) _world.Spawn().Add<Tracked>();

        _needPosition = _world.Query<Tracked>().Not<Position>().Compile();
        _havePosition = _world.Query<Tracked>().Has<Position>().Compile();
    }


    [GlobalCleanup]
    public void Cleanup() => _world.Dispose();


    [IterationSetup(Target = nameof(Batch_Add))]
    public void MakeNeedPosition() => _havePosition.Batch().Remove<Position>().Submit();

    [Benchmark]
    public void Batch_Add() => _needPosition.Batch().Add(new Position(1, 2)).Submit();


    [IterationSetup(Target = nameof(Batch_Remove))]
    public void MakeHavePosition() => _needPosition.Batch().Add(new Position(1, 2)).Submit();

    [Benchmark]
    public void Batch_Remove() => _havePosition.Batch().Remove<Position>().Submit();


    [IterationSetup(Target = nameof(Spawn))]
    public void NoWave() => _world.DespawnAllWith<Wave>();

    [Benchmark]
    public void Spawn() => _world.Template().Add(new Position(1, 2)).Add<Wave>().Spawn(EntityCount).Dispose();


    [IterationSetup(Target = nameof(Despawn))]
    public void MakeWave() => _world.Template().Add(new Position(1, 2)).Add<Wave>().Spawn(EntityCount).Dispose();

    [Benchmark]
    public void Despawn() => _world.DespawnAllWith<Wave>();
}
