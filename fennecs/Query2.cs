// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

/// <summary>
/// Query with 2 output Stream Types, <c>C0</c> and <c>C1</c>.
/// </summary>
public class Query<C0, C1> : Query<C0>
{
    // The counters backing the Query's Cross Join.
    // CAVEAT: stackalloc prevents inlining, thus we preallocate.
    private readonly int[] _counter = new int[2];
    private readonly int[] _limiter = new int[2];
    
    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }

    public void ForSpan(SpanAction<C0, C1> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, count);
                var span1 = storages1[_counter[1]].AsSpan(0, count);
                action(span0, span1);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }

    
    public void ForSpan<U>(SpanActionU<C0, C1, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, count);
                var span1 = storages1[_counter[1]].AsSpan(0, count);
                action(span0, span1, uniform);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }


    public void ForEach(RefAction<C0, C1> action)
    {
        AssertNotDisposed();

        World.Lock();
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, table.Count);
                var span1 = storages1[_counter[1]].AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i]);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }

    public void ForEach<U>(RefActionU<C0, C1, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
                
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            
            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, table.Count);
                var span1 = storages1[_counter[1]].AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], uniform);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }

    public void Job(RefAction<C0, C1> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();
        
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C0, C1>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            
            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var job = JobPool<Work<C0, C1>>.Rent();
                    job.Memory1 = storages0[_counter[0]].AsMemory(start, length);
                    job.Memory2 = storages1[_counter[1]].AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (CrossJoin(_counter, _limiter));
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0, C1>>.Return(jobs);

        World.Unlock();
    }

    public void Job<U>(RefActionU<C0, C1, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, C1, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var job = JobPool<UniformWork<C0, C1, U>>.Rent();
                    job.Memory1 = storages0[_counter[0]].AsMemory(start, length);
                    job.Memory2 = storages1[_counter[1]].AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (CrossJoin(_counter, _limiter));
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C0, C1, U>>.Return(jobs);

        World.Unlock();
    }

    public void Raw(MemoryAction<C0, C1> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            
            do
            {
                var mem0 = storages0[_counter[0]].AsMemory(0, table.Count);
                var mem1 = storages1[_counter[1]].AsMemory(0, table.Count);
                action(mem0, mem1);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }

    public void Raw<U>(MemoryActionU<C0, C1, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            
            do
            {
                var mem0 = storages0[_counter[0]].AsMemory(0, table.Count);
                var mem1 = storages1[_counter[1]].AsMemory(0, table.Count);
                action(mem0, mem1, uniform);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }
}