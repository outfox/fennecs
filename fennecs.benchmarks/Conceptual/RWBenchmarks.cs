﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using fennecs;
using fennecs.storage;

namespace Benchmark.Conceptual;

#if !NET9_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class OverloadResolutionPriorityAttribute : Attribute
    {
        /// <summary>
        /// DUMMY REPLACEMENT FOR NET9_0 Attribute - does nothing.
        /// Initializes a new instance of the <see cref="OverloadResolutionPriorityAttribute"/> class.
        /// </summary>
        /// <param name="priority">The priority of the attributed member. Higher numbers are prioritized, lower numbers are deprioritized. 0 is the default if no attribute is present.</param>
        public OverloadResolutionPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// The priority of the member.
        /// </summary>
        public int Priority { get; }
    }
#endif

public class RWBenchmarks
{
    [Params(1_000)]
    public int count { get; set; }

    private BenchStream2<int, int> _stream;
    [GlobalSetup]
    public void Setup()
    {
        _stream = new(count);
    }

    [Benchmark]
    public void OldNOP()
    {
        _stream.Old((in Entity entity, [In] ref int a, [In] ref int b) =>
            { });
    }

    [Benchmark(Baseline = true)]
    public void OldWW()
    {
        _stream.Old((in Entity entity, ref int a, ref int b) =>
        {
            a++;
            b++;
        });
    }

    [Benchmark]
    public void OldWRo()
    {
        _stream.Old((ref int a, ref readonly int b) =>
        {
            a += b;
        });
    }

    [Benchmark]
    public void OldEWR()
    {
        _stream.Old((in Entity entity, ref int a, ref int b) =>
        {
            a += b + 1;
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
    public void NewRR()
    {
        _stream.New((a, b) =>
        {
            _ = a.read + b.read;
        });
    }

    [Benchmark]
    public void NewWR()
    {
        _stream.New((a, b) =>
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

internal interface Fox<T> where T : notnull
{
    T value { get; set; }
}

internal readonly record struct BenchStream2<C1, C2>(int Count)
    where C1 : notnull
    where C2 : notnull
{
    public readonly Entity[] entities = new Entity[Count];
    public readonly C1[] Data1 = new C1[Count];
    public readonly C2[] Data2 = new C2[Count];


    public void New(ComponentActionEWR<C1, C2> action)
    {
        var match = default(TypeExpression);
        for (var i = 0; i < Count; i++) action(new(ref entities[i]), new(ref Data1[i], ref entities[i], ref match), new(ref Data2[i]));
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
        var match = default(TypeExpression);
        for (var i = 0; i < Count; i++)
        {
            action(new(ref Data1[i], ref entities[i], ref match), new(ref Data2[i], ref entities[i], ref match));
        }
    }
    [OverloadResolutionPriority(1)]
    public void New(ComponentActionRW<C1, C2> action)
    {
        var match = TypeExpression.Of<C2>(Match.Any);
        for (var i = 0; i < Count; i++)
        {
            var ref1 = new R<C1>(ref Data1[i]);
            var ref2 = new RW<C2>(ref Data2[i], ref entities[i], ref match);
            action(ref1, ref2);
        }
    }

    [OverloadResolutionPriority(1)]
    public void New(ComponentActionWR<C1, C2> action)
    {
        var match = default(TypeExpression);
        for (var i = 0; i < Count; i++)
        {
            var ref1 = new RW<C1>(ref Data1[i], ref entities[i], in match);
            var ref2 = new R<C2>(ref Data2[i]);
            action(ref1, ref2);
        }
    }

    [OverloadResolutionPriority(2)]
    public void NewRo(ComponentActionWR<C1, C2> action)
    {
        var match = TypeExpression.Of<C1>(Match.Any);
        for (var i = 0; i < Count; i++)
        {
            var ref1 = new RW<C1>(ref Data1[i], ref entities[i], ref match);
            var ref2 = new R<C2>(ref Data2[i]);
            action(ref1, ref2);
        }
    }

    [OverloadResolutionPriority(3)]
    public void New(ComponentActionRR<C1, C2> action)
    {
        for (var i = 0; i < Count; i++)
        {
            var ref1 = new R<C1>(ref Data1[i]);
            var ref2 = new R<C2>(ref Data2[i]);
            action(ref1, ref2);
            //action(new(ref Data1[i]), new(ref Data2[i]));
        }
    }


    public void Old(ComponentActionWRo<C1, C2> action)
    {
        for (var i = 0; i < Count; i++) action(ref Data1[i], ref Data2[i]);
    }
}

internal delegate void ComponentActionRead<C0, C1>(in C0 comp0, in C1 comp1);
internal delegate void ComponentActionWRo<C0, C1>(ref C0 comp0, ref readonly C1 comp1);
internal delegate void ComponentActionEWR<C0, C1>([In] EntityRef entity, RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionWR<C0, C1>(RW<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionRW<C0, C1>(R<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionWWref<C0, C1>(ref readonly RW<C0> comp0, ref readonly RW<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionWW<C0, C1>(RW<C0> comp0, RW<C1> comp1) where C0 : notnull where C1 : notnull;
internal delegate void ComponentActionRR<C0, C1>(R<C0> comp0, R<C1> comp1) where C0 : notnull where C1 : notnull;
