// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C>(Archetypes archetypes, Mask mask, List<Table> tables) : Query(archetypes, mask, tables)
{
    public ref C Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var storage = table.GetStorage<C>(Identity.None);
        return ref storage[meta.Row];
    }

    #region Runners

    public void Run(QueryAction_C<C> action)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C>(Identity.None).AsSpan(0, table.Count);
            foreach (ref var c in storage) action(ref c);
        }

        Archetypes.Unlock();
    }

    public void RunParallel(QueryAction_C<C> action, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();

        var queued = 0;

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C>(Identity.None);
            var length = table.Count;

            var partitions = Math.Clamp(length / chunkSize, 1, Options.MaxDegreeOfParallelism);
            var partitionSize = length / partitions;

            for (var partition = 1; partition < partitions; partition++)
            {
                Interlocked.Increment(ref queued);

                ThreadPool.QueueUserWorkItem(delegate(int part)
                {
                    foreach (ref var c in storage.AsSpan(part * partitionSize, partitionSize))
                    {
                        action(ref c);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Decrement(ref queued);
                }, partition, preferLocal: true);
            }

            //Optimization: Also process one partition right here on the calling thread.
            foreach (ref var c in storage.AsSpan(0, partitionSize))
            {
                action(ref c);
            }
        }

        while (queued > 0) Thread.SpinWait(SpinTimeout);
        Archetypes.Unlock();
    }
    
    public void Run<U>(QueryAction_CU<C, U> action, U uniform)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C>(Identity.None).AsSpan(0, table.Count);
            foreach (ref var c in storage) action(ref c, uniform);
        }

        Archetypes.Unlock();
    }


    public void RunParallel<U>(QueryAction_CU<C, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();
        var queued = 0;

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage = table.GetStorage<C>(Identity.None);

            var length = table.Count;

            var partitions = Math.Clamp(length / chunkSize, 1, Options.MaxDegreeOfParallelism);
            var partitionSize = length / partitions;

            for (var partition = 1; partition < partitions; partition++)
            {
                Interlocked.Increment(ref queued);

                ThreadPool.QueueUserWorkItem(delegate(int part)
                {
                    foreach (ref var c in storage.AsSpan(part * partitionSize, partitionSize))
                    {
                        action(ref c, uniform);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Decrement(ref queued);
                }, partition, preferLocal: true);
            }

            //Optimization: Also process one partition right here on the calling thread.
            foreach (ref var c in storage.AsSpan(0, partitionSize))
            {
                action(ref c, uniform);
            }
        }

        while (queued > 0) Thread.Yield();
        Archetypes.Unlock();
    }


    public void Run(SpanAction_C<C> action)
    {
        Archetypes.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(table.GetStorage<C>(Identity.None).AsSpan(0, table.Count));
        }

        Archetypes.Unlock();
    }

    public void Raw(Action<Memory<C>> action)
    {
        Archetypes.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            action(table.GetStorage<C>(Identity.None).AsMemory(0, table.Count));
        }

        Archetypes.Unlock();
    }

    public void RawParallel(Action<Memory<C>> action)
    {
        Archetypes.Lock();
        Parallel.ForEach(Tables, Options, table =>
        {
            if (table.IsEmpty) return; //TODO: This wastes a scheduled thread.
            action(table.GetStorage<C>(Identity.None).AsMemory(0, table.Count)); 
        });
        
        Archetypes.Unlock();
    }
    #endregion
}