// SPDX-License-Identifier: MIT

using System.Data;
using fennecs.pools;

namespace fennecs;

public class Query<C0> : Query
{
    internal Query(World world, Mask mask, List<Archetype> archetypes) : base(world, mask, archetypes)
    {
    }

    private void Comb1(SpanAction<C0> action, PooledList<C0[]> l0)
    {
        Span<int> counters = stackalloc int[1];

        counters[0] = l0.Count;
        
        for (var i = 0; i < l0.Count; i++)
        {
            action(l0[i]);
        }
        
        l0.Dispose();
    }

    public void ForSpan(SpanAction<C0> action)
    {
        AssertNotDisposed();

        World.Lock();

        Span<int> counters = stackalloc int[1];
        Span<int> goals    = stackalloc int[1];
        
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(Mask.HasTypes[0]);

            counters[0] = 0;
            goals[0] = storages0.Count;

            do
            {
                var span0 = storages0[counters[0]].AsSpan(0, count);
                action(span0);
            } while (FullJoin(ref counters, ref goals));
        }

        World.Unlock();
    }

    
    public void ForSpan<U>(SpanActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C0>(Entity.None).AsSpan(0, table.Count);
            action(storage, uniform);
        }

        World.Unlock();
    }
    
    
    public void ForEach(RefAction<C0> action)
    {
        AssertNotDisposed();
        
        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C0>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < storage.Length; i++) action(ref storage[i]);
        }

        World.Unlock();
    }
    
    public void ForEach<U>(RefActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();
        
        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C0>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < storage.Length; i++) action(ref storage[i], uniform);
        }

        World.Unlock();
    }
    
    public void Job(RefAction<C0> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();
        
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C0>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C0>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<Work<C0>>.Rent();
                job.Memory1 = storage.AsMemory(start, length);
                job.Action = action;
                job.CountDown = Countdown;
                jobs.Add(job);

                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0>>.Return(jobs);

        World.Unlock();
    }

    public void Job<U>(RefActionU<C0, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();
        
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C0>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<UniformWork<C0, U>>.Rent();
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

        JobPool<UniformWork<C0, U>>.Return(jobs);

        World.Unlock();
    }

    public void Raw(MemoryAction<C0> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            action(table.Memory<C0>(Entity.None));
        }

        World.Unlock();
    }

    
    public void Raw<U>(MemoryActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            action(table.Memory<C0>(Entity.None), uniform);
        }

        World.Unlock();
    }
}