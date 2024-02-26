// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

/// <summary>
/// Query with 5 output Stream Types, <c>C0</c> to <c>C4</c>.
/// </summary>
public class Query<C0, C1, C2, C3, C4> : Query<C0, C1, C2, C3>
{
    #region Internals

    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }

    #endregion


    #region Runners

    public void ForSpan(SpanAction<C0, C1, C2, C3, C4> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                var span0 = s0.AsSpan(0, count);
                var span1 = s1.AsSpan(0, count);
                var span2 = s2.AsSpan(0, count);
                var span3 = s3.AsSpan(0, count);
                var span4 = s4.AsSpan(0, count);
                action(span0, span1, span2, span3, span4);
            } while (join.Iterate());
        }
    }


    public void ForSpan<U>(SpanActionU<C0, C1, C2, C3, C4, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                var span0 = s0.AsSpan(0, count);
                var span1 = s1.AsSpan(0, count);
                var span2 = s2.AsSpan(0, count);
                var span3 = s3.AsSpan(0, count);
                var span4 = s4.AsSpan(0, count);
                action(span0, span1, span2, span3, span4, uniform);
            } while (join.Iterate());
        }
    }


    public void ForEach(RefAction<C0, C1, C2, C3, C4> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                var span0 = s0.AsSpan(0, count);
                var span1 = s1.AsSpan(0, count);
                var span2 = s2.AsSpan(0, count);
                var span3 = s3.AsSpan(0, count);
                var span4 = s4.AsSpan(0, count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], ref span4[i]);
            } while (join.Iterate());
        }
    }


    public void ForEach<U>(RefActionU<C0, C1, C2, C3, C4, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            var count = table.Count;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                var span0 = s0.AsSpan(0, count);
                var span1 = s1.AsSpan(0, count);
                var span2 = s2.AsSpan(0, count);
                var span3 = s3.AsSpan(0, count);
                var span4 = s4.AsSpan(0, count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], ref span4[i], uniform);
            } while (join.Iterate());
        }
    }


    public void Job(RefAction<C0, C1, C2, C3, C4> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<Work<C0, C1, C2, C3, C4>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

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
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Iterate());
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0, C1, C2, C3, C4>>.Return(jobs);
    }


    public void Job<U>(RefActionU<C0, C1, C2, C3, C4, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, C1, C2, C3, C4, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1, s2, s3, s4) = join.Select;

                    var job = JobPool<UniformWork<C0, C1, C2, C3, C4, U>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Memory4 = s3.AsMemory(start, length);
                    job.Memory5 = s4.AsMemory(start, length);

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

        JobPool<UniformWork<C0, C1, C2, C3, C4, U>>.Return(jobs);
    }


    public void Raw(MemoryAction<C0, C1, C2, C3, C4> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                var mem1 = s1.AsMemory(0, table.Count);
                var mem2 = s2.AsMemory(0, table.Count);
                var mem3 = s3.AsMemory(0, table.Count);
                var mem4 = s4.AsMemory(0, table.Count);

                action(mem0, mem1, mem2, mem3, mem4);
            } while (join.Iterate());
        }
    }


    public void Raw<U>(MemoryActionU<C0, C1, C2, C3, C4, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(StreamTypes);
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                var mem1 = s1.AsMemory(0, table.Count);
                var mem2 = s2.AsMemory(0, table.Count);
                var mem3 = s3.AsMemory(0, table.Count);
                var mem4 = s4.AsMemory(0, table.Count);

                action(mem0, mem1, mem2, mem3, mem4, uniform);
            } while (join.Iterate());
        }
    }

    #endregion
}