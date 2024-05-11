// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

/// <summary>
/// Query with 3 output Stream Types, <c>C0</c> to <c>C2</c>.
/// </summary>
public class Query<C0, C1, C2> : Query<C0, C1>
{
    #region Internals
    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }
    
    #endregion


    #region Runners
    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(RefAction<C0, C1, C2> action)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var span0 = s0.AsSpan(0, table.Count);
                var span1 = s1.AsSpan(0, table.Count);
                var span2 = s2.AsSpan(0, table.Count);

                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], ref span2[i]);
            } while (join.Iterate());
        }
    }
    
    
    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForU"]'/>
    public void For<U>(RefActionU<C0, C1, C2, U> action, U uniform)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var span0 = s0.AsSpan(0, table.Count);
                var span1 = s1.AsSpan(0, table.Count);
                var span2 = s2.AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], ref span2[i], uniform);
            } while (join.Iterate());
        }
    }
    
    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForE"]'/>
    public void For(EntityAction<C0, C1, C2> action)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var span0 = s0.AsSpan(0, table.Count);
                var span1 = s1.AsSpan(0, table.Count);
                var span2 = s2.AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(table[i], ref span0[i], ref span1[i], ref span2[i]);
            } while (join.Iterate());
        }
    }


    /// <include file='XMLdoc.xml' path='members/member[@name="T:ForEU"]'/>
    public void For<U>(EntityActionU<C0, C1, C2, U> action, U uniform)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;
        
        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var span0 = s0.AsSpan(0, table.Count);
                var span1 = s1.AsSpan(0, table.Count);
                var span2 = s2.AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(table[i], ref span0[i], ref span1[i], ref span2[i], uniform);
            } while (join.Iterate());
        }
    }
    

    /// <inheritdoc cref="Query{C0}.Job"/>
    public void Job(RefAction<C0, C1, C2> action)
    {
        AssertNotDisposed();
        
        ThreadPool.GetMaxThreads(out var workerThreads, out _);
        var chunkSize = Count / workerThreads;

        using var worldLock = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<Work<C0, C1, C2>>.Rent();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
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

                    var (s0, s1, s2) = join.Select;

                    var job = JobPool<Work<C0, C1, C2>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);
                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
                
            } while (join.Iterate());
        }


        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0, C1, C2>>.Return(jobs);
    }


    /// <inheritdoc cref="Query{C0}.Job{U}"/>
    public void Job<U>(RefActionU<C0, C1, C2, U> action, U uniform)
    {
        AssertNotDisposed();

        ThreadPool.GetMaxThreads(out var workerThreads, out _);
        var chunkSize = Count / workerThreads;
        
        using var worldLock = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, C1, C2, U>>.Rent();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
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

                    var (s0, s1, s2) = join.Select;

                    var job = JobPool<UniformWork<C0, C1, C2, U>>.Rent();
                    job.Memory1 = s0.AsMemory(start, length);
                    job.Memory2 = s1.AsMemory(start, length);
                    job.Memory3 = s2.AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.CountDown = Countdown;
                    jobs.Add(job);
                }
            } while (join.Iterate());
        }

        foreach (var job in jobs)
        {
            ThreadPool.UnsafeQueueUserWorkItem(job, true);
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C0, C1, C2, U>>.Return(jobs);
    }


    /// <inheritdoc cref="Query{C0}.Raw"/>
    public void Raw(MemoryAction<C0, C1, C2> action)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                var mem1 = s1.AsMemory(0, table.Count);
                var mem2 = s2.AsMemory(0, table.Count);
                action(mem0, mem1, mem2);
            } while (join.Iterate());
        }
    }


    /// <inheritdoc cref="Query{C0}.Raw{U}"/>
    public void Raw<U>(MemoryActionU<C0, C1, C2, U> action, U uniform)
    {
        AssertNotDisposed();

        using var worldLock = World.Lock;

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1, s2) = join.Select;
                var mem0 = s0.AsMemory(0, table.Count);
                var mem1 = s1.AsMemory(0, table.Count);
                var mem2 = s2.AsMemory(0, table.Count);
                action(mem0, mem1, mem2, uniform);
            } while (join.Iterate());
        }
    }
    #endregion

    /// <inheritdoc />
    public override Query<C0, C1, C2> Warmup()
    {
        base.Warmup();
        PooledList<Work<C0, C1, C2>>.Rent().Dispose();
        JobPool<Work<C0, C1, C2>>.Return(JobPool<Work<C0, C1, C2>>.Rent());
        return this;
    }
    
    /// <inheritdoc />
    public override Query<C0, C1, C2> Warmup<U>()
    {
        base.Warmup<U>();
        PooledList<UniformWork<C0, C1, C2, U>>.Rent().Dispose();
        JobPool<UniformWork<C0, C1, C2, U>>.Return(JobPool<UniformWork<C0, C1, C2, U>>.Rent());
        return this;
    }
}