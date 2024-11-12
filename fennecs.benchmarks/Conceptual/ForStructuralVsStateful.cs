using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using fennecs;

namespace Benchmark.Conceptual;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class ForStructuralVsStateful
{
    [Params(10_000)]
    public int Entities { get; set; }
    
    [Params(0.9f, 0.5f, 0.1f)]
    public float Homogenity { get; set; }
    
    [Params(100, 1_000, 10_000)]
    public int Actions { get; set; }
    
    private World _world = null!;
    private Stream<ushort> _stream = null!;
    private Random _random = null!;

    [GlobalSetup]
    public void GlobaSetup()
    {
        _world = new(Entities * 30);

        _stream = _world.Query<ushort>().Stream();
        
        _world.Spawn().Add(1).Despawn();
    }
    
    [IterationSetup]
    public void IterationSetup()
    {
        Console.WriteLine("IterationSetup");
        _world.All.Despawn();
        if (_world.Count != 0) throw new("World is not empty!");
        
        _random = new(69);
        
        _world.Entity().Spawn(Entities);
        foreach (var entity in _world.All.ToArray())
        {
            entity.Add((ushort) _random.Next((ushort) (Actions * Homogenity), Actions));
        }
    }

    [Benchmark]
    public int CountDown_with_Structural_Change()
    {
        while (_stream.Count > 0)
        {
            _stream.For((in Entity entity, ref ushort value) =>
            {
                value--;
                if (value <= 0) entity.Remove<ushort>();
            });
        }
        return _stream.Count;
    }

    [Benchmark]
    public int CountDown_with_Delayed_Change()
    {
        var done = false;
        while (!done)
        {
            done = true;
            _stream.For((ref ushort value) =>
            {
                if (value <= 0) return;
                value--;
                done = false; 
            });
        }
        _stream.Query.Remove<ushort>();
        return _stream.Count;
    }

    private record Done(bool done)
    {
        public bool done { get; set; } = done;
    };

    [Benchmark]
    public int CountDown_with_Raw_Loop()
    {
        var done = new Done(false);

        while (!done.done)
        {
            _stream.Raw(values =>
                {
                    var localDone = true;
                    var span = values.Span;
                    for (var i = 0; i < span.Length; i++)
                    {
                        if (span[i] <= 0) continue;
                        localDone = false;
                        span[i]--;
                    }
                    done.done = localDone;
                }
            );
        }
        _stream.Query.Remove<ushort>();
        return _stream.Count;
    }

    [Benchmark]
    public int CountDown_with_Raw_SIMD()
    {
        var done = new Done(false);

        while (!done.done)
        {
            _stream.Raw(values =>
            {
                var count = values.Length;
                var localDone = true;
                
                using var mem1 = values.Pin();

                unsafe
                {
                    var p1 = (ushort*)mem1.Pointer;

                    var vectorSize = Vector256<ushort>.Count;
                    var vectorEnd = count - count % vectorSize;
                    for (var i = 0; i <= vectorEnd; i += vectorSize)
                    {
                        var v1 = Avx.LoadVector256(p1 + i);
                        var sum = Avx2.SubtractSaturate(v1, Vector256<ushort>.One);
                        Avx.Store(p1 + i, sum);
                        localDone &= sum == Vector256<ushort>.Zero;
                    }
                }
                done.done = localDone;
            }
            );
        }
        _stream.Query.Remove<ushort>();
        return _stream.Count;
    }
}
