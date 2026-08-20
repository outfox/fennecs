using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// Measures what Component Signals cost the structural change paths that emit them:
/// the guard when nothing is watching, the registry probe when a Signal exists but nobody
/// subscribed, and the dispatch itself for one and for several handlers.
/// </summary>
/// <remarks>
/// Every benchmark is symmetric — it leaves the World exactly as it found it — so iterations
/// are comparable without an IterationSetup skewing the numbers.
/// </remarks>
[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared)]
[HideColumns("Job", "Error", "RatioSD")]
public class SignalBenchmarks
{
    private record struct Position(float X, float Y);
    private record struct Unwatched(float X);
    private record struct Tracked;
    private record struct Wave;

    /// <summary>What is watching <see cref="Position"/> while the benchmark mutates it.</summary>
    public enum Watchers
    {
        /// <summary>No Signal exists at all — the <c>Signalling</c> guard short-circuits.</summary>
        None,

        /// <summary>A Signal exists, but for a Component type nobody touches: a registry miss.</summary>
        OtherType,

        /// <summary>A Signal for the mutated type exists, with no subscribers: probe, but no invoke.</summary>
        Idle,

        /// <summary>One subscriber on each of Added and Removed.</summary>
        One,

        /// <summary>Four subscribers on each of Added and Removed.</summary>
        Four,
    }

    [Params(Watchers.None, Watchers.OtherType, Watchers.Idle, Watchers.One, Watchers.Four)]
    public Watchers Watching { get; set; }

    [Params(10_000)]
    public int EntityCount { get; set; }

    private World _world = null!;
    private Entity[] _entities = null!;

    private Query _needPosition = null!;
    private Query _havePosition = null!;

    private long _counter;


    [GlobalSetup]
    public void Setup()
    {
        _world = new World(EntityCount * 2);

        _entities = new Entity[EntityCount];
        for (var i = 0; i < EntityCount; i++) _entities[i] = _world.Spawn().Add<Tracked>();

        _needPosition = _world.Query<Tracked>().Not<Position>().Compile();
        _havePosition = _world.Query<Tracked>().Has<Position>().Compile();

        switch (Watching)
        {
            case Watchers.None:
                break;

            case Watchers.OtherType:
                _world.On<Unwatched>().Added += Bump;
                _world.On<Unwatched>().Removed += Drop;
                break;

            case Watchers.Idle:
                _world.On<Position>();  // materializes the Signal without subscribing
                break;

            case Watchers.One:
                _world.On<Position>().Added += Bump;
                _world.On<Position>().Removed += Drop;
                break;

            case Watchers.Four:
                for (var i = 0; i < 4; i++)
                {
                    _world.On<Position>().Added += Bump;
                    _world.On<Position>().Removed += Drop;
                }
                break;
        }
    }


    [GlobalCleanup]
    public void Cleanup() => _world.Dispose();


    // Deliberately trivial: we are measuring fennecs dispatch, not the handler' own work.
    private void Bump<T>(EntityRef entity, ref T value) => _counter++;

    private void Drop<T>(EntityRef entity, in T value, RemoveCause cause) => _counter++;


    /// <summary>Per-Entity CRUD: one Archetype move per Entity, per direction.</summary>
    [Benchmark(Baseline = true)]
    public void AddRemove_PerEntity()
    {
        foreach (var entity in _entities) entity.Add(new Position(1, 2));
        foreach (var entity in _entities) entity.Remove<Position>();
    }


    /// <summary>Bulk CRUD: two Archetype migrations total, but one Signal per Entity.</summary>
    [Benchmark]
    public void AddRemove_Batch()
    {
        _needPosition.Batch().Add(new Position(1, 2)).Submit();
        _havePosition.Batch().Remove<Position>().Submit();
    }


    /// <summary>A spawn wave and its despawn: the Template and Despawn paths.</summary>
    [Benchmark]
    public void Spawn_Despawn_Wave()
    {
        _world.Template()
            .Add(new Position(1, 2))
            .Add<Wave>()
            .Spawn(EntityCount)
            .Dispose();

        _world.DespawnAllWith<Wave>();
    }
}
