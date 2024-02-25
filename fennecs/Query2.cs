// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

/// <summary>
/// Query with 2 output Stream Types, <c>C0</c> and <c>C1</c>.
/// </summary>
public class Query<C0, C1> : Query<C0>
{
    #region Internals

    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }

    #endregion


    #region Runners

    
    
    public void ForSpan(SpanAction<C0, C1> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            do
            {
                var (s0, s1) = join.Select;
                
                var span0 = s0.AsSpan(0, count);
                var span1 = s1.AsSpan(0, count);
                action(span0, span1);
            } while (join.Permutate);
        }
    }


    public void ForSpan<U>(SpanActionU<C0, C1, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);

            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.AsSpan(0, count);
                var span1 = s1.AsSpan(0, count);
                action(span0, span1, uniform);
            } while (join.Permutate);
        }
    }


    public void ForEach(RefAction<C0, C1> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.AsSpan(0, table.Count);
                var span1 = s1.AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i]);
            } while (join.Permutate);
        }
    }


    public void ForEach<U>(RefActionU<C0, C1, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.AsSpan(0, table.Count);
                var span1 = s1.AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], uniform);
            } while (join.Permutate);
        }
    }


    public void Job(RefAction<C0, C1> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<Work<C0, C1>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            
            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1) = join.Select;
                    var job = JobPool<Work<C0, C1>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Permutate);
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0, C1>>.Return(jobs);
    }


    public void Job<U>(RefActionU<C0, C1, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, C1, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            
            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var (s0, s1) = join.Select;
                    
                    var job = JobPool<UniformWork<C0, C1, U>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (join.Permutate);
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C0, C1, U>>.Return(jobs);
    }


    public void Raw(MemoryAction<C0, C1> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            do
            {
                var (s0, s1) = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                var mem1 = s1.AsMemory(0, table.Count);
                action(mem0, mem1);
            } while (join.Permutate);
        }
    }


    public void Raw<U>(MemoryActionU<C0, C1, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var join = table.CrossJoin<C0, C1>(StreamTypes);
            do
            {
                var (s0, s1) = join.Select;
                var mem0 = s0.AsMemory(0, table.Count); 
                var mem1 = s1.AsMemory(0, table.Count);
                action(mem0, mem1, uniform);
            } while (join.Permutate);
        }
    }

    #endregion
}