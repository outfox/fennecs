using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark.Conceptual;

public class QueryWideVsTall
{
    [Params(1, 2, 3, 4, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000)]
    public int Archetypes { get; set; }
    
    [Params(10_000)]
    public int Entities { get; set; }
    
    private World _world = null!;
    private Stream<int> _stream = null!;

    private Random _valueRandom = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _valueRandom = new(420);
        
        _world = new(Entities * 3);
        
        var unique = _world.Spawn();
        for (var i = 0; i < Entities; i++)
        {
            if (i % (Entities / Archetypes) == 0) unique = _world.Spawn();
            _world.Spawn().Add(_valueRandom.Next(Entities)).Add("relation", unique);
        }
        
        _stream = _world.Query<int>().Has<string>(Match.Any).Stream();
    }

    [Benchmark]
    public int SumOverAllEntities()
    {
        var output = 0;
        _stream.For((ref int value) => { output += value; });
        return output;
    }
}
