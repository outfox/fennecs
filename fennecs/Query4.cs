// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public class Query<C0, C1, C2, C3> : Query<C0, C1, C2>
{
    // The counters backing the Query's Cross Join.
    // CAVEAT: stackalloc prevents inlining, thus we preallocate.
    private readonly int[] _counter = new int[4];
    private readonly int[] _limiter = new int[4];

    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }

    public void ForSpan(SpanAction<C0, C1, C2, C3> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, count);
                var span1 = storages1[_counter[1]].AsSpan(0, count);
                var span2 = storages2[_counter[2]].AsSpan(0, count);
                var span3 = storages3[_counter[3]].AsSpan(0, count);
                action(span0, span1, span2, span3);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }

    public void ForSpan<U>(SpanActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, count);
                var span1 = storages1[_counter[1]].AsSpan(0, count);
                var span2 = storages2[_counter[2]].AsSpan(0, count);
                var span3 = storages3[_counter[3]].AsSpan(0, count);
                action(span0, span1, span2, span3, uniform);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }
    
    public void ForEach(RefAction<C0, C1, C2, C3> action)
    {
        AssertNotDisposed();

        World.Lock();
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, table.Count);
                var span1 = storages1[_counter[1]].AsSpan(0, table.Count);
                var span2 = storages2[_counter[2]].AsSpan(0, table.Count);
                var span3 = storages3[_counter[3]].AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], ref span2[i], ref span3[i]);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }


    public void ForEach<U>(RefActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);
            
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, table.Count);
                var span1 = storages1[_counter[1]].AsSpan(0, table.Count);
                var span2 = storages2[_counter[2]].AsSpan(0, table.Count);
                var span3 = storages3[_counter[3]].AsSpan(0, table.Count);
                for (var i = 0; i < table.Count; i++) action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], uniform);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }


    public void Job(RefAction<C0, C1, C2, C3> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C0, C1, C2, C3>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var job = JobPool<Work<C0, C1, C2, C3>>.Rent();
                    job.Memory1 = storages0[_counter[0]].AsMemory(start, length);
                    job.Memory2 = storages1[_counter[1]].AsMemory(start, length);
                    job.Memory3 = storages2[_counter[2]].AsMemory(start, length);
                    job.Memory4 = storages3[_counter[3]].AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (CrossJoin(_counter, _limiter));
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0, C1, C2, C3>>.Return(jobs);

        World.Unlock();
    }


    public void Job<U>(RefActionU<C0, C1, C2, C3, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, C1, C2, C3, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var job = JobPool<UniformWork<C0, C1, C2, C3, U>>.Rent();
                    job.Memory1 = storages0[_counter[0]].AsMemory(start, length);
                    job.Memory2 = storages1[_counter[1]].AsMemory(start, length);
                    job.Memory3 = storages2[_counter[2]].AsMemory(start, length);
                    job.Memory4 = storages3[_counter[3]].AsMemory(start, length);
                    
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

        JobPool<UniformWork<C0, C1, C2, C3, U>>.Return(jobs);

        World.Unlock();
    }

    public void Raw(MemoryAction<C0, C1, C2, C3> action)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);
            
            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;
            
            do
            {
                var mem0 = storages0[_counter[0]].AsMemory(0, table.Count);
                var mem1 = storages1[_counter[1]].AsMemory(0, table.Count);
                var mem2 = storages2[_counter[2]].AsMemory(0, table.Count);
                var mem3 = storages3[_counter[3]].AsMemory(0, table.Count);
                action(mem0, mem1, mem2, mem3);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }

    public void Raw<U>(MemoryActionU<C0, C1, C2, C3, U> action, U uniform)
    {
        AssertNotDisposed();

        World.Lock();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            using var storages1 = table.Match<C1>(StreamTypes[1]);
            using var storages2 = table.Match<C2>(StreamTypes[2]);
            using var storages3 = table.Match<C3>(StreamTypes[3]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;
            _counter[1] = 0;
            _limiter[1] = storages1.Count;
            _counter[2] = 0;
            _limiter[2] = storages2.Count;
            _counter[3] = 0;
            _limiter[3] = storages3.Count;

            do
            {
                var mem0 = storages0[_counter[0]].AsMemory(0, table.Count);
                var mem1 = storages1[_counter[1]].AsMemory(0, table.Count);
                var mem2 = storages2[_counter[2]].AsMemory(0, table.Count);
                var mem3 = storages3[_counter[3]].AsMemory(0, table.Count);
                action(mem0, mem1, mem2, mem3, uniform);
            } while (CrossJoin(_counter, _limiter));
        }

        World.Unlock();
    }
}