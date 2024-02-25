// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

/// <summary>
/// <para>
/// Query with 1 output Stream Type, <c>C0</c>.
/// </para>
/// <para>
/// Queries expose methods to rapidly iterate all Entities that match their Mask and Stream Types.
/// </para>
/// <ul>
/// <li><c>ForEach(...)</c> - call a delegate <see cref="RefAction{C0}"/> for each entity.</li>
/// <li><c>Job(...)</c> - parallel process, calling a delegate <see cref="RefAction{C0}"/> for each entity.</li>
/// <li><c>ForSpan(...)</c> - call a delegate <see cref="SpanAction{C0}"/> per matched Archetype (× matched Wildcards) of entities.</li>
/// <li><c>Raw(...)</c> - pass Memory objects too a delegate <see cref="MemoryAction{C0}"/> per matched Archetype (× matched Wildcards) of entities.</li>
/// </ul>
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class Query<C0> : Query
{
    #region Internals

    // The counters backing the Query's Cross Join.
    // CAVEAT: stackalloc prevents inlining, thus we preallocate.
    private readonly int[] _counter = new int[1];
    private readonly int[] _limiter = new int[1];


    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes) : base(world, streamTypes, mask, archetypes)
    {
    }

    #endregion


    #region Runners

    public void ForSpan(SpanAction<C0> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, count);
                action(span0);
            } while (Match.CrossJoin(_counter, _limiter));
        }
    }


    public void ForSpan<U>(SpanActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;
            var count = table.Count;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, count);
                action(span0, uniform);
            } while (Match.CrossJoin(_counter, _limiter));
        }
    }


    public void ForEach(RefAction<C0> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, table.Count);
                foreach (ref var c0 in span0) action(ref c0);
            } while (Match.CrossJoin(_counter, _limiter));
        }
    }


    public void ForEach<U>(RefActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            do
            {
                var span0 = storages0[_counter[0]].AsSpan(0, table.Count);
                foreach (ref var c0 in span0)
                {
                    action(ref c0, uniform);
                }
            } while (Match.CrossJoin(_counter, _limiter));
        }
    }


    public void Job(RefAction<C0> action, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<Work<C0>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var job = JobPool<Work<C0>>.Rent();
                    job.Memory1 = storages0[_counter[0]].AsMemory(start, length);
                    job.Action = action;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (Match.CrossJoin(_counter, _limiter));
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C0>>.Return(jobs);
    }


    public void Job<U>(RefActionU<C0, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        AssertNotDisposed();

        using var lck = World.Lock;
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C0, U>>.Rent();

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);
            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);
            do
            {
                for (var chunk = 0; chunk < partitions; chunk++)
                {
                    Countdown.AddCount();

                    var start = chunk * chunkSize;
                    var length = Math.Min(chunkSize, count - start);

                    var job = JobPool<UniformWork<C0, U>>.Rent();
                    job.Memory1 = storages0[_counter[0]].AsMemory(start, length);
                    job.Action = action;
                    job.Uniform = uniform;
                    job.CountDown = Countdown;
                    jobs.Add(job);

                    ThreadPool.UnsafeQueueUserWorkItem(job, true);
                }
            } while (Match.CrossJoin(_counter, _limiter));
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C0, U>>.Return(jobs);
    }


    public void Raw(MemoryAction<C0> action)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            do
            {
                var mem0 = storages0[_counter[0]].AsMemory(0, table.Count);
                action(mem0);
            } while (Match.CrossJoin(_counter, _limiter));
        }
    }


    public void Raw<U>(MemoryActionU<C0, U> action, U uniform)
    {
        AssertNotDisposed();

        using var lck = World.Lock;

        foreach (var table in Archetypes)
        {
            if (table.IsEmpty) continue;

            using var storages0 = table.Match<C0>(StreamTypes[0]);

            _counter[0] = 0;
            _limiter[0] = storages0.Count;

            do
            {
                var mem0 = storages0[_counter[0]].AsMemory(0, table.Count);
                action(mem0, uniform);
            } while (Match.CrossJoin(_counter, _limiter));
        }
    }

    #endregion
}