using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// The structural change paths with <b>no Signal in sight</b>, written against API that exists
/// both on <c>main</c> and on the Signals branch — so the very same file measures what merely
/// <i>having</i> the feature costs the people who never use it.
/// </summary>
/// <remarks>
/// Run it on both trees and compare the means; the Micro group splits the remaining per-event
/// costs (World lock, component lookup) out of the Signal dispatch.
/// </remarks>
[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared)]
[HideColumns("Job", "Error", "RatioSD")]
public class SignalOverheadBenchmarks
{
    private record struct Position(float X, float Y);
    private record struct Tracked;
    private record struct Wave;

    [Params(10_000)]
    public int EntityCount { get; set; }

    private World _world = null!;
    private Entity[] _entities = null!;
    private Query _needPosition = null!;
    private Query _havePosition = null!;
    private long _sink;


    [GlobalSetup]
    public void Setup()
    {
        _world = new World(EntityCount * 2);
        _entities = new Entity[EntityCount];
        for (var i = 0; i < EntityCount; i++) _entities[i] = _world.Spawn().Add<Tracked>();

        _needPosition = _world.Query<Tracked>().Not<Position>().Compile();
        _havePosition = _world.Query<Tracked>().Has<Position>().Compile();

        foreach (var entity in _entities) entity.Add(new Position(1, 2));
    }


    [GlobalCleanup]
    public void Cleanup() => _world.Dispose();


    [Benchmark(Baseline = true)]
    public void AddRemove_PerEntity()
    {
        foreach (var entity in _entities) entity.Remove<Position>();
        foreach (var entity in _entities) entity.Add(new Position(1, 2));
    }


    [Benchmark]
    public void AddRemove_Batch()
    {
        _havePosition.Batch().Remove<Position>().Submit();
        _needPosition.Batch().Add(new Position(1, 2)).Submit();
    }


    [Benchmark]
    public void Spawn_Despawn_Wave()
    {
        _world.Template().Add(new Position(1, 2)).Add<Wave>().Spawn(EntityCount).Dispose();
        _world.DespawnAllWith<Wave>();
    }


    #region Micro — the building blocks of a dispatch, one per Entity

    /// <summary>An empty World lock, taken and released: the price of every watched change.</summary>
    [Benchmark]
    public void Micro_Lock()
    {
        for (var i = 0; i < EntityCount; i++)
        {
            using var worldLock = _world.Lock();
        }
    }


    /// <summary>Reading a Component by reference: what every Signal handler call does first.</summary>
    [Benchmark]
    public void Micro_Ref()
    {
        long sum = 0;
        foreach (var entity in _entities) sum += (long)entity.Ref<Position>().X;
        _sink = sum;
    }


    /// <summary>The cheapest structural change imaginable: a failed (allowed) Remove.</summary>
    [Benchmark]
    public void Micro_Has()
    {
        long sum = 0;
        foreach (var entity in _entities) sum += entity.Has<Position>() ? 1 : 0;
        _sink = sum;
    }

    #endregion
}
