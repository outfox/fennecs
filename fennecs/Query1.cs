// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public class Query<C1> : Query
{
    internal Query(World world, Mask mask, List<Archetype> archetypes) : base(world, mask, archetypes)
    {
    }

    public void ForSpan(SpanAction<C1> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            action(table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count));
        }

        World.Unlock();
    }

    
    public void ForSpan<U>(SpanActionU<C1, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            action(storage, uniform);
        }

        World.Unlock();
    }
    
    
    public void ForEach(RefAction<C1> action)
    {
        AssertNotDisposed();
        
        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < storage.Length; i++) action(ref storage[i]);
        }

        World.Unlock();
    }
    
    public void ForEach<U>(RefActionU<C1, U> action, U uniform)
    {
        AssertNotDisposed();
        
        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < storage.Length; i++) action(ref storage[i], uniform);
        }

        World.Unlock();
    }
    
    public void Job(RefAction<C1> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();
        
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C1>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<Work<C1>>.Rent();
                job.Memory1 = storage.AsMemory(start, length);
                job.Action = action;
                job.CountDown = Countdown;
                jobs.Add(job);

                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C1>>.Return(jobs);

        World.Unlock();
    }

    public void Job<U>(RefActionU<C1, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();
        
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C1, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<UniformWork<C1, U>>.Rent();
                job.Memory1 = storage.AsMemory(start, length);
                job.Action = action;
                job.Uniform = uniform;
                job.CountDown = Countdown;
                jobs.Add(job);
                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C1, U>>.Return(jobs);

        World.Unlock();
    }

    public void Raw(MemoryAction<C1> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            action(table.Memory<C1>(Entity.None));
        }

        World.Unlock();
    }

    
    public void Raw<U>(MemoryActionU<C1, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            action(table.Memory<C1>(Entity.None), uniform);
        }

        World.Unlock();
    }
}