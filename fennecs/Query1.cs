// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public class Query<C0> : Query
{
    private readonly List<C0[]> _storages0 = new(64);
    private readonly int[] _counter = new int[1];
    private readonly int[] _limiter = new int[1];
    
    internal Query(World world, Mask mask, List<Archetype> archetypes) : base(world, mask, archetypes)
    {
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

            table.Match<C0>(Mask.HasTypes[0], _storages0);

            counters[0] = 0;
            goals[0] = _storages0.Count;

            do
            {
                var span0 = _storages0[counters[0]].AsSpan(0, count);
                action(span0);
            } while (CrossJoin(counters, goals));

            _storages0.Clear();
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

            table.Match<C0>(Mask.HasTypes[0], _storages0);

            _counter[0] = 0;
            _limiter[0] = _storages0.Count;

            do
            {
                var span0 = _storages0[_counter[0]].AsSpan(0, table.Count);
                foreach (ref var c0 in span0) action(ref c0);
            } while (CrossJoin(_counter, _limiter));

            _storages0.Clear();
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

            table.Match<C0>(Mask.HasTypes[0], _storages0);

            _counter[0] = 0;
            _limiter[0] = _storages0.Count;

            do
            {
                var span0 = _storages0[_counter[0]].AsSpan(0, table.Count);
                foreach (ref var c0 in span0) action(ref c0, uniform);
            } while (CrossJoin(_counter, _limiter));

            _storages0.Clear();
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