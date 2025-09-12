using System.Collections;
using System.Collections.Immutable;

using fennecs.pools;

namespace fennecs;

/// <inheritdoc cref="Stream{C0}"/>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
/// <typeparam name="C2">stream type</typeparam>
public readonly record struct Stream<C0, C1, C2>
    : IEnumerable<(Entity, C0, C1, C2)>
    where C0 : notnull
    where C1 : notnull
    where C2 : notnull
{
    #region Stream Fields

    private readonly ImmutableArray<TypeExpression> _streamTypes;

    /// <summary>
    /// Archetypes, or Archetypes that match the Stream's Subset and Exclude filters.
    /// </summary>
    private SortedSet<Archetype> Filtered =>
        Subset.IsEmpty && Exclude.IsEmpty
            ? Archetypes
            : new SortedSet<Archetype>(
                Archetypes.Where(InclusionPredicate));

    private bool InclusionPredicate(Archetype candidate) =>
        (Subset.IsEmpty || candidate.MatchSignature.Matches(Subset)) &&
        !candidate.MatchSignature.Matches(Exclude);

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
    /// Subset Stream Filter - if not empty, only entities with these components 
    /// will be included in the Stream. 
    /// </summary>
    public ImmutableSortedSet<Comp> Subset { get; init; } = [];

    /// <summary>
    /// Exclude Stream Filter - any entities with these components 
    /// will be excluded from the Stream. (none if empty)
    /// </summary>
    public ImmutableSortedSet<Comp> Exclude { get; init; } = [];

    /// <summary>
    /// Countdown event for parallel runners.
    /// </summary>
    private readonly CountdownEvent _countdown = new(initialCount: 1);

    /// <summary>   
    /// Constructs a Stream of three component types.
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="match0">Match expression for C0</param>
    /// <param name="match1">Match expression for C1</param>
    /// <param name="match2">Match expression for C2</param>
    public Stream(Query query, Match match0, Match match1, Match match2)
    {
        _streamTypes =
        [
            TypeExpression.Of<C0>(match0),
            TypeExpression.Of<C1>(match1),
            TypeExpression.Of<C2>(match2)
        ];
        Query = query;
    }

    /// <summary>   
    /// The number of threads this Stream uses for parallel processing.
    /// </summary>
    private static int Concurrency => Math.Max(1, Environment.ProcessorCount - 2);

    #endregion

    #region Filter State

    /// <summary>
    /// Filter for component C0. Return true to include the entity in the Stream,
    /// false to skip it.
    /// </summary>
    public ComponentFilter<C0> Filter0 { private get; init; } = (in C0 _) => true;

    /// <summary>
    /// Filter for component C1. Return true to include the entity in the Stream,
    /// false to skip it.
    /// </summary>
    public ComponentFilter<C1> Filter1 { private get; init; } = (in C1 _) => true;

    /// <summary>
    /// Filter for component C2. Return true to include the entity in the Stream,
    /// false to skip it.
    /// </summary>
    public ComponentFilter<C2> Filter2 { private get; init; } = (in C2 _) => true;

    private bool Pass(in C0 c0, in C1 c1, in C2 c2) => Filter0(c0) && Filter1(c1) && Filter2(c2);

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C0</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2> Where(ComponentFilter<C0> filter0) =>
        this with {Filter0 = filter0};

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C1</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2> Where(ComponentFilter<C1> filter1) =>
        this with {Filter1 = filter1};

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C2</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2> Where(ComponentFilter<C2> filter2) =>
        this with {Filter2 = filter2};

    #endregion

    #region Stream.For

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(ComponentAction<C0, C1, C2> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2) = join.Select;
                Loop(s0.Span, s1.Span, s2.Span, action);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(U uniform, UniformComponentAction<U, C0, C1, C2> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2) = join.Select;
                LoopUniform(s0.Span, s1.Span, s2.Span, action, uniform);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityComponentAction<C0, C1, C2> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2) = join.Select;
                LoopEntity(table, s0.Span, s1.Span, s2.Span, action);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(U uniform, UniformEntityComponentAction<U, C0, C1, C2> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2) = join.Select;
                LoopUniformEntity(table, s0.Span, s1.Span, s2.Span, action, uniform);
            } while (join.Iterate());
        }
    }

    #endregion

    #region Stream.Job

    /// <inheritdoc cref="Stream{C0}.Job"/>
    public void Job(ComponentAction<C0, C1, C2> action)
    {
        AssertNoWildcards(_streamTypes);

        using var worldLock = World.Lock();
        var chunkSize = Math.Max(1, Count / Concurrency);

        _countdown.Reset();
        using var jobs = PooledList<Work<C0, C1, C2>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var count = table.Count;
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    _countdown.AddCount();
                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1, s2) = join.Select;
                    var job = JobPool<Work<C0, C1, C2>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Action = action;
                    job.Pass = Pass;
                    job.CountDown = _countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<Work<C0, C1, C2>>.Return(jobs);
    }

    /// <inheritdoc cref="Stream{C0}.Job{U}"/>
    public void Job<U>(U uniform, UniformComponentAction<U, C0, C1, C2> action)
    {
        AssertNoWildcards(_streamTypes);

        using var worldLock = World.Lock();
        var chunkSize = Math.Max(1, Count / Concurrency);

        _countdown.Reset();
        using var jobs = PooledList<UniformWork<U, C0, C1, C2>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var count = table.Count;
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    _countdown.AddCount();
                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1, s2) = join.Select;
                    var job = JobPool<UniformWork<U, C0, C1, C2>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.Pass = Pass;
                    job.CountDown = _countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<UniformWork<U, C0, C1, C2>>.Return(jobs);
    }

    #endregion

    #region Stream.Raw

    /// <inheritdoc cref="Stream{C0}.Raw"/>
    public void Raw(MemoryAction<C0, C1, C2> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var count = table.Count;

            do
            {
                var (s0, s1, s2) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);
                var mem2 = s2.AsMemory(0, count);

                action(mem0, mem1, mem2);
            } while (join.Iterate());
        }
    }

    /// <inheritdoc cref="Stream{C0}.Raw{U}"/>
    public void Raw<U>(U uniform, MemoryUniformAction<U, C0, C1, C2> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var count = table.Count;

            do
            {
                var (s0, s1, s2) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);
                var mem2 = s2.AsMemory(0, count);

                action(uniform, mem0, mem1, mem2);
            } while (join.Iterate());
        }
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<(Entity, C0, C1, C2)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var (s0, s1, s2) = join.Select;
                for (var i = 0; i < table.Count; i++)
                {
                    yield return (table[i], s0[i], s1[i], s2[i]);
                    if (table.Version != snapshot)
                        throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
    
    #region Blitters
    /// <summary>
    /// <para>Blit (write) a component value of type <c>C0</c> to all entities matched by this query.</para>
    /// <para>🚀 Very fast!</para>
    /// </summary>
    /// <remarks>
    /// Each entity in the Query must possess the component type.
    /// Otherwise, consider using <see cref="Query.Add{T}()"/> with <see cref="Batch.AddConflict.Replace"/>. 
    /// </remarks>
    /// <param name="value">a component value</param>
    /// <param name="match">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks</param>
    public void Blit(C0 value, Match match = default)
    {
        var typeExpression = TypeExpression.Of<C0>(match);
        foreach (var table in Filtered)
            table.Fill(typeExpression, value);
    }

    /// <summary>
    /// <para>Blit (write) a component value of type <c>C1</c> to all entities matched by this query.</para>
    /// <para>🚀 Very fast!</para>
    /// </summary>
    /// <remarks>
    /// Each entity in the Query must possess the component type.
    /// Otherwise, consider using <see cref="Query.Add{T}()"/> with <see cref="Batch.AddConflict.Replace"/>. 
    /// </remarks>
    /// <param name="value">a component value</param>
    /// <param name="match">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks</param>
    public void Blit(C1 value, Match match = default)
    {
        var typeExpression = TypeExpression.Of<C1>(match);
        foreach (var table in Filtered)
            table.Fill(typeExpression, value);
    }

    /// <summary>
    /// <para>Blit (write) a component value of type <c>C2</c> to all entities matched by this query.</para>
    /// <para>🚀 Very fast!</para>
    /// </summary>
    /// <remarks>
    /// Each entity in the Query must possess the component type.
    /// Otherwise, consider using <see cref="Query.Add{T}()"/> with <see cref="Batch.AddConflict.Replace"/>. 
    /// </remarks>
    /// <param name="value">a component value</param>
    /// <param name="match">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks</param>
    public void Blit(C2 value, Match match = default)
    {
        var typeExpression = TypeExpression.Of<C2>(match);
        foreach (var table in Filtered)
            table.Fill(typeExpression, value);
    }
    #endregion

    #region Action Loops

    private void Loop(
        Span<C0> span0,
        Span<C1> span1,
        Span<C2> span2,
        ComponentAction<C0, C1, C2> action)
    {
        var length = span0.Length;
        for (var i = 0; i < length; i++)
        {
            if (!Pass(in span0[i], in span1[i], in span2[i])) continue;
            action(ref span0[i], ref span1[i], ref span2[i]);
        }
    }

    private void LoopEntity(
        Archetype table,
        Span<C0> span0,
        Span<C1> span1,
        Span<C2> span2,
        EntityComponentAction<C0, C1, C2> action)
    {
        var length = span0.Length;
        for (var i = 0; i < length; i++)
        {
            if (!Pass(in span0[i], in span1[i], in span2[i])) continue;
            action(table[i], ref span0[i], ref span1[i], ref span2[i]);
        }
    }

    private void LoopUniform<U>(
        Span<C0> span0,
        Span<C1> span1,
        Span<C2> span2,
        UniformComponentAction<U, C0, C1, C2> action,
        U uniform)
    {
        var length = span0.Length;
        for (var i = 0; i < length; i++)
        {
            if (!Pass(in span0[i], in span1[i], in span2[i])) continue;
            action(uniform, ref span0[i], ref span1[i], ref span2[i]);
        }
    }

    private void LoopUniformEntity<U>(
        Archetype table,
        Span<C0> span0,
        Span<C1> span1,
        Span<C2> span2,
        UniformEntityComponentAction<U, C0, C1, C2> action,
        U uniform)
    {
        var length = span0.Length;
        for (var i = 0; i < length; i++)
        {
            if (!Pass(in span0[i], in span1[i], in span2[i])) continue;
            action(uniform, table[i], ref span0[i], ref span1[i], ref span2[i]);
        }
    }

    #endregion

    #region Assertions

    /// <summary>
    /// Throws if the query has any Wildcards.
    /// </summary>
    private static void AssertNoWildcards(ImmutableArray<TypeExpression> streamTypes)
    {
        if (streamTypes.Any(t => t.isWildcard))
            throw new InvalidOperationException(
                $"Cannot run this operation on wildcard Stream Types (write destination Aliasing). {streamTypes}");
    }

    #endregion
}