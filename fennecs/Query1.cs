// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// <para>
/// Query with 1 output Stream Type, <c>C0</c>.
/// </para>
/// <para>
/// Queries expose methods to rapidly iterate all Entities that match their Mask and Stream Types.
/// </para>
/// <ul>
/// <li><c>ForEach(...)</c> - call a delegate <see cref="RefAction{C0}"/> for each Entity.</li>
/// <li><c>Job(...)</c> - parallel process, calling a delegate <see cref="RefAction{C0}"/> for each Entity.</li>
/// <li><c>Raw(...)</c> - pass Memory regions / Spans to a delegate <see cref="MemoryAction{C0}"/> per matched Archetype (× matched Wildcards) of entities.</li>
/// </ul>
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class Query<C0> : Query
{
    #region Internals

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{C0}"/> class.
    /// </summary>
    /// <param name="world">The world context for the query.</param>
    /// <param name="streamTypes">The stream types for the query.</param>
    /// <param name="mask">The mask for the query.</param>
    /// <param name="archetypes">The archetypes for the query.</param>
    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }

    #endregion


    #region Runners

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(RefAction<C0> action)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        foreach (var table in Archetypes)
        {
            var count = table.Count;

            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var s0 = join.Select;
                var span0 = s0.AsSpan(0, count);
                // foreach is faster than for loop & unroll
                foreach (ref var c0 in span0) action(ref c0); 
            } while (join.Iterate());
        }
    }


    // #region Showcase
    /// <summary>
    /// Executes an action for each entity that matches the query, passing an additional uniform parameter to the action.
    /// </summary>
    /// <param name="action"><see cref="RefActionU{C0,U}"/> taking references to Component Types.</param>
    /// <param name="uniform">The uniform data to pass to the action.</param>
    // /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(RefActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;

        foreach (var table in Archetypes)
        {
            var count = table.Count;

            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;
            do
            {
                var s0 = join.Select;
                var span0 = s0.AsSpan(0, count);
                // foreach is faster than for loop & unroll
                foreach (ref var c0 in span0) action(ref c0, uniform); 
            } while (join.Iterate());
        }
    }
    // #endregion Showcase


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityAction<C0> action)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var s0 = join.Select;
                var span0 = s0.AsSpan(0, count);
                
                for (var i = 0; i < count; i++) action(table[i], ref span0[i]);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(EntityActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var s0 = join.Select;
                var span0 = s0.AsSpan(0, count);
                
                for (var i = 0; i < count; i++) action(table[i], ref span0[i], uniform);
            } while (join.Iterate());
        }
    }


    /// <summary>
    /// Executes an action <em>in parallel chunks</em> for each entity that matches the query.
    /// </summary>
    /// <param name="action"><see cref="RefAction{C0}"/> taking references to Component Types.</param>
    public void Job(RefAction<C0> action)
    {
        AssertNotDisposed();


        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<Work<C0>>.Rent();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var s0 = join.Select;

                    var job = JobPool<Work<C0>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0>>.Return(jobs);
    }


    /// <summary>
    /// Executes an action <em>in parallel chunks</em> for each entity that matches the query, passing an additional uniform parameter to the action.
    /// </summary>
    /// <param name="action"><see cref="RefAction{C0}"/> taking references to Component Types.</param>
    /// <param name="uniform">The uniform data to pass to the action.</param>
    public void Job<U>(RefActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();


        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, U>>.Rent();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0>(StreamTypes);


            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var s0 = join.Select;

                    var job = JobPool<UniformWork<C0, U>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C0, U>>.Return(jobs);
    }


    /// <summary>
    /// Executes an action passing in bulk data in <see cref="Memory{T}"/> streams that match the query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Suggested uses include search algorithms with early-out, and passing bulk data into a game engine's native structures.
    /// </para>
    /// <para>
    /// <see cref="Memory{T}"/> contains a <c>Span</c> that can be used to access the data in a contiguous block of memory.
    /// </para>
    /// </remarks>
    /// <param name="action"><see cref="MemoryAction{C0}"/> action to execute.</param>
    public void Raw(MemoryAction<C0> action)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var s0 = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                
                action(mem0);
            } while (join.Iterate());
        }
    }


    /// <summary>
    /// Executes an action passing in bulk data in <see cref="Memory{T}"/> streams that match the query, and providing an additional uniform parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Suggested uses include search algorithms with early-out, and passing bulk data into a game engine's native structures.
    /// </para>
    /// <para>
    /// <see cref="Memory{T}"/> contains a <c>Span</c> that can be used to access the data in a contiguous block of memory.
    /// </para>
    /// </remarks>
    /// <param name="action"><see cref="MemoryAction{C0}"/> action to execute.</param>
    /// <param name="uniform">The uniform data to pass to the action.</param>
    public void Raw<U>(MemoryActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var s0 = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                
                action(mem0, uniform);
            } while (join.Iterate());
        }
    }

    #endregion
    
    #region Blitters

    /// <summary>
    /// Blit (write) a component value of a stream type to all entities matched by this query.
    /// </summary>
    /// <param name="value">a component value</param>
    /// <param name="target">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks </param>
    public void Blit(C0 value, Identity target = default)
    {
        //using var worldLock = World.Lock;

        var typeExpression = TypeExpression.Of<C0>(target);

        foreach (var table in Archetypes)
        {
            table.Fill(typeExpression, value);
        }
    }

    
    
    /// <summary>
    /// Blit (write) component values of a stream type to all entities matched by this query.
    /// The provided IList will be wrapped around (repeated) if there are fewer elements than Entities.
    /// </summary>
    /// <param name="values">a component value</param>
    /// <param name="target">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks </param>
    public void Blit(IList<C0> values, Identity target = default)
    {
        Debug.Assert(World.Mode == World.WorldMode.Immediate, "Can only blit into an unlocked world");

        var typeExpression = TypeExpression.Of<C0>(target);

        foreach (var table in Archetypes)
        {
            table.Fill(typeExpression, values);
        }
    }
    #endregion

    #region Warmup & Unroll

    /// <inheritdoc />
    public override Query<C0> Warmup()
    {
        base.Warmup();
        Job(NoOp);
        return this;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NoOp(ref C0 c0)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NoOp(ref C0 c0, int uniform)
    {
    }

    /// <inheritdoc />
    public override Query<C0> Warmup<U>()
    {
        base.Warmup<U>();
        Job(NoOp, 0);
        return this;
    }
    
    #endregion
}