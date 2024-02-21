// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public class Query<C1, C2, C3, C4, C5>(World world, Mask mask, List<Table> tables) : Query(world, mask, tables)
{
     public void ForEach(RefAction_CCCCC<C1, C2, C3, C4, C5> action)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Entity.None).AsSpan(0, table.Count);
            var storage3 = table.GetStorage<C3>(Entity.None).AsSpan(0, table.Count);
            var storage4 = table.GetStorage<C4>(Entity.None).AsSpan(0, table.Count);
            var storage5 = table.GetStorage<C5>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++)
            {
                action(ref storage1[i], ref storage2[i], ref storage3[i], ref storage4[i], ref storage5[i]);
            }
        }

        World.Unlock();
    }
    
    public void ForEach<U>(RefAction_CCCCCU<C1, C2, C3, C4, C5, U> action, U uniform)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Entity.None).AsSpan(0, table.Count);
            var storage3 = table.GetStorage<C3>(Entity.None).AsSpan(0, table.Count);
            var storage4 = table.GetStorage<C4>(Entity.None).AsSpan(0, table.Count);
            var storage5 = table.GetStorage<C5>(Entity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++)
            {
                action(ref storage1[i], ref storage2[i], ref storage3[i], ref storage4[i], ref storage5[i], uniform);
            }
        }

        World.Unlock();
    }
    
    public void ForSpan<U>(SpanAction_CCCCCU<C1, C2, C3, C4, C5, U> action, U uniform)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.Memory<C1>(Entity.None);
            var storage2 = table.Memory<C2>(Entity.None);
            var storage3 = table.Memory<C3>(Entity.None);
            var storage4 = table.Memory<C4>(Entity.None);
            var storage5 = table.Memory<C5>(Entity.None);
            action(storage1.Span, storage2.Span, storage3.Span, storage4.Span, storage5.Span, uniform);
        }

        World.Unlock();
    }
    
    public void ForSpan(SpanAction_CCCCC<C1, C2, C3, C4, C5> action)
    {
        World.Lock();
        
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.Memory<C1>(Entity.None);
            var storage2 = table.Memory<C2>(Entity.None);
            var storage3 = table.Memory<C3>(Entity.None);
            var storage4 = table.Memory<C4>(Entity.None);
            var storage5 = table.Memory<C5>(Entity.None);
            action(storage1.Span, storage2.Span, storage3.Span, storage4.Span, storage5.Span);
        }

        World.Unlock();
    }

    public void Job(RefAction_CCCCC<C1, C2, C3, C4, C5> action, int chunkSize = int.MaxValue)
    {
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<Work<C1, C2, C3, C4, C5>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None);
            var storage2 = table.GetStorage<C2>(Entity.None);
            var storage3 = table.GetStorage<C3>(Entity.None);
            var storage4 = table.GetStorage<C4>(Entity.None);
            var storage5 = table.GetStorage<C5>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<Work<C1, C2, C3, C4, C5>>.Rent();
                job.Memory1 = storage1.AsMemory(start, length);
                job.Memory2 = storage2.AsMemory(start, length);
                job.Memory3 = storage3.AsMemory(start, length);
                job.Memory4 = storage4.AsMemory(start, length);
                job.Memory5 = storage5.AsMemory(start, length);
                
                job.Action = action;
                job.CountDown = Countdown;
                jobs.Add(job);

                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<Work<C1, C2, C3, C4, C5>>.Return(jobs);

        World.Unlock();
    }
    
    public void Job<U>(RefAction_CCCCCU<C1, C2, C3, C4, C5, U> action, in U uniform, int chunkSize = int.MaxValue)
    {
        World.Lock();
        Countdown.Reset();

        using var jobs = PooledList<UniformWork<C1, C2, C3, C4, C5, U>>.Rent();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Entity.None);
            var storage2 = table.GetStorage<C2>(Entity.None);
            var storage3 = table.GetStorage<C3>(Entity.None);
            var storage4 = table.GetStorage<C4>(Entity.None);
            var storage5 = table.GetStorage<C5>(Entity.None);

            var count = table.Count; // storage.Length is the capacity, not the count.
            var partitions = count / chunkSize + Math.Sign(count % chunkSize);

            for (var chunk = 0; chunk < partitions; chunk++)
            {
                Countdown.AddCount();

                var start = chunk * chunkSize;
                var length = Math.Min(chunkSize, count - start);

                var job = JobPool<UniformWork<C1, C2, C3, C4, C5, U>>.Rent();
                job.Memory1 = storage1.AsMemory(start, length);
                job.Memory2 = storage2.AsMemory(start, length);
                job.Memory3 = storage3.AsMemory(start, length);
                job.Memory4 = storage4.AsMemory(start, length);
                job.Memory5 = storage5.AsMemory(start, length);
                job.Action = action;
                job.Uniform = uniform;
                job.CountDown = Countdown;
                jobs.Add(job);
                ThreadPool.UnsafeQueueUserWorkItem(job, true);
            }
        }

        Countdown.Signal();
        Countdown.Wait();

        JobPool<UniformWork<C1, C2, C3, C4, C5, U>>.Return(jobs);

        World.Unlock();
    }
    
    public void Raw(MemoryAction_CCCCC<C1, C2, C3, C4, C5> action)
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
                table.Memory<C5>(Entity.None)
            );
        }

        World.Unlock();
    }

    public void Raw<U>(MemoryAction_CCCCCU<C1, C2, C3, C4, C5, U> action, U uniform)
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
                table.Memory<C5>(Entity.None),
                uniform
            );
        }

        World.Unlock();
    }
}