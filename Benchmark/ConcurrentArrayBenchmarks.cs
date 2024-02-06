using System.Collections.Concurrent;
using System.Numerics;
using BenchmarkDotNet.Attributes;
namespace Benchmark;

[MemoryDiagnoser(false)]
public class ConcurrentArrayBenchmarks
{
    [Params(1000, 1000000)] 
    public int entityCount { get; set; }

    private static readonly Random random = new(1337);

    private ConcurrentBag<Vector3> _bag = null!;
    private ConcurrentBag<Vector3> _bag2 = null!;
    private List<Vector3> _list = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        var randoms = Enumerable.Range(0, entityCount).Select(_ => new Vector3(random.Next(), random.Next(), random.Next())).ToArray();
        _bag = [..randoms];
        _bag2 = [];
        _list = [..randoms];
    }

    [Benchmark]
    public void TakeOneBag()
    {
        _bag.TryTake(out var x);
    }

    [Benchmark]
    public void FillBag()
    {
        for (var i = 0; i < entityCount; i++)
        {
            _bag2.Add(new Vector3(1,2,3));
        }
    }

    [Benchmark]
    public void TakeAllBag()
    {
        while (!_bag.IsEmpty) _bag.TryTake(out var x);
    }

    [Benchmark]
    public void TakeAllList()
    {
        while (_list.Count > 0)
        {
            var x = _list[^1];
            _list.RemoveAt(_list.Count-1);
        }
    }
}