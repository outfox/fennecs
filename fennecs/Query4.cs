// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public class Query<C1, C2, C3, C4>(World world, Mask mask, List<Table> tables) : Query(world, mask, tables)
{
     public void ForEach(RefAction_CCCC<C1, C2, C3, C4> action)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Entity.None).AsSpan(0, table.Count);
            var storage3 = table.GetStorage<C3>(Entity.None).AsSpan(0, table.Count);
            var storage4 = table.GetStorage<C4>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++)
            {
                action(ref storage1[i], ref storage2[i], ref storage3[i], ref storage4[i]);
            }
        }

        World.Unlock();
    }
    
    public void ForEach<U>(RefAction_CCCCU<C1, C2, C3, C4, U> action, U uniform)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Entity.None).AsSpan(0, table.Count);
            var storage3 = table.GetStorage<C3>(Entity.None).AsSpan(0, table.Count);
            var storage4 = table.GetStorage<C4>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++)
            {
                action(ref storage1[i], ref storage2[i], ref storage3[i], ref storage4[i], uniform);
            }
        }

        World.Unlock();
    }
    
    public void ForSpan<U>(SpanAction_CCCCU<C1, C2, C3, C4, U> action, U uniform)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.Memory<C1>(Entity.None);
            var storage2 = table.Memory<C2>(Entity.None);
            var storage3 = table.Memory<C3>(Entity.None);
            var storage4 = table.Memory<C4>(Entity.None);
            action(storage1.Span, storage2.Span, storage3.Span, storage4.Span, uniform);
        }

        World.Unlock();
    }
    
    public void ForSpan(SpanAction_CCCC<C1, C2, C3, C4> action)
    {
        World.Lock();
        
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.Memory<C1>(Entity.None);
            var storage2 = table.Memory<C2>(Entity.None);
            var storage3 = table.Memory<C3>(Entity.None);
            var storage4 = table.Memory<C4>(Entity.None);
            action(storage1.Span, storage2.Span, storage3.Span, storage4.Span);
        }

        World.Unlock();
    }

    public void Job(RefAction_CCCC<C1, C2, C3, C4> action, int chunkSize = int.MaxValue)
    {
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C1, C2, C3, C4>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None);
            var storage2 = table.GetStorage<C2>(Entity.None);
            var storage3 = table.GetStorage<C3>(Entity.None);
            var storage4 = table.GetStorage<C4>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<Work<C1, C2, C3, C4>>.Rent();
                job.Memory1 = storage1.AsMemory(start, length);
                job.Memory2 = storage2.AsMemory(start, length);
                job.Memory3 = storage3.AsMemory(start, length);
                job.Memory4 = storage4.AsMemory(start, length);
                
                job.Action = action;
                job.CountDown = Countdown;
                jobs.Add(job);

                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C1, C2, C3, C4>>.Return(jobs);

        World.Unlock();
    }
    
    public void Job<U>(RefAction_CCCCU<C1, C2, C3, C4, U> action, in U uniform, int chunkSize = int.MaxValue)
    {
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C1, C2, C3, C4, U>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None);
            var storage2 = table.GetStorage<C2>(Entity.None);
            var storage3 = table.GetStorage<C3>(Entity.None);
            var storage4 = table.GetStorage<C4>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<UniformWork<C1, C2, C3, C4, U>>.Rent();
                job.Memory1 = storage1.AsMemory(start, length);
                job.Memory2 = storage2.AsMemory(start, length);
                job.Memory3 = storage3.AsMemory(start, length);
                job.Memory4 = storage4.AsMemory(start, length);
                job.Action = action;
                job.Uniform = uniform;
                job.CountDown = Countdown;
                jobs.Add(job);
                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C1, C2, C3, C4, U>>.Return(jobs);

        World.Unlock();
    }
    
    public void Raw(MemoryAction_CCCC<C1, C2, C3, C4> action)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(
                table.Memory<C1>(Entity.None),
                table.Memory<C2>(Entity.None),
                table.Memory<C3>(Entity.None),
                table.Memory<C4>(Entity.None)
            );
        }

        World.Unlock();
    }

    public void Raw<U>(MemoryAction_CCCCU<C1, C2, C3, C4, U> action, U uniform)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(
                table.Memory<C1>(Entity.None),
                table.Memory<C2>(Entity.None),
                table.Memory<C3>(Entity.None),
                table.Memory<C4>(Entity.None),
                uniform
            );
        }

        World.Unlock();
    }

}