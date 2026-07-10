using System.Numerics;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// Compares iterating hot data (Position + Velocity) in a fragmented World against a World
/// where the hot types are grouped into a dedicated Aspect.
///
/// Fragmentation is induced by a cold Grouping relation with <see cref="Fragments"/> distinct
/// targets - each target splinters off its own Archetype. In the fragmented World, the hot
/// components are dragged along into those splinters; in the Aspect World, the "hot" Aspect
/// keeps them in a single contiguous Archetype while Main fragments on its own.
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns("Job", "Error", "RatioSD")]
[MedianColumn]
public class AspectFragmentationBenchmarks
{
    private record struct Position(Vector3 Value);
    private record struct Velocity(Vector3 Value);

    // Cold data: never touched by the workload, exists only to fragment storage.
    private record struct Grouping;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(100_000)]
    public int EntityCount { get; set; }

    // Distinct Archetypes the Grouping relation splinters the entities into.
    // 100_000 entities -> ~6250, ~390, ~24 entities per Archetype.
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(16, 256, 4096)]
    public int Fragments { get; set; }

    private World _fragmentedWorld = null!;
    private World _aspectWorld = null!;

    private Stream<Position, Velocity> _fragmentedStream;
    private Stream<Position, Velocity> _aspectStream;


    [GlobalSetup]
    public void Setup()
    {
        _fragmentedWorld = new World(EntityCount);
        _aspectWorld = new World(EntityCount);

        // The only difference between the two Worlds: hot types get their own Aspect here.
        var hot = _aspectWorld.AddAspect("hot").Owns<Position>().Owns<Velocity>();

        Populate(_fragmentedWorld);
        Populate(_aspectWorld);

        _fragmentedStream = _fragmentedWorld.Query<Position, Velocity>().Stream();
        _aspectStream = hot.Query<Position, Velocity>().Stream();

        if (hot.Count != EntityCount) throw new InvalidOperationException("hot Aspect membership mismatch");
    }


    private void Populate(World world)
    {
        // Same seed per World, so both iterate identical data.
        var random = new Random(1337);

        var targets = new Entity[Fragments];
        for (var i = 0; i < Fragments; i++) targets[i] = world.Spawn();

        for (var i = 0; i < EntityCount; i++)
        {
            world.Spawn()
                .Add(new Position(new(random.NextSingle(), random.NextSingle(), random.NextSingle())))
                .Add(new Velocity(new(random.NextSingle(), random.NextSingle(), random.NextSingle())))
                .Add(new Grouping(), targets[i % Fragments]); // each distinct target = its own Archetype
        }
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _fragmentedWorld.Dispose();
        _aspectWorld.Dispose();
    }


    private const float DeltaTime = 1f / 60f;

    // A reasonably heavy per-entity tick: swirl, clamp, normalize, integrate.
    // CAUTION: these dynamics must stay numerically stationary. An earlier variant
    // included gravity, which made velocities converge on "terminal velocity" - the
    // lateral components decayed exponentially until their squares (in Length/
    // Normalize/Cross) underflowed to subnormal floats, whose microcode penalty
    // then flipped the whole benchmark ~3x slower after ~12k ticks per entity.
    // The pure rotation preserves component magnitudes indefinitely.
    private static void Tick(ref Position position, ref Velocity velocity)
    {
        var v = velocity.Value;
        v += Vector3.Cross(v, Vector3.UnitY) * DeltaTime;
        var speed = Math.Clamp(v.Length(), 2f, 42f);
        v = Vector3.Normalize(v) * speed;

        position = new(Vector3.FusedMultiplyAdd(v, new Vector3(DeltaTime), position.Value));
        velocity = new(v);
    }


    [Benchmark(Baseline = true)]
    public void Fragmented_For()
    {
        _fragmentedStream.For(static (ref p, ref v) => Tick(ref p, ref v));
    }

    [Benchmark]
    public void Aspect_For()
    {
        _aspectStream.For(static (ref p, ref v) => Tick(ref p, ref v));
    }

    [Benchmark]
    public void Fragmented_Job()
    {
        _fragmentedStream.Job(static (ref p, ref v) => Tick(ref p, ref v));
    }

    [Benchmark]
    public void Aspect_Job()
    {
        _aspectStream.Job(static (ref p, ref v) => Tick(ref p, ref v));
    }

    private static void RawLoop(Memory<Position> positions, Memory<Velocity> velocities)
    {
        var p = positions.Span;
        var v = velocities.Span;
        for (var i = 0; i < p.Length; i++) Tick(ref p[i], ref v[i]);
    }

    [Benchmark]
    public void Fragmented_Raw()
    {
        _fragmentedStream.Raw(RawLoop);
    }

    [Benchmark]
    public void Aspect_Raw()
    {
        _aspectStream.Raw(RawLoop);
    }
}
