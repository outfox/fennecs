using System.Numerics;
using BenchmarkDotNet.Attributes;
using fennecs;
using fennecs.storage;

namespace Benchmark.ECS;

[ShortRunJob]
[ThreadingDiagnoser]
[MemoryDiagnoser]
public class SimpleEntityBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int entityCount { get; set; }

    private static readonly Random random = new(1337);

    private World _world = null!;
    
    private Stream<Vector3> _streamV3 = null!;
    private Vector3[] _vectorsRaw = null!;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World();
        _streamV3 = _world.Query<Vector3>().Stream();
        _vectorsRaw = new Vector3[entityCount];

        for (var i = 0; i < entityCount; i++)
        {
            _vectorsRaw[i] = new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle());
            
            //Multiple unused Components added to create fennecs Archetype fragmentation, which is used as basis for many parallel processing partitions.
            switch (i % 4)
            {
                case 0:
                    _world.Spawn().Add(_vectorsRaw[i]);
                    break;
                case 1:
                    _world.Spawn().Add(_vectorsRaw[i]).Add<int>();
                    break;
                case 2:
                    _world.Spawn().Add(_vectorsRaw[i]).Add<double>();
                    break;
                case 3:
                    _world.Spawn().Add(_vectorsRaw[i]).Add<float>();
                    break;
            }
        }
    }

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);

    [Benchmark]
    public void CrossProduct_Parallel_ECS_Delegate_Chunk1k()
    {
        _streamV3.Job(delegate (RW<Vector3> v) { v.write = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void CrossProduct_Parallel_ECS_Delegate_Chunk4k()
    {
        _streamV3.Job(delegate (RW<Vector3> v) { v.write = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void CrossProduct_Single_ECS_Lambda()
    {
        _streamV3.For(static (v) => { v.write = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void CrossProduct_Parallel_ECS_Lambda()
    {
        _streamV3.Job(static (v) => { v.write = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void CrossProduct_Single_ECS_Delegate()
    {
        _streamV3.For(delegate (RW<Vector3> v) { v.write= Vector3.Cross(v, UniformConstantVector); });
    }


    [Benchmark(Baseline = true)]
    public void CrossProduct_Single_ECS_Raw()
    {
        _streamV3.Raw(static vectors =>
        {
            foreach (ref var v in vectors.write)
            {
                v = Vector3.Cross(v, UniformConstantVector);
            }
        });
    }

    [Benchmark]
    public void CrossProduct_Parallel_ECS_Raw()
    {
        _streamV3.Raw(static (vectors) =>
        {
            foreach (ref var v in vectors.write)
            {
                v = Vector3.Cross(v, UniformConstantVector);
            }
        });
    }

    [Benchmark]
    public void CrossProduct_Parallel_ECS_Delegate_Archetype()
    {
        _streamV3.Job(static (v) => { v.write = Vector3.Cross(v, UniformConstantVector); });
    }
}