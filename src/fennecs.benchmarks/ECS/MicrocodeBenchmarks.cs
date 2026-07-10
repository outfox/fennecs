using System.Numerics;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// Same code, same entity count, same memory layout - the only difference between the
/// two Worlds is the magnitude of their Velocity data. The "cursed" World's velocities
/// are scaled down to ~1e-25, so the squares computed inside Dot and Sqrt underflow
/// into subnormal floats, and each such operation triggers an FP assist: a microcode
/// trap costing tens to hundreds of cycles. Note that the workload never stores a
/// subnormal value anywhere - only the intermediate results are subnormal.
/// </summary>
[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns("Job", "Error", "RatioSD")]
[MedianColumn]
public class MicrocodeBenchmarks
{
    private record struct Position(Vector3 Value);
    private record struct Velocity(Vector3 Value);

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(100_000)]
    public int EntityCount { get; set; }

    private World _healthyWorld = null!;
    private World _cursedWorld = null!;

    private Stream<Position, Velocity> _healthy;
    private Stream<Position, Velocity> _cursed;


    [GlobalSetup]
    public void Setup()
    {
        _healthyWorld = new World(EntityCount);
        _cursedWorld = new World(EntityCount);

        Populate(_healthyWorld, scale: 1f);     // velocity components in 0.5 ... 1.5
        Populate(_cursedWorld, scale: 1e-25f);  // same values, 25 orders of magnitude down

        _healthy = _healthyWorld.Query<Position, Velocity>().Stream();
        _cursed = _cursedWorld.Query<Position, Velocity>().Stream();
    }


    private void Populate(World world, float scale)
    {
        // Same seed for both Worlds - the data differs only in magnitude.
        var random = new Random(1337);

        for (var i = 0; i < EntityCount; i++)
        {
            world.Spawn()
                .Add(new Position(new(random.NextSingle(), random.NextSingle(), random.NextSingle())))
                .Add(new Velocity(new Vector3(
                    random.NextSingle() + 0.5f,
                    random.NextSingle() + 0.5f,
                    random.NextSingle() + 0.5f) * scale));
        }
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _healthyWorld.Dispose();
        _cursedWorld.Dispose();
    }


    private const float DeltaTime = 1f / 60f;

    // A typical little movement integrator. The rotation preserves each dataset's
    // magnitude, so healthy data stays healthy and cursed data stays cursed run
    // after run. With tiny velocities, Dot and Sqrt chew on subnormal intermediates
    // on every single call.
    private static void Tick(ref Position position, ref Velocity velocity)
    {
        var v = velocity.Value;
        v += Vector3.Cross(v, Vector3.UnitY) * DeltaTime;

        var speedSquared = Vector3.Dot(v, v);        // (1e-25)^2 == 1e-50 -> subnormal!
        var heading = v / MathF.Sqrt(speedSquared);  // sqrt of a subnormal -> FP assist

        position = new(Vector3.FusedMultiplyAdd(heading, new Vector3(DeltaTime), position.Value));
        velocity = new(v);
    }


    [Benchmark(Baseline = true)]
    public void Healthy()
    {
        _healthy.For(static (ref p, ref v) => Tick(ref p, ref v));
    }

    [Benchmark]
    public void Cursed()
    {
        _cursed.For(static (ref p, ref v) => Tick(ref p, ref v));
    }
}
