using System.Numerics;
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
    private Stream<int> _ints = null!;
    private Stream<Matrix4x4> _mats = null!;

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
            var matrix = new Matrix4x4();
            for (int j = 0; j < 16; j++) matrix[j /4, j % 4] = _valueRandom.NextSingle();
            
            _world.Spawn().Add(_valueRandom.Next(Entities)).Add(matrix).Add("relation", unique);
        }
        
        _ints = _world.Query<int>().Has<string>(Match.Any).Stream();
        _mats = _world.Query<Matrix4x4>().Has<string>(Match.Any).Stream();
    }

    [Benchmark]
    public int Sum()
    {
        var output = 0;
        _ints.For(value => { output += value; });
        return output;
    }

    [Benchmark]
    public Vector4 MatrixMul()
    {
        var vector = Vector4.One;
        _mats.For(matrix => { vector = Vector4.Transform(vector, matrix); });
        return vector;
    }
}
