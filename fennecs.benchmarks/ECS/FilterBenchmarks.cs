using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using fennecs;

namespace Benchmark.ECS;

[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
//[ThreadingDiagnoser]
//[MemoryDiagnoser]
public class FilterBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(100, 1_000, 10_000, 100_000, 1_000_000)]
    public int EntityCount { get; set; }

    private static readonly Random Random = new(1337);

    private World _world = null!;
    
    private Stream<Vector3, int> _streamV3 = null!;
    private Stream<Vector3, int> _streamV3TopHalf = null!;
    private Stream<Vector3, int> _streamV3TopHalfInt = null!;
    private Vector3[] _vectorsRaw = null!;
    private int[] _intsRaw = null!;

    [GlobalSetup]
    public void Setup()
    {
        _world = new();
        _streamV3 = _world.Query<Vector3, int>().Stream();
        _streamV3TopHalf = _streamV3.Where((in Vector3 v) => v.Y > 0.5f);
        _streamV3TopHalfInt = _streamV3.Where((in int i) => i >= 50);

        _vectorsRaw = new Vector3[EntityCount];
        _intsRaw = new int[EntityCount];

        for (var i = 0; i < EntityCount; i++)
        {
            _vectorsRaw[i] = new(Random.NextSingle(), Random.NextSingle(), Random.NextSingle());
            _intsRaw[i] = Random.Next() % 101;

            switch (i % 4)
            {
                case 0:
                    _world.Spawn().Add(_vectorsRaw[i]).Add(_intsRaw[i]);
                    break;
                case 1:
                    _world.Spawn().Add(_vectorsRaw[i]).Add(_intsRaw[i]).Add($"hello{i}");
                    break;
                case 2:
                    _world.Spawn().Add(_vectorsRaw[i]).Add(_intsRaw[i]).Add<float>();
                    break;
                case 3:
                    _world.Spawn().Add(_vectorsRaw[i]).Add(_intsRaw[i]).Add<double>();
                    break;
            }
            
        }
    }

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);

    [Benchmark]
    public int ManualVector()
    {
        var count = 0;
        
        _streamV3.For((ref Vector3 v, ref int i) =>
        {
            if (i > 50) i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }


    [Benchmark]
    public int ManualInteger()
    {
        var count = 0;
        
        _streamV3.For((ref Vector3 v, ref int i) =>
        {
            if (v.Y > 0.5f) i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }
    
    [Benchmark]
    public int FilterVector()
    {
        var count = 0;
        
        _streamV3TopHalf.For((ref Vector3 v, ref int i) =>
        {
            i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }

    [Benchmark]
    public int FilterInteger()
    {
        var count = 0;
        
        _streamV3TopHalfInt.For((ref Vector3 v, ref int i) =>
        {
            i = (int) Vector3.Dot(v, UniformConstantVector);
        });
        
        return count;
    }
}