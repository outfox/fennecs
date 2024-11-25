using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using fennecs;
using fennecs_Components;
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
public class FusedMultiplyAdd
{
    private Stream<Position, Velocity, Acceleration> _query = null!;
    private World _world = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(100_000)] public int EntityCount { get; set; } = 0;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World();
        _query = _world.Query<Position, Velocity, Acceleration>().Stream();

        var rnd = new Random(1337);
        for (var i = 0; i < EntityCount; ++i)
        {
            _world.Spawn().Add<Position>()
                .Add(new Velocity {Value = new(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle())})
                .Add(new Acceleration {Value = new(rnd.NextSingle(), rnd.NextSingle(), rnd.NextSingle())});
        }

        fennecs_Raw_AVXPVA();
        _query.Job(1.0f/60.0f, Integrate);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Integrate For)", Baseline = true)]
    public void fennecs_For_PVA()
    {
        _query.For(1.0f / 60f, Integrate);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Integrate Job)")]
    public void fennecs_Job_PVA()
    {
        _query.Job(1.0f / 60f, Integrate);
    }

    [BenchmarkCategory("fennecs", nameof(Avx512F))]
    [Benchmark(Description = "fennecs (Integrate Raw AVX512 FMA)")]
    public void fennecs_Raw_AVX512()
    {
        _query.Raw(1.0f / 60f, Integrate_AVX512_FMA);
    }

    [BenchmarkCategory("fennecs", nameof(Avx2))]
    [Benchmark(Description = "fennecs (Integrate Raw AVX2)")]
    public void fennecs_Raw_AVXPVA()
    {
        _query.Raw(1.0f / 60f, Integrate_AVX2);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Integrate Raw FMA)")]
    public void fennecs_Raw_FMA()
    {
        _query.Raw(1.0f / 60f, Integrate_Raw_FMA);
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (Integrate For FMA)")]
    public void fennecs_For_FMA()
    {
        _query.For(1.0f / 60f, Integrate_FMA);
    }

    


    private static void Integrate(float deltaTime, RW<Position> pos, RW<Velocity> vel, R<Acceleration> acc)
    {
        vel.write.Value += acc.read.Value * deltaTime;
        pos.write.Value += vel.write.Value * deltaTime;
    }

    private static void Integrate_FMA(float deltaTime, RW<Position> pos, RW<Velocity> vel, R<Acceleration> acc)
    {
        var dt = Vector3.Create(deltaTime);
        vel.write.Value = Vector3.FusedMultiplyAdd(acc.read.Value, dt, vel.write.Value);
        pos.write.Value = Vector3.FusedMultiplyAdd(vel.write.Value, dt, pos.write.Value);
    }

    private static void Integrate_Raw_FMA(float deltaTime, Span<Position> pos, Span<Velocity> vel,
        ReadOnlySpan<Acceleration> acc)
    {
        var dt = Vector3.Create(deltaTime);
        for (var i = 0; i < pos.Length; i++)
        {
            vel[i] = new(Vector3.FusedMultiplyAdd(acc[i].Value, dt, vel[i].Value));
            pos[i] = new(Vector3.FusedMultiplyAdd(vel[i].Value, dt, pos[i].Value));
        }
    }

    private static void Integrate_AVX2(float deltaTime, Span<Position> pos, Span<Velocity> vel,
        ReadOnlySpan<Acceleration> acc)
    {
        var count = pos.Length;
        var dt = Vector256.Create(deltaTime);

        unsafe
        {
            fixed (Position* pw = pos)
            fixed (Velocity* vw = vel)
            fixed (Acceleration* ar = acc)
            {
                var p0 = (float*) pw;
                var v0 = (float*) vw;
                var a0 = (float*) ar;

                var vectorSize = Vector256<float>.Count;
                var vectorEnd = count - count % vectorSize;
                for (var i = 0; i < vectorEnd; i += vectorSize)
                {

                    var p = Avx.LoadVector256(p0 + i);
                    var v = Avx.LoadVector256(v0 + i);
                    var a = Avx.LoadVector256(a0 + i);

                    var accI = Avx.Multiply(a, dt);
                    var velV = Avx.Add(v, accI);
                    Avx.Store(v0 + i, velV);

                    var velI = Avx.Multiply(velV, dt);
                    var posV = Avx.Add(p, velI);
                    Avx.Store(p0 + i, posV);
                }

                for (var i = vectorEnd; i < count; i++) // remaining elements
                {
                    v0[i] += a0[i] * deltaTime;
                    p0[i] += v0[i] * deltaTime;
                }
            }
        }
    }


    private static void Integrate_AVX512_FMA(float deltaTime, Span<Position> pos, Span<Velocity> vel,
        ReadOnlySpan<Acceleration> acc)
    {
        var count = pos.Length;
        var dt = Vector512.Create(deltaTime);

        unsafe
        {
            fixed (Position* pw = pos)
            fixed (Velocity* vw = vel)
            fixed (Acceleration* ar = acc)
            {
                var p0 = (float*) pw;
                var v0 = (float*) vw;
                var a0 = (float*) ar;

                var vectorSize = Vector512<float>.Count;
                var vectorEnd = count - count % vectorSize;
                for (var i = 0; i < vectorEnd; i += vectorSize)
                {
                    var p = Avx512F.LoadVector512(p0 + i);
                    var v = Avx512F.LoadVector512(v0 + i);
                    var a = Avx512F.LoadVector512(a0 + i);

                    // v = v + (a * dt)
                    var velV = Avx512F.FusedMultiplyAdd(a, dt, v);
                    Avx512F.Store(v0 + i, velV);

                    // p = p + (v * dt)
                    var posV = Avx512F.FusedMultiplyAdd(velV, dt, p);
                    Avx512F.Store(p0 + i, posV);
                }

                for (var i = vectorEnd; i < count; i++) // remaining elements
                {
                    v0[i] += a0[i] * deltaTime;
                    p0[i] += v0[i] * deltaTime;
                }
            }
        }
    }
}