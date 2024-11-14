using System.Collections;
using System.Collections.Immutable;
using fennecs.pools;

namespace fennecs;

/// <inheritdoc cref="Stream{C0}"/>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
/// <typeparam name="C2">stream type</typeparam>
/// <typeparam name="C3">stream type</typeparam>
// ReSharper disable once NotAccessedPositionalProperty.Global
public partial record Stream<C0, C1, C2, C3>(Query Query, Match Match0, Match Match1, Match Match2, Match Match3)
    : Stream<C0, C1, C2>(Query, Match0, Match1, Match2), IEnumerable<(Entity, C0, C1, C2, C3)>
    where C0 : notnull
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
{
    private readonly ImmutableArray<TypeExpression> _streamTypes = [TypeExpression.Of<C0>(Match0), TypeExpression.Of<C1>(Match1), TypeExpression.Of<C2>(Match2), TypeExpression.Of<C3>(Match3)];


    #region Stream.For

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(ComponentAction<C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                Unroll8(s0, s1, s2, s3, action);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(U uniform, UniformComponentAction<U, C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                var span2 = s2.Span;
                var span3 = s3.Span;

                Unroll8U(uniform, span0, span1, span2, span3, action);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityComponentAction<C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                var span2 = s2.Span;
                var span3 = s3.Span;
                for (var i = 0; i < count; i++) action(table[i], ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(U uniform, UniformEntityComponentAction<U, C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                var span2 = s2.Span;
                var span3 = s3.Span;
                for (var i = 0; i < count; i++) action(uniform, table[i], ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
            } while (join.Iterate());
        }
    }

    #endregion

    #region Stream.Job

    /// <inheritdoc cref="Stream{C0}.Job"/>
    public void Job(ComponentAction<C0, C1, C2, C3> action)
    {
        AssertNoWildcards();

        using var worldLock = World.Lock();
        var chunkSize = Math.Max(1, Count / Concurrency);

        Countdown.Reset();

        using var jobs = PooledList<Work<C0, C1, C2, C3>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
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

                    var (s0, s1, s2, s3) = join.Select;

                    var job = JobPool<Work<C0, C1, C2, C3>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Memory4 = s3.AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0, C1, C2, C3>>.Return(jobs);
    }


    /// <inheritdoc cref="Stream{C0}.Job{U}"/>
    public void Job<U>(U uniform, UniformComponentAction<U, C0, C1, C2, C3> action)
    {
        AssertNoWildcards();

        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<U, C0, C1, C2, C3>>.Rent();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
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

                    var (s0, s1, s2, s3) = join.Select;

                    var job = JobPool<UniformWork<U, C0, C1, C2, C3>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Memory4 = s3.AsMemory(start, length);
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

        JobPool<UniformWork<U, C0, C1, C2, C3>>.Return(jobs);
    }

    #endregion

    #region Stream.Raw

    /// <inheritdoc cref="Stream{C0}.Raw"/>
    public void Raw(MemoryAction<C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);
                var mem2 = s2.AsMemory(0, count);
                var mem3 = s3.AsMemory(0, count);

                action(mem0, mem1, mem2, mem3);
            } while (join.Iterate());
        }
    }


    /// <inheritdoc cref="Stream{C0}.Raw{U}"/>
    public void Raw<U>(U uniform, MemoryUniformAction<U, C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);
                var mem2 = s2.AsMemory(0, count);
                var mem3 = s3.AsMemory(0, count);

                action(uniform, mem0, mem1, mem2, mem3);
            } while (join.Iterate());
        }
    }

    #endregion


    #region Blitters

    /// <inheritdoc cref="Stream{C0}.Blit(C0,Match)"/>
    public void Blit(C3 value, Match match = default)
    {
        using var worldLock = World.Lock();

        var typeExpression = TypeExpression.Of<C3>(match);

        foreach (var table in Filtered)
        {
            table.Fill(typeExpression, value);
        }
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc />
    public new IEnumerator<(Entity, C0, C1, C2, C3)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                for (var index = 0; index < table.Count; index++)
                {
                    yield return (table[index], s0[index], s1[index], s2[index], s3[index]);
                    if (table.Version != snapshot) throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    private static void Unroll8(Span<C0> span0, Span<C1> span1, Span<C2> span2, Span<C3> span3, ComponentAction<C0, C1, C2, C3> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
            action(ref span0[i + 1], ref span1[i + 1], ref span2[i + 1], ref span3[i + 1]);
            action(ref span0[i + 2], ref span1[i + 2], ref span2[i + 2], ref span3[i + 2]);
            action(ref span0[i + 3], ref span1[i + 3], ref span2[i + 3], ref span3[i + 3]);

            action(ref span0[i + 4], ref span1[i + 4], ref span2[i + 4], ref span3[i + 4]);
            action(ref span0[i + 5], ref span1[i + 5], ref span2[i + 5], ref span3[i + 5]);
            action(ref span0[i + 6], ref span1[i + 6], ref span2[i + 6], ref span3[i + 6]);
            action(ref span0[i + 7], ref span1[i + 7], ref span2[i + 7], ref span3[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
        }
    }

    private static void Unroll8U<U>(U uniform, Span<C0> span0, Span<C1> span1, Span<C2> span2, Span<C3> span3, UniformComponentAction<U, C0, C1, C2, C3> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(uniform, ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
            action(uniform, ref span0[i + 1], ref span1[i + 1], ref span2[i + 1], ref span3[i + 1]);
            action(uniform, ref span0[i + 2], ref span1[i + 2], ref span2[i + 2], ref span3[i + 2]);
            action(uniform, ref span0[i + 3], ref span1[i + 3], ref span2[i + 3], ref span3[i + 3]);

            action(uniform, ref span0[i + 4], ref span1[i + 4], ref span2[i + 4], ref span3[i + 4]);
            action(uniform, ref span0[i + 5], ref span1[i + 5], ref span2[i + 5], ref span3[i + 5]);
            action(uniform, ref span0[i + 6], ref span1[i + 6], ref span2[i + 6], ref span3[i + 6]);
            action(uniform, ref span0[i + 7], ref span1[i + 7], ref span2[i + 7], ref span3[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(uniform, ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
        }
    }
}
