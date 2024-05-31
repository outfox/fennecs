// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// Query with 4 output Stream Types, <c>C0</c> to <c>C3</c>.
/// </summary>
public class Query<C0, C1, C2, C3> : Query<C0, C1, C2> where C3 : notnull where C2 : notnull where C1 : notnull where C0 : notnull
{
    #region Internals

    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    { }

    #endregion


    #region Runners

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(RefAction<C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                var span2 = s2.Span;
                var span3 = s3.Span;

                Unroll8(span0, span1, span2, span3, action);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(RefActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        using var worldLock = World.Lock();
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                var span2 = s2.Span;
                var span3 = s3.Span;

                Unroll8U(span0, span1, span2, span3, action, uniform);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityAction<C0, C1, C2, C3> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
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
    public void For<U>(EntityActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        using var worldLock = World.Lock();
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;
                var span2 = s2.Span;
                var span3 = s3.Span;

                for (var i = 0; i < count; i++) action(table[i], ref span0[i], ref span1[i], ref span2[i], ref span3[i], uniform);
            } while (join.Iterate());
        }
    }


    /// <inheritdoc cref="Query{C0}.Job"/>
    public void Job(RefAction<C0, C1, C2, C3> action)
    {
        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock();
        using var jobs = PooledList<Work<C0, C1, C2, C3>>.Rent();
        Countdown.Reset();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
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


    /// <inheritdoc cref="Query{C0}.Job{U}"/>
    public void Job<U>(RefActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        var chunkSize = Math.Max(1, Count / Concurrency);

        using var worldLock = World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, C1, C2, C3, U>>.Rent();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
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

                    var job = JobPool<UniformWork<C0, C1, C2, C3, U>>.Rent();
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

        JobPool<UniformWork<C0, C1, C2, C3, U>>.Return(jobs);
    }


    /// <inheritdoc cref="Query{C0}.Raw"/>
    public void Raw(MemoryAction<C0, C1, C2, C3> action)
    {

        using var worldLock = World.Lock();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
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


    /// <inheritdoc cref="Query{C0}.Raw{U}"/>
    public void Raw<U>(MemoryActionU<C0, C1, C2, C3, U> action, U uniform)
    {

        using var worldLock = World.Lock();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3>(StreamTypes);
            if (join.Empty) continue;

            var count = table.Count;
            do
            {
                var (s0, s1, s2, s3) = join.Select;
                var mem0 = s0.AsMemory(0, count);
                var mem1 = s1.AsMemory(0, count);
                var mem2 = s2.AsMemory(0, count);
                var mem3 = s3.AsMemory(0, count);

                action(mem0, mem1, mem2, mem3, uniform);
            } while (join.Iterate());
        }
    }

    #endregion

    #region Blitters

    /// <inheritdoc cref="Query{C0}.Blit(C0,fennecs.Identity)"/>
    public void Blit(C3 value, Identity target)
    {
        using var worldLock = World.Lock();

        var typeExpression = TypeExpression.Of<C3>(target);

        foreach (var table in Archetypes)
        {
            table.Fill(typeExpression, value);
        }
    }

    /// <inheritdoc cref="Blit(C3, fennecs.Identity)"/>
    public void Blit(C3 value) => Blit(value, Match.Plain);


    #endregion


    #region Warmup & Unroll

    /// <inheritdoc />
    public override Query<C0, C1, C2, C3> Warmup()
    {
        base.Warmup();
        Job(NoOp);
        Job(NoOp, 0);

        C0 c0 = default!;
        C1 c1 = default!;
        C2 c2 = default!;
        C3 c3 = default!;
        NoOp(ref c0, ref c1, ref c2, ref c3);
        NoOp(ref c0, ref c1, ref c2, ref c3, 0);

        return this;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NoOp(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3)
    { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NoOp(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, int uniform)
    { }

    private static void Unroll8(Span<C0> span0, Span<C1> span1, Span<C2> span2, Span<C3> span3, RefAction<C0, C1, C2, C3> action)
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

    private static void Unroll8U<U>(Span<C0> span0, Span<C1> span1, Span<C2> span2, Span<C3> span3, RefActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], uniform);
            action(ref span0[i + 1], ref span1[i + 1], ref span2[i + 1], ref span3[i + 1], uniform);
            action(ref span0[i + 2], ref span1[i + 2], ref span2[i + 2], ref span3[i + 2], uniform);
            action(ref span0[i + 3], ref span1[i + 3], ref span2[i + 3], ref span3[i + 3], uniform);

            action(ref span0[i + 4], ref span1[i + 4], ref span2[i + 4], ref span3[i + 4], uniform);
            action(ref span0[i + 5], ref span1[i + 5], ref span2[i + 5], ref span3[i + 5], uniform);
            action(ref span0[i + 6], ref span1[i + 6], ref span2[i + 6], ref span3[i + 6], uniform);
            action(ref span0[i + 7], ref span1[i + 7], ref span2[i + 7], ref span3[i + 7], uniform);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], uniform);
        }
    }

    #endregion
}
