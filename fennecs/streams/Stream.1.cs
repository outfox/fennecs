using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A Stream is an accessor that allows for iteration over a Query's contents.
/// It exposes both the Runners as well as IEnumerable over a value tuple of the
/// Query's contents.
/// </summary>
/// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
// ReSharper disable once NotAccessedPositionalProperty.Global
public partial record Stream<C0>(Query Query, Match Match0) : IEnumerable<(Entity, C0)>, IBatchBegin
    where C0 : notnull
{
    private readonly ImmutableArray<TypeExpression> _streamTypes = [TypeExpression.Of<C0>(Match0)];

    /// <summary>
    /// Archetypes, or Archetypes that match the Stream's Subset and Exclude filters.
    /// </summary>
    protected HashSet<Archetype> Filtered => Subset.IsEmpty && Exclude.IsEmpty
        ? Archetypes
        : [..Archetypes.Where(a => (Subset.IsEmpty || a.Signature.Matches(Subset)) && !a.Signature.Matches(Exclude))];

    /// <summary>
    /// Creates a builder for a Batch Operation on the Stream's underyling Query.
    /// </summary>
    /// <returns>fluent builder</returns>
    public Batch Batch() => Query.Batch();

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public Batch Batch(Batch.AddConflict add) => Query.Batch(add);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public Batch Batch(Batch.RemoveConflict remove) => Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public Batch Batch(Batch.AddConflict add, Batch.RemoveConflict remove) => Query.Batch(add, remove);


    /// <summary>
    /// The number of entities that match the underlying Query.
    /// </summary>
    public int Count => Filtered.Sum(f => f.Count);


    /// <summary>
    /// The Archetypes that this Stream is iterating over.
    /// </summary>
    protected HashSet<Archetype> Archetypes => Query.Archetypes;

    /// <summary>
    /// The World this Stream is associated with.
    /// </summary>
    protected World World => Query.World;

    /// <summary>
    /// The Query this Stream is associated with.
    /// Can be re-inited via the with keyword.
    /// </summary>
    public Query Query { get; } = Query;

    /// <summary>
    /// Subset Stream Filter - if not empty, only entities with these components will be included in the Stream. 
    /// </summary>
    public ImmutableSortedSet<Comp> Subset { get; init; } = [];

    /// <summary>
    /// Exclude Stream Filter - any entities with these components will be excluded from the Stream. (none if empty)
    /// </summary>
    public ImmutableSortedSet<Comp> Exclude { get; init; } = [];

    /// <summary>
    ///     Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(initialCount: 1);

    /// <summary>   
    ///     The number of threads this Stream uses for parallel processing.
    /// </summary>
    protected static int Concurrency => Math.Max(1, Environment.ProcessorCount - 2);


    #region Stream.For

    /// <include file='../XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(ComponentAction<C0> action)
    {

        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var s0 = join.Select;
                // sic! foreach is faster here than for loop or unroll8()
                foreach (ref var c0 in s0.Span) action(ref c0);
            } while (join.Iterate());
        }
    }

    // #region Showcase
    /// <summary>
    /// Executes an action for each entity that matches the query, passing an additional uniform parameter to the action.
    /// </summary>
    /// <param name="action"><see cref="UniformComponentAction{C0,U}"/> taking references to Component Types.</param>
    /// <param name="uniform">The uniform data to pass to the action.</param>
    // /// <include file='../XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(U uniform, UniformComponentAction<U, C0> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var s0 = join.Select;
                var span0 = s0.Span;
                // foreach is faster than for loop & unroll
                foreach (ref var c0 in span0) action(uniform, ref c0);
            } while (join.Iterate());
        }
    }


    /// <include file='../XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityComponentAction<C0> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var s0 = join.Select;
                var span0 = s0.Span;
                for (var i = 0; i < count; i++) action(table[i], ref span0[i]);
            } while (join.Iterate());
        }
    }


    /// <include file='../XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(U uniform, UniformEntityComponentAction<U, C0> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var s0 = join.Select;
                var span0 = s0.Span;
                for (var i = 0; i < count; i++) action(uniform, table[i], ref span0[i]);
            } while (join.Iterate());
        }
    }

    // #endregion Showcase

    #endregion


    #region Stream.Job

    /// <summary>
    /// Executes an action <em>in parallel chunks</em> for each entity that matches the query.
    /// </summary>
    /// <param name="action"><see cref="ComponentAction{C0}"/> taking references to Component Types.</param>
    public void Job(ComponentAction<C0> action)
    {
        AssertNoWildcards();

        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C0>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
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
    /// <param name="action"><see cref="ComponentAction{C0}"/> taking references to Component Types.</param>
    /// <param name="uniform">The uniform data to pass to the action.</param>
    public void Job<U>(U uniform, UniformComponentAction<U, C0> action)
    {
        AssertNoWildcards();

        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<U, C0>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
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

                    var job = JobPool<UniformWork<U, C0>>.Rent();
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

        JobPool<UniformWork<U, C0>>.Return(jobs);
    }

    #endregion


    #region Stream.Raw

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
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
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
    public void Raw<U>(U uniform, MemoryUniformAction<U, C0> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var s0 = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                action(uniform, mem0);
            } while (join.Iterate());
        }
    }

    #endregion


    #region Assertions

    /// <summary>
    /// Throws if the query has any Wildcards.
    /// </summary>
    protected void AssertNoWildcards()
    {
        if (_streamTypes.Any(t => t.isWildcard)) throw new InvalidOperationException($"Cannot run a this operation on wildcard Stream Types (write destination Aliasing). {_streamTypes}");
    }

    #endregion


    #region Blitters

    /// <summary>
    /// <para>Blit (write) a component value of a stream type to all entities matched by this query.</para>
    /// <para>🚀 Very fast!</para>
    /// </summary>
    /// <remarks>
    /// Each entity in the Query must possess the component type.
    /// Otherwise, consider using <see cref="Query.Add{T}()"/> with <see cref="Batch.AddConflict.Replace"/>. 
    /// </remarks>
    /// <param name="value">a component value</param>
    /// <param name="match">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks </param>
    public void Blit(C0 value, Match match = default)
    {
        var typeExpression = TypeExpression.Of<C0>(match);
        foreach (var table in Filtered) table.Fill(typeExpression, value);
    }

    #endregion


    #region Query Forwarding

    /// <inheritdoc cref="fennecs.Query.Truncate"/>
    public void Truncate(int targetSize, Query.TruncateMode mode = default)
    {
        Query.Truncate(targetSize, mode);
    }

    /// <inheritdoc cref="fennecs.Query.Despawn"/>
    public void Despawn()
    {
        foreach (var archetype in Filtered) archetype.Truncate(0);
    }

    #endregion


    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<(Entity, C0)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var s0 = join.Select;
                for (var index = 0; index < table.Count; index++)
                {
                    yield return (table[index], s0.Span[index]);
                    if (table.Version != snapshot) throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion


}
