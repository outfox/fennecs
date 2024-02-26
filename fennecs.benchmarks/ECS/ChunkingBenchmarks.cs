using System.Numerics;
using BenchmarkDotNet.Attributes;
using fennecs;
using fennecs.pools;

namespace Benchmark.ECS;

[ShortRunJob]
[ThreadingDiagnoser]
[MemoryDiagnoser]
//[HardwareCounters(HardwareCounter.CacheMisses)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class ChunkingBenchmarks
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [Params(1_000_000)] public int entityCount { get; set; } = 1_000_000;
    [Params(4096, 16384, 32768)] public int chunkSize { get; set; } = 16384;

    private static readonly Random random = new(1337);

    private World _world = null!;

    private Query<Vector3> _queryV3 = null!;
    private Vector3[] _vectorsRaw = null!;

    [GlobalSetup]
    public void Setup()
    {
        PooledList<Work<Vector3>>.Rent().Dispose();
        PooledList<UniformWork<Vector3, Vector3>>.Rent().Dispose();
        
        //ThreadPool.SetMaxThreads(24, 24);
        using var countdown = new CountdownEvent(500);
        for (var i = 0; i < 500; i++)
        {
            ThreadPool.UnsafeQueueUserWorkItem
            (_ =>
            {
                Thread.Sleep(1);
                // ReSharper disable once AccessToDisposedClosure
                countdown.Signal();
            }, true);
        }
        countdown.Wait();
        Thread.Yield();

        _world = new World();
        _queryV3 = _world.Query<Vector3>().Build();
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

    [GlobalCleanup]
    public void Cleanup()
    {
        _queryV3 = null!;
        _world.Dispose();
        _world = null!;
    }


    private static readonly Vector3 UniformConstantVector = new(3, 4, 5);

    [Benchmark]
    public void CrossProduct_Run()
    {
        _queryV3.ForEach(static (ref Vector3 v) => { v = Vector3.Cross(v, UniformConstantVector); });
    }

    [Benchmark]
    public void CrossProduct_RunU()
    {
        _queryV3.ForEach(static (ref Vector3 v, Vector3 uniform) => { v = Vector3.Cross(v, uniform); }, UniformConstantVector);
    }

    [Benchmark]
    public void CrossProduct_Job()
    {
        _queryV3.Parallel(delegate(ref Vector3 v) { v = Vector3.Cross(v, UniformConstantVector); }, chunkSize);
    }

    [Benchmark]
    public void CrossProduct_JobU()
    {
        _queryV3.Parallel(delegate(ref Vector3 v, Vector3 uniform) { v = Vector3.Cross(v, uniform); }, UniformConstantVector, chunkSize);
    }
}