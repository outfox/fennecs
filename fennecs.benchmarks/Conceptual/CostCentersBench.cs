using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark.Conceptual;

public class CostCentersBench
{
    [Params(10000)]
    public int Count { get; set; }
    
    private World _world = null!;
    private Stream<int> _streamOne = null!;
    private Stream<int> _streamMany = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _world = new(Count * 3);
        
        for (var i = 0; i < Count; i++)
        {
            _world.Spawn().Add(i);
        }

        for (var i = 0; i < Count; i++)
        {
            var unique = _world.Spawn();
            _world.Spawn().Add(i).Add("relation", unique);
        }
        
        _streamOne = _world.Query<int>().Not<string>(Match.Any).Stream();
        _streamMany = _world.Query<int>().Has<string>(Match.Any).Stream();
        
        Console.WriteLine($"World: {_world.Count} Entities");
        Console.WriteLine($"Stream One: {_streamOne.Count} Entities, {_streamOne.Query.Archetypes.Count} Archetypes");
        Console.WriteLine($"Stream Many: {_streamMany.Count} Entities, {_streamMany.Query.Archetypes.Count} Archetypes");
    }

    [Benchmark]
    public int SingleArchetype()
    {
        var output = 0;
        _streamOne.For((ref int value) => { output += value; });
        return output;
    }

    [Benchmark]
    public int ManyArchetypes()
    {
        var output = 0;
        _streamMany.For((ref int value) => { output += value; });
        return output;
    }
}
