// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C1>(Archetypes archetypes, Mask mask, List<Table> tables) : Query(archetypes, mask, tables)
{
    private readonly CountdownEvent _countdown = new(1);

    public ref C1 Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var storage = table.GetStorage<C1>(Identity.None);
        return ref storage[meta.Row];
    }

    #region Runners

    public void Run(RefAction_C<C1> action)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            foreach (ref var c in storage) action(ref c);
        }

        Archetypes.Unlock();
    }

    public void Run<U>(RefAction_CU<C1, U> action, U uniform)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            foreach (ref var c in storage) action(ref c, uniform);
        }

        Archetypes.Unlock();
    }

    public void Run(SpanAction_C<C1> action)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count));
        }

        Archetypes.Unlock();
    }


    public void Job(RefAction_C<C1> action, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();
        _countdown.Reset();

        var jobs = ListPool<Work<C1>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Identity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                _countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<Work<C1>>.Rent();
                job.Memory1 = storage.AsMemory(start, length);
                job.Action = action;
                job.CountDown = _countdown;
                jobs.Add(job);

                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<Work<C1>>.Return(jobs);
        ListPool<Work<C1>>.Return(jobs);

        Archetypes.Unlock();
    }

    public void Job<U>(RefAction_CU<C1, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();
        _countdown.Reset();

        var jobs = ListPool<UniformWork<C1, U>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C1>(Identity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                _countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<UniformWork<C1, U>>.Rent();
                job.Memory = storage.AsMemory(start, length);
                job.Action = action;
                job.Uniform = uniform;
                job.CountDown = _countdown;
                jobs.Add(job);
                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<UniformWork<C1, U>>.Return(jobs);
        ListPool<UniformWork<C1, U>>.Return(jobs);

        Archetypes.Unlock();
    }

    public void Raw(MemoryAction_C<C1> action)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(table.Memory<C1>(Identity.None));
        }

        Archetypes.Unlock();
    }

    #endregion
}