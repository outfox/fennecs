using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using fennecs;
using fennecs_Components;
using fennecs.pools;
//using fennecs.SIMD;

namespace Benchmark.ECS;

[ShortRunJob]
//[TailCallDiagnoser]
[ThreadingDiagnoser]
[MemoryDiagnoser]
//[InliningDiagnoser(true, true)]
//[HardwareCounters(HardwareCounter.CacheMisses)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[AnyCategoriesFilter("fennecs", "fennecs2")]
// ReSharper disable once IdentifierTypo
public class SIMDBenchmarks
{
    private Stream<Component1, Component2, Component3> _query;
    private World _world = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    [Params(1_000_000)] public int entityCount { get; set; } = 1_000_000;

    // ReSharper disable once MemberCanBePrivate.Global
    [GlobalSetup]
    public void Setup()
    {
        PooledList<UniformWork<Component1, Component2, Component3>>.Rent().Dispose();

        _world = new World();
        _query = _world.Query<Component1, Component2, Component3>().Stream();

        _world.Entity().Add<Component1>()
            .Add(new Component2 {Value = 1})
            .Add(new Component3 {Value = 1})
            .Spawn(entityCount);

        _query.Query.Warmup();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }
    
    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (SIMD)")]
    public void fennecs_SIMD()
    {
        //var simd = new SIMD(_query.Query);
        //simd.AddI32(new Comp<Component1>(), new Comp<Component1>(), new Comp<Component2>(), new Comp<Component3>());
    }

    [BenchmarkCategory("fennecs")]
    [Benchmark(Description = "fennecs (SIMD-burst)")]
    public void fennecs_SIMD_Burst()
    {
        //var simd = new SIMD(_query.Query);
        //simd.SumI32Burst(new Comp<Component1>(), new Comp<Component1>(), new Comp<Component2>(), new Comp<Component3>());
    }
}