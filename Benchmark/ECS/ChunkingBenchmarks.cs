using System.Numerics;
using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark;


[ShortRunJob]
[ThreadingDiagnoser]
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class ChunkingBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int entityCount { get; set; }

    [Params(128, 512, 1024, 2048, 4096, 8192, 16384, 32768)] public int chunkSize { get; set; }

    private static readonly Random random = new(1337);

    private World _world = null!;

    private Query<Vector3> _queryV3 = null!;
    private Vector3[] _vectorsRaw = null!;

    [GlobalSetup]
    public void Setup()
    {
        _world = new World();
        _queryV3 = _world.Query<Vector3>().Build();
        _vectorsRaw = new Vector3[entityCount];

        for (var i = 0; i < entityCount; i++)
        {
            _vectorsRaw[i] = new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle());

            //Multiple unused components added to create fennecs archetype fragmentation, which is used as basis for many parallel processing partitions.
            switch (i % 4)
            {
                case 0:
                    _world.Spawn().Add(_vectorsRaw[i]).Id();
                    break;
                case 1:
                    _world.Spawn().Add(_vectorsRaw[i]).Add<int>().Id();
                    break;
                case 2:
                    _world.Spawn().Add(_vectorsRaw[i]).Add<double>().Id();
                    break;
                case 3:
                    _world.Spawn().Add(_vectorsRaw[i]).Add<float>().Id();
                    break;
            }
        }
    }

    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);
    private static readonly ParallelOptions options = new() {MaxDegreeOfParallelism = 12};

    [Benchmark]
    //Work parallelized by Archetype, passed into delegate as ref Vector3.
    public void CrossProduct_Parallel_ECS_Delegate_Chunk()
    {
        _queryV3.RunParallel(delegate(ref Vector3 v) { v = Vector3.Cross(v, UniformConstantVector); }, chunkSize);
    }
}