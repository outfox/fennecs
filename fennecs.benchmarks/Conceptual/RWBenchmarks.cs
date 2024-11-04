using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using fennecs;

namespace Benchmark.Conceptual;

public interface ITest
{ }

record struct MyInt(in int value) : Fox<int>;

record struct MyVector(in Vector3 value) : Fox<Vector3>
{
    private Vector3 _value = value;

    public float X
    {
        get => _value.X;
        set => _value.X = value;
    }

    public float Y
    {
        get => _value.Y;
        set => _value.Y = value;
    }

    public float Z
    {
        get => _value.Z;
        set => _value.Z = value;
    }

    public Vector3 value
    {
        readonly get => _value;
        set => _value = value;
    }
}

[ShortRunJob]
public class RWBenchmarks
{
    [Params(1_000, 10_000, 100_000)]
    public int count { get; set; }

    private BenchStream2<int, int> _stream;
    [GlobalSetup]
    public void Setup()
    {
        _stream = new BenchStream2<int, int>(count);
    }
    
    [Benchmark]
    public void OldWW()
    {
        _stream.Old((in Entity entity, ref int a, ref int b) =>
        {
            a++;
            b++;
        });
    }
    [Benchmark]
    public void OldRW()
    {
        _stream.Old((in Entity entity, ref int a, ref int b) =>
        {
            b += a + 1;
        });
    }
    [Benchmark]
    public void OldRR()
    {
        _stream.Old2((in int a, in int b) =>
        {
            var c = a + b;
            if (c > 10000) Console.WriteLine("Jackpot");
        });
    }
    [Benchmark]
    public void NewWW()
    {
        _stream.New((a, b) =>
        {
            a.write++;
            b.write++;
        });
    }

    [Benchmark]
    public void NewERW()
    {
        _stream.New((entity, a, b) =>
        {
            a.write++;
        });
    }
    [Benchmark]
    public void NewRW()
    {
        _stream.New((a, b) =>
        {
            b.write += a.read + 1;
        });
    }
}

internal interface Fox<T>where T : notnull
{
   T value { get; set; }
}

internal readonly ref struct R<T>(ref T val) where T : notnull
{
    private readonly ref T _value = ref val;
    public T read => _value;

    public static implicit operator T(R<T> self) => self._value;
}

internal readonly ref struct RW<T>(ref T val, ref Entity entity) where T : notnull
{
    private readonly ref Entity _entity = ref entity;
    private readonly ref T _value = ref val;
    public T In => _value;
    public T write
    {
        get => _value;
        set
        {
            if (typeof(T).IsAssignableFrom(typeof(ITest)))
            {
                Console.WriteLine($"WTF + {_entity.ToString()}");
            }
            //TODO: Notify for change.
            _value = value;
        }
    }
    
    public T Consume()
    {
        _entity.Remove<T>();
        return _value;
    }

    private void Test(in int a)
    {
        
    }
}

internal readonly record struct BenchStream2<C1, C2>(int Count) 
    where C1 : notnull
    where C2 :  notnull 
{
    public readonly Entity[] entities = new Entity[Count];
    public readonly C1[] Data1 = new C1[Count];
    public readonly C2[] Data2 = new C2[Count];
    
    public void New(ComponentActionEWR<C1, C2> action)
    {
        for (var i = 0; i < Count; i++) action(entities[i], new RW<C1>(ref Data1[i], ref entities[i]), new R<C2>(ref Data2[i]));
    }
    public void Old(EntityComponentAction<C1, C2> action)
    {
        for (var i = 0; i < Count; i++) action(entities[i], ref Data1[i], ref Data2[i]);
    }

    public void Old2(ComponentActionRead<C1, C2> action)
    {
        for (var i = 0; i < Count; i++) action(in Data1[i], in Data2[i]);
    }
    
    [OverloadResolutionPriority(0)]
    public void New(ComponentActionWW<C1, C2> action)
    {
        for (var i = 0; i < Count; i++)
        {
            action(new RW<C1>(ref Data1[i], ref entities[i]), new RW<C2>(ref Data2[i], ref entities[i]));
        }
    }
    [OverloadResolutionPriority(1)]
    public void New(ComponentActionRW<C1, C2> action)
    {
        for (var i = 0; i < Count; i++)
        {
            action(new R<C1>(ref Data1[i]), new RW<C2>(ref Data2[i], ref entities[i]));
        }
    }

    [OverloadResolutionPriority(2)]
    public void New(ComponentActionWR<C1, C2> action)
    {
        for (var i = 0; i < Count; i++) action(new RW<C1>(ref Data1[i], ref entities[i]), new R<C2>(ref Data2[i]));
    }
    
    [OverloadResolutionPriority(3)]
    public void New(ComponentActionRR<C1, C2> action)
    {
        for (var i = 0; i < Count; i++) action(new R<C1>(ref Data1[i]), new R<C2>(ref Data2[i]));
    }
}

internal delegate void ComponentActionRead<C0, C1>(in C0 comp0, in C1 comp1);

file delegate void ComponentActionER<C0>(Entity entity, R<C0> comp0) where C0 : notnull;
file delegate void ComponentActionEW<C0>(Entity entity, RW<C0> comp0) where C0 : notnull;
file delegate void ComponentActionW<C0>(RW<C0> comp0) where C0 : notnull;
file delegate void ComponentActionR<C0>(R<C0> comp0) where C0 : notnull;
internal delegate void ComponentActionEWW<C0, C1>(Entity entity, RW<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionEWR<C0, C1>(Entity entity, RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionWR<C0, C1>(RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionRW<C0, C1>(R<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionWW<C0, C1>(RW<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionRR<C0, C1>(R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;