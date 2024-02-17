// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C1, C2>(World world, Mask mask, List<Table> tables) : Query(world, mask, tables)
{
    public RefValueTuple<C1, C2> Get(Entity entity)
    {
        var meta = world.GetEntityMeta(entity.Identity);
        var table = world.GetTable(meta.TableId);
        var storage1 = table.GetStorage<C1>(Identity.None);
        var storage2 = table.GetStorage<C2>(Identity.None);
        return new RefValueTuple<C1, C2>(ref storage1[meta.Row], ref storage2[meta.Row]);
    }
    
    private readonly CountdownEvent _countdown = new(1);

    #region Runners

    public void ForEach(RefAction_CC<C1, C2> action)
    {
        world.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++)
            {
                action(ref storage1[i], ref storage2[i]);
            }
        }

        world.Unlock();
    }

    public void ForEach<U>(RefAction_CCU<C1, C2, U> action, U uniform)
    {
        world.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++)
            {
                action(ref storage1[i], ref storage2[i], uniform);
            }
        }

        world.Unlock();
    }

    public void Span<U>(SpanAction_CCU<C1, C2, U> action, U uniform)
    {
        world.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.Memory<C1>(Identity.None);
            var storage2 = table.Memory<C2>(Identity.None);
            action(storage1.Span, storage2.Span, uniform);
        }

        world.Unlock();
    }

    public void Run(SpanAction_CC<C1, C2> action)
    {
        world.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var span1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var span2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            action(span1, span2);
        }

        world.Unlock();
    }


    public void Job(RefAction_CC<C1, C2> action, int chunkSize = int.MaxValue)
    {
        world.Lock();
        _countdown.Reset();

        using var jobs = PooledList<Work<C1, C2>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                _countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<Work<C1, C2>>.Rent();
                job.Memory1 = storage1.AsMemory(start, length);
                job.Memory2 = storage2.AsMemory(start, length);
                job.Action = action;
                job.CountDown = _countdown;
                jobs.Add(job);

                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<Work<C1, C2>>.Return(jobs);

        world.Unlock();
    }

    public void Job<U>(RefAction_CCU<C1, C2, U> action, in U uniform, int chunkSize = int.MaxValue)
    {
        world.Lock();
        _countdown.Reset();

        using var jobs = PooledList<UniformWork<C1, C2, U>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                _countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<UniformWork<C1, C2, U>>.Rent();
                job.Memory1 = storage1.AsMemory(start, length);
                job.Memory2 = storage2.AsMemory(start, length);
                job.Action = action;
                job.Uniform = uniform;
                job.CountDown = _countdown;
                jobs.Add(job);
                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        _countdown.Signal();
        _countdown.Wait();

        JobPool<UniformWork<C1, C2, U>>.Return(jobs);

        world.Unlock();
    }

    public void Raw(MemoryAction_CC<C1, C2> action)
    {
        world.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(table.Memory<C1>(Identity.None), table.Memory<C2>(Identity.None));
        }

        world.Unlock();
    }
    #endregion
}