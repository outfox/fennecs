using System.Collections;
using System.Collections.Immutable;
using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <inheritdoc cref="Stream{C0}"/>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
public readonly record struct Stream<C0, C1> : IEnumerable<(Entity, C0, C1)>, IBatchBegin
    where C0 : notnull
    where C1 : notnull
{
    #region Stream Fields
    private readonly ImmutableArray<TypeExpression> _streamTypes;

    /// <summary>
    /// Archetypes, or Archetypes that match the Stream's Subset and Exclude filters.
    /// </summary>
    private SortedSet<Archetype> Filtered => Subset.IsEmpty && Exclude.IsEmpty
        ? Archetypes
        : new SortedSet<Archetype>(Archetypes.Where(InclusionPredicate)); //TODO: Create immutable set?

    private bool InclusionPredicate(Archetype candidate) => (Subset.IsEmpty || candidate.MatchSignature.Matches(Subset)) && !candidate.MatchSignature.Matches(Exclude);

    /// <summary>
    /// Creates a builder for a Batch Operation on the Stream's underlying Query.
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
    private SortedSet<Archetype> Archetypes => Query.Archetypes;

    /// <summary>
    /// The World this Stream is associated with.
    /// </summary>
    private World World => Query.World;

    /// <summary>
    /// The Query this Stream is associated with.
    /// Can be re-initialized via the with keyword.
    /// </summary>
    public Query Query { get; }

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
    private readonly CountdownEvent _countdown = new(initialCount: 1);

    /// <inheritdoc cref="Stream{C0}"/>
    /// <typeparam name="C0">stream type</typeparam>
    /// <typeparam name="C1">stream type</typeparam>
    public Stream(Query Query, Match match0, Match match1)
    {
        _streamTypes = [TypeExpression.Of<C0>(match0), TypeExpression.Of<C1>(match1)];
        this.Query = Query;
    }

    /// <summary>   
    ///     The number of threads this Stream uses for parallel processing.
    /// </summary>
    private static int Concurrency => Math.Max(1, Environment.ProcessorCount - 2);

    #endregion
    
    #region Filter State
    
    /// <summary>
    /// Filter for component 0. Return true to include the entity in the Stream, false to skip it.
    /// </summary>
    public ComponentFilter<C0>? F0 { private get; init; }

    /// <summary>
    /// Filter for component 0. Return true to include the entity in the Stream, false to skip it.
    /// </summary>
    public ComponentFilter<C1>? F1 { private get; init; }
    
    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the filter for Component <c>C0</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1> Where(ComponentFilter<C0>? f0)
    {
        return this with
        {
            F0 = f0,
        };
    }

    
    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the filter for Component <c>C1</c> with the provided predicate.
    /// </summary>
    public Stream<C0, C1> Where(ComponentFilter<C1>? f1)
    {
        return this with
        {
            F1 = f1,
        };
    }
    #endregion

    #region Stream.For

    private void FastFor(ComponentAction<C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1) = join.Select;
                LoopUnroll8(s0, s1, action);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(ComponentAction<C0, C1> action)
    {
        using var worldLock = World.Lock();

        if (F0 == null && F1 == null)
        {
            FastFor(action);
            return;
        }
        
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1) = join.Select;
                LoopFilteredUnroll8(s0, s1, action);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(U uniform, UniformComponentAction<U, C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;

                LoopUnroll8U(span0, span1, action, uniform);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityComponentAction<C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                for (var i = 0; i < count; i++) action(table[i], ref span0[i], ref span1[i]);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(U uniform, UniformEntityComponentAction<U, C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                for (var i = 0; i < count; i++) action(uniform, table[i], ref span0[i], ref span1[i]);
            } while (join.Iterate());
        }
    }

    #endregion

    #region Stream.Job

    /// <inheritdoc cref="Stream{C0}.Job"/>
    public void Job(ComponentAction<C0, C1> action)
    {
        AssertNoWildcards(_streamTypes);
        
        using var worldLock = World.Lock();
        var chunkSize = Math.Max(1, Count / Concurrency);

        _countdown.Reset();

        using var jobs = PooledList<Work<C0, C1>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    _countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1) = join.Select;

                    var job = JobPool<Work<C0, C1>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = _countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<Work<C0, C1>>.Return(jobs);
    }


    /// <inheritdoc cref="Stream{C0}.Job{U}"/>
    public void Job<U>(U uniform, UniformComponentAction<U, C0, C1> action)
    {
        AssertNoWildcards(_streamTypes);

        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock();
        _countdown.Reset();

        using var jobs = PooledList<UniformWork<U, C0, C1>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    _countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1) = join.Select;

                    var job = JobPool<UniformWork<U, C0, C1>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.CountDown = _countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<UniformWork<U, C0, C1>>.Return(jobs);
    }

    #endregion

    #region Stream.Raw

    /// <inheritdoc cref="Stream{C0}.Raw"/>
    public void Raw(MemoryAction<C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);

                action(mem0, mem1);
            } while (join.Iterate());
        }
    }


    /// <inheritdoc cref="Stream{C0}.Raw{U}"/>
    public void Raw<U>(U uniform, MemoryUniformAction<U, C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);

                action(uniform, mem0, mem1);
            } while (join.Iterate());
        }
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


    /// <inheritdoc cref="Stream{C0}.Blit(C0,Match)"/>
    public void Blit(C1 value, Match match = default)
    {
        using var worldLock = World.Lock();

        var typeExpression = TypeExpression.Of<C1>(match);

        foreach (var table in Filtered)
        {
            table.Fill(typeExpression, value);
        }
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<(Entity, C0, C1)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var (s0, s1) = join.Select;
                for (var index = 0; index < table.Count; index++)
                {
                    yield return (table[index], s0[index], s1[index]);
                    if (table.Version != snapshot) throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }


    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    #endregion
    
    #region Action Loops

    // ReSharper disable once CognitiveComplexity
    private void LoopFilteredUnroll8(Span<C0> span0, Span<C1> span1, ComponentAction<C0, C1> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            if ((F0 == null || F0(span0[i])) && 
                (F1 == null || F1(span1[i]))) 
                action(ref span0[i], ref span1[i]);

            if ((F0 == null || F0(span0[i + 1])) && 
                (F1 == null || F1(span1[i + 1]))) 
                action(ref span0[i + 1], ref span1[i + 1]);   
            
            if ((F0 == null || F0(span0[i + 2])) && 
                (F1 == null || F1(span1[i + 2]))) 
                action(ref span0[i + 2], ref span1[i + 2]);
            
            if ((F0 == null || F0(span0[i + 3])) && 
                (F1 == null || F1(span1[i + 3]))) 
                action(ref span0[i + 3], ref span1[i + 3]);
            
            if ((F0 == null || F0(span0[i + 4])) && 
                (F1 == null || F1(span1[i + 4]))) 
                action(ref span0[i + 4], ref span1[i + 4]);
            
            if ((F0 == null || F0(span0[i + 5])) && 
                (F1 == null || F1(span1[i + 5]))) 
                action(ref span0[i + 5], ref span1[i + 5]);
            
            if ((F0 == null || F0(span0[i + 6])) && 
                (F1 == null || F1(span1[i + 6]))) 
                action(ref span0[i + 6], ref span1[i + 6]);
            
            if ((F0 == null || F0(span0[i + 7])) && 
                (F1 == null || F1(span1[i + 7]))) 
                action(ref span0[i + 7], ref span1[i + 7]);
            
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            if (F0 != null && !F0(span0[i])) continue;
            if (F1 != null && !F1(span1[i])) continue;
            action(ref span0[i], ref span1[i]);
        }
    }

    private static void LoopUnroll8<U0, U1>(Span<U0> span0, Span<U1> span1, ComponentAction<U0, U1> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(ref span0[i], ref span1[i]);
            action(ref span0[i + 1], ref span1[i + 1]);
            action(ref span0[i + 2], ref span1[i + 2]);
            action(ref span0[i + 3], ref span1[i + 3]);

            action(ref span0[i + 4], ref span1[i + 4]);
            action(ref span0[i + 5], ref span1[i + 5]);
            action(ref span0[i + 6], ref span1[i + 6]);
            action(ref span0[i + 7], ref span1[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(ref span0[i], ref span1[i]);
        }
    }

    private static void LoopUnroll8U<U>(Span<C0> span0, Span<C1> span1, UniformComponentAction<U, C0, C1> action, U uniform)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(uniform, ref span0[i], ref span1[i]);
            action(uniform, ref span0[i + 1], ref span1[i + 1]);
            action(uniform, ref span0[i + 2], ref span1[i + 2]);
            action(uniform, ref span0[i + 3], ref span1[i + 3]);

            action(uniform, ref span0[i + 4], ref span1[i + 4]);
            action(uniform, ref span0[i + 5], ref span1[i + 5]);
            action(uniform, ref span0[i + 6], ref span1[i + 6]);
            action(uniform, ref span0[i + 7], ref span1[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(uniform, ref span0[i], ref span1[i]);
        }
    }
    
    #endregion
    
    #region Assertions
    /// <summary>
    /// Throws if the query has any Wildcards.
    /// </summary>
    private static void AssertNoWildcards(ImmutableArray<TypeExpression> streamTypes)
    {
        if (streamTypes.Any(t => t.isWildcard)) throw new InvalidOperationException($"Cannot run a this operation on wildcard Stream Types (write destination Aliasing). {streamTypes}");
    }
    #endregion
}
