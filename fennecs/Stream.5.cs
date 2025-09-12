using System.Collections;
using System.Collections.Immutable;
using fennecs.pools;

namespace fennecs;

/// <inheritdoc cref="Stream{C0}"/>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
/// <typeparam name="C2">stream type</typeparam>
/// <typeparam name="C3">stream type</typeparam>
/// <typeparam name="C4">stream type</typeparam>
public readonly record struct Stream<C0, C1, C2, C3, C4>
    : IEnumerable<(Entity, C0, C1, C2, C3, C4)>
    where C0 : notnull
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
    where C4 : notnull
{
    #region Stream Fields

    private readonly ImmutableArray<TypeExpression> _streamTypes;

    /// <summary>
    /// Archetypes, or Archetypes that match the Stream's Subset and Exclude filters.
    /// </summary>
    private SortedSet<Archetype> Filtered =>
        Subset.IsEmpty && Exclude.IsEmpty
            ? Archetypes
            : new SortedSet<Archetype>(Archetypes.Where(InclusionPredicate));

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
    /// Constructs a Stream of five component types.
    /// </summary>
    public Stream(Query query, Match match0, Match match1, Match match2, Match match3, Match match4)
    {
        _streamTypes =
        [
            TypeExpression.Of<C0>(match0),
            TypeExpression.Of<C1>(match1),
            TypeExpression.Of<C2>(match2),
            TypeExpression.Of<C3>(match3),
            TypeExpression.Of<C4>(match4)
        ];
        Query = query;
    }

    /// <summary>
    /// The number of threads this Stream uses for parallel processing.
    /// </summary>
    private static int Concurrency => Math.Max(1, Environment.ProcessorCount - 2);

    #endregion

    #region Filter State

    /// <summary>Filter for component C0.</summary>
    public ComponentFilter<C0> Filter0 { private get; init; } = (in C0 _) => true;

    /// <summary>Filter for component C1.</summary>
    public ComponentFilter<C1> Filter1 { private get; init; } = (in C1 _) => true;

    /// <summary>Filter for component C2.</summary>
    public ComponentFilter<C2> Filter2 { private get; init; } = (in C2 _) => true;

    /// <summary>Filter for component C3.</summary>
    public ComponentFilter<C3> Filter3 { private get; init; } = (in C3 _) => true;

    /// <summary>Filter for component C4.</summary>
    public ComponentFilter<C4> Filter4 { private get; init; } = (in C4 _) => true;

    private bool Pass(in C0 c0, in C1 c1, in C2 c2, in C3 c3, in C4 c4) =>
        Filter0(c0) && Filter1(c1) && Filter2(c2) && Filter3(c3) && Filter4(c4);

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C0</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2, C3, C4> Where(ComponentFilter<C0> filter0) => this with {Filter0 = filter0};

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C1</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2, C3, C4> Where(ComponentFilter<C1> filter1) => this with {Filter1 = filter1};

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C2</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2, C3, C4> Where(ComponentFilter<C2> filter2) => this with {Filter2 = filter2};

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C3</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2, C3, C4> Where(ComponentFilter<C3> filter3) => this with {Filter3 = filter3};

    /// <summary>
    /// Creates a new Stream with the same Query and Filters, but replacing the 
    /// filter for Component <c>C4</c> with the provided predicate. 
    /// </summary>
    public Stream<C0, C1, C2, C3, C4> Where(ComponentFilter<C4> filter4) => this with {Filter4 = filter4};

    #endregion

    #region Stream.For

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(ComponentAction<C0, C1, C2, C3, C4> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                Loop(s0.Span, s1.Span, s2.Span, s3.Span, s4.Span, action);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(U uniform, UniformComponentAction<U, C0, C1, C2, C3, C4> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                LoopUniform(s0.Span, s1.Span, s2.Span, s3.Span, s4.Span, action, uniform);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityComponentAction<C0, C1, C2, C3, C4> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                LoopEntity(table, s0.Span, s1.Span, s2.Span, s3.Span, s4.Span, action);
            } while (join.Iterate());
        }
    }

    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(U uniform, UniformEntityComponentAction<U, C0, C1, C2, C3, C4> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                LoopUniformEntity(table, s0.Span, s1.Span, s2.Span, s3.Span, s4.Span, action, uniform);
            } while (join.Iterate());
        }
    }

    #endregion

    #region Stream.Job

    /// <inheritdoc cref="Stream{C0}.Job"/>
    public void Job(ComponentAction<C0, C1, C2, C3, C4> action)
    {
        AssertNoWildcards(_streamTypes);

        using var worldLock = World.Lock();
        var chunkSize = Math.Max(1, Count / Concurrency);

        _countdown.Reset();
        using var jobs = PooledList<Work<C0, C1, C2, C3, C4>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
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

                    var (s0, s1, s2, s3, s4) = join.Select;
                    var job = JobPool<Work<C0, C1, C2, C3, C4>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Memory4 = s3.AsMemory(start, length);
                    job.Memory5 = s4.AsMemory(start, length);
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

        JobPool<Work<C0, C1, C2, C3, C4>>.Return(jobs);
    }

    /// <inheritdoc cref="Stream{C0}.Job{U}"/>
    public void Job<U>(U uniform, UniformComponentAction<U, C0, C1, C2, C3, C4> action)
    {
        AssertNoWildcards(_streamTypes);

        using var worldLock = World.Lock();
        var chunkSize = Math.Max(1, Count / Concurrency);

        _countdown.Reset();
        using var jobs = PooledList<UniformWork<U, C0, C1, C2, C3, C4>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
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

                    var (s0, s1, s2, s3, s4) = join.Select;
                    var job = JobPool<UniformWork<U, C0, C1, C2, C3, C4>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Memory4 = s3.AsMemory(start, length);
                    job.Memory5 = s4.AsMemory(start, length);
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

        JobPool<UniformWork<U, C0, C1, C2, C3, C4>>.Return(jobs);
    }

    #endregion

    #region Stream.Raw

    /// <inheritdoc cref="Stream{C0}.Raw"/>
    public void Raw(MemoryAction<C0, C1, C2, C3, C4> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var count = table.Count;

            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                action(
                    s0.AsMemory(0, count),
                    s1.AsMemory(0, count),
                    s2.AsMemory(0, count),
                    s3.AsMemory(0, count),
                    s4.AsMemory(0, count));
            } while (join.Iterate());
        }
    }

    /// <inheritdoc cref="Stream{C0}.Raw{U}"/>
    public void Raw<U>(U uniform, MemoryUniformAction<U, C0, C1, C2, C3, C4> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var count = table.Count;

            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                action(
                    uniform,
                    s0.AsMemory(0, count),
                    s1.AsMemory(0, count),
                    s2.AsMemory(0, count),
                    s3.AsMemory(0, count),
                    s4.AsMemory(0, count));
            } while (join.Iterate());
        }
    }

    #endregion

    #region Blitters

    /// <summary>
    /// Blit (write) a component value of type <c>C0</c> to all entities matched by this query.
    /// </summary>
    public void Blit(C0 value, Match match = default)
    {
        var t = TypeExpression.Of<C0>(match);
        foreach (var table in Filtered) table.Fill(t, value);
    }

    /// <summary>
    /// Blit (write) a component value of type <c>C1</c> to all entities matched by this query.
    /// </summary>
    public void Blit(C1 value, Match match = default)
    {
        var t = TypeExpression.Of<C1>(match);
        foreach (var table in Filtered) table.Fill(t, value);
    }

    /// <summary>
    /// Blit (write) a component value of type <c>C2</c> to all entities matched by this query.
    /// </summary>
    public void Blit(C2 value, Match match = default)
    {
        var t = TypeExpression.Of<C2>(match);
        foreach (var table in Filtered) table.Fill(t, value);
    }

    /// <summary>
    /// Blit (write) a component value of type <c>C3</c> to all entities matched by this query.
    /// </summary>
    public void Blit(C3 value, Match match = default)
    {
        var t = TypeExpression.Of<C3>(match);
        foreach (var table in Filtered) table.Fill(t, value);
    }

    /// <summary>
    /// Blit (write) a component value of type <c>C4</c> to all entities matched by this query.
    /// </summary>
    public void Blit(C4 value, Match match = default)
    {
        var t = TypeExpression.Of<C4>(match);
        foreach (var table in Filtered) table.Fill(t, value);
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<(Entity, C0, C1, C2, C3, C4)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                for (var i = 0; i < table.Count; i++)
                {
                    yield return (table[i], s0[i], s1[i], s2[i], s3[i], s4[i]);
                    if (table.Version != snapshot)
                        throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Action Loops

    private void Loop(Span<C0> s0, Span<C1> s1, Span<C2> s2, Span<C3> s3, Span<C4> s4,
        ComponentAction<C0, C1, C2, C3, C4> action)
    {
        for (int i = 0; i < s0.Length; i++)
        {
            if (!Pass(in s0[i], in s1[i], in s2[i], in s3[i], in s4[i])) continue;
            action(ref s0[i], ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
        }
    }

    private void LoopEntity(Archetype table, Span<C0> s0, Span<C1> s1, Span<C2> s2, Span<C3> s3, Span<C4> s4,
        EntityComponentAction<C0, C1, C2, C3, C4> action)
    {
        for (int i = 0; i < s0.Length; i++)
        {
            if (!Pass(in s0[i], in s1[i], in s2[i], in s3[i], in s4[i])) continue;
            action(table[i], ref s0[i], ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
        }
    }

    private void LoopUniform<U>(Span<C0> s0, Span<C1> s1, Span<C2> s2, Span<C3> s3, Span<C4> s4,
        UniformComponentAction<U, C0, C1, C2, C3, C4> action, U uniform)
    {
        for (int i = 0; i < s0.Length; i++)
        {
            if (!Pass(in s0[i], in s1[i], in s2[i], in s3[i], in s4[i])) continue;
            action(uniform, ref s0[i], ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
        }
    }

    private void LoopUniformEntity<U>(Archetype table, Span<C0> s0, Span<C1> s1, Span<C2> s2, Span<C3> s3, Span<C4> s4,
        UniformEntityComponentAction<U, C0, C1, C2, C3, C4> action, U uniform)
    {
        for (int i = 0; i < s0.Length; i++)
        {
            if (!Pass(in s0[i], in s1[i], in s2[i], in s3[i], in s4[i])) continue;
            action(uniform, table[i], ref s0[i], ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
        }
    }

    #endregion

    #region Assertions

    private static void AssertNoWildcards(ImmutableArray<TypeExpression> streamTypes)
    {
        if (streamTypes.Any(t => t.isWildcard))
            throw new InvalidOperationException(
                $"Cannot run this operation on wildcard Stream Types (write destination Aliasing). {streamTypes}");
    }

    #endregion
}