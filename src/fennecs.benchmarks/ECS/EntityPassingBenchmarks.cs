using System.Numerics;
using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark.ECS;

/// <summary>
/// Measures the cost of receiving the entity in a runner delegate, against a component-only
/// baseline. Written to compile unchanged pre- and post- the 32-bit entity refactor
/// (implicitly-typed lambdas bind to 'in Entity' before, 'in EntityRef' after).
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
public class EntityPassingBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(100_000, 1_000_000)]
    public int entityCount { get; set; }

    private World _world = null!;
    private Stream<Vector3> _stream;
    private Entity[] _entities = null!;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World(entityCount);
        _stream = _world.Query<Vector3>().Stream();
        _entities = new Entity[entityCount];

        for (var i = 0; i < entityCount; i++)
        {
            _entities[i] = _world.Spawn().Add(new Vector3(i, i, i));
        }
    }

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);

    [Benchmark(Baseline = true)]
    public void Component_Only()
    {
        _stream.For((ref Vector3 v) => { v = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void Entity_Passed_Unused()
    {
        _stream.For((in e, ref v) => { v = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void Entity_Passed_Stored()
    {
        _stream.For((in e, ref v) =>
        {
            Entity stored = e;
            v = Vector3.Cross(v, UniformConstantVector) + new Vector3((int) (stored.ToRaw() & 1), 0, 0);
        });
    }

    [Benchmark]
    public Vector3 Fluent_Ref_Per_Entity()
    {
        var sum = Vector3.Zero;
        var entities = _entities;
        for (var i = 0; i < entities.Length; i++)
        {
            sum += entities[i].Ref<Vector3>(default);
        }
        return sum;
    }

    [Benchmark]
    public int Fluent_Has_Per_Entity()
    {
        var count = 0;
        var entities = _entities;
        for (var i = 0; i < entities.Length; i++)
        {
            if (entities[i].Has<Vector3>()) count++;
        }
        return count;
    }
}
