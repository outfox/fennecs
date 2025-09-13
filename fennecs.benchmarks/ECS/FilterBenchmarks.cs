using System.Numerics;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns("Job", "Error", "RatioSD")]
[MedianColumn]
public class FilterBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(100_000)]
    public int EntityCount { get; set; }

    private static readonly Random Random = new(1337);

    private World _world = null!;
    
    private const int Threshold = 50;
    
    private bool ComponentFilter(in int i) => i >= Threshold;

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);
    
    private Stream<Vector3, int> _streamV3;
    private Stream<Vector3, int> _streamV3Top10Percent;
    private Vector3[] _vectorsRaw = null!;
    private int[] _intsRaw = null!;

    [IterationSetup]
    public void Setup()
    {
        _world = new();
        _streamV3 = _world.Query<Vector3, int>().Stream();
        _streamV3Top10Percent = _streamV3.Where(ComponentFilter);

        _vectorsRaw = new Vector3[EntityCount];
        _intsRaw = new int[EntityCount];
 
        for (var i = 0; i < EntityCount; i++)
        {
            _vectorsRaw[i] = new(Random.NextSingle(), Random.NextSingle(), Random.NextSingle());
            _intsRaw[i] = Random.Next() % 100;

            _world.Spawn().Add(_vectorsRaw[i]).Add(_intsRaw[i]);
        }
    }
    
    
    [Benchmark]
    public void ManualInteger()
    {
        _streamV3.For(delegate (ref Vector3 v, ref int i)
        {
            if (i < Threshold) return;
            v = UniformConstantVector + v;
        });
    }

    [Benchmark]
    public void ManualIntegerEntity()
    {
        _streamV3.For(delegate (in Entity _, ref Vector3 v, ref int i)
        {
            if (i < Threshold) return;
            v = UniformConstantVector + v;
        });
    }
    
    [Benchmark]
    public void FilterInteger()
    {
        _streamV3Top10Percent.For(delegate (ref Vector3 v, ref int _)
        {
            v = UniformConstantVector + v;
        });
    }

    [Benchmark]
    public void FilterIntegerEntity()
    {
        _streamV3Top10Percent.For(delegate (in Entity _, ref Vector3 v, ref int _)
        {
            v = UniformConstantVector + v;
        });
    }
}