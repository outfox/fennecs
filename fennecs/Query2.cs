// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C1, C2>(Archetypes archetypes, Mask mask, List<Table> tables) : Query(archetypes, mask, tables)
{
    public RefValueTuple<C1, C2> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var storage1 = table.GetStorage<C1>(Identity.None);
        var storage2 = table.GetStorage<C2>(Identity.None);
        return new RefValueTuple<C1, C2>(ref storage1[meta.Row], ref storage2[meta.Row]);
    }

     #region Runners

     public void Run(RefAction_CC<C1, C2> action)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var storage2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);

            for (var i = 0; i < table.Count; i++) action(ref storage1[i], ref storage2[i]);
        }

        Archetypes.Unlock();
    }

    public void RunParallel(RefAction_CC<C1, C2> action, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();

        var queued = 0;

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);
            var length = table.Count;

            var partitions = Math.Clamp(length / chunkSize, 1, Options.MaxDegreeOfParallelism);
            var partitionSize = length / partitions;

            for (var partition = 1; partition < partitions; partition++)
            {
                Interlocked.Increment(ref queued);

                ThreadPool.QueueUserWorkItem(delegate(int part)
                {
                    var s1 = storage1.AsSpan(part * partitionSize, partitionSize);
                    var s2 = storage2.AsSpan(part * partitionSize, partitionSize);
                    
                    for (var i = 0; i < s1.Length; i++)
                    {
                        action(ref s1[i], ref s2[i]);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Decrement(ref queued);
                }, partition, preferLocal: true);
            }

            //Optimization: Also process one partition right here on the calling thread.
            var s1 = storage1.AsSpan();
            var s2 = storage2.AsSpan();
            for (var i = 0; i < partitionSize; i++)
            {
                action(ref s1[i], ref s2[i]);
            }
        }

        while (queued > 0) Thread.SpinWait(SpinTimeout);
        Archetypes.Unlock();
    }
    
    public void Run<U>(RefAction_CCU<C1, C2, U> action, U uniform)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++) action(ref s1[i], ref s2[i], uniform);
        }

        Archetypes.Unlock();
    }


    public void RunParallel<U>(RefAction_CCU<C1, C2, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();
        var queued = 0;

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);
            var length = table.Count;

            var partitions = Math.Clamp(length / chunkSize, 1, Options.MaxDegreeOfParallelism);
            var partitionSize = length / partitions;

            for (var partition = 1; partition < partitions; partition++)
            {
                Interlocked.Increment(ref queued);

                ThreadPool.QueueUserWorkItem(delegate(int part)
                {
                    var s1 = storage1.AsSpan(part * partitionSize, partitionSize);
                    var s2 = storage2.AsSpan(part * partitionSize, partitionSize);

                    for (var i = 0; i < s1.Length; i++)
                    {
                        action(ref s1[i], ref s2[i], uniform);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Decrement(ref queued);
                }, partition, preferLocal: true);
            }

            //Optimization: Also process one partition right here on the calling thread.
            var s1 = storage1.AsSpan(0, partitionSize);
            var s2 = storage2.AsSpan(0, partitionSize);
            for (var i = 0; i < partitionSize; i++)
            {
                action(ref s1[i], ref s2[i], uniform);
            }
        }

        while (queued > 0) Thread.SpinWait(SpinTimeout);
        Archetypes.Unlock();

    }


    public void Run(SpanAction_CC<C1, C2> action)
    {
        Archetypes.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            action(s1, s2);
        }

        Archetypes.Unlock();
    }

    public void Raw(Action<Memory<C1>, Memory<C2>> action)
    {
        Archetypes.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var m1 = table.GetStorage<C1>(Identity.None).AsMemory(0, table.Count);
            var m2 = table.GetStorage<C2>(Identity.None).AsMemory(0, table.Count);
            action(m1, m2);
        }

        Archetypes.Unlock();
    }

    public void RawParallel(Action<Memory<C1>, Memory<C2>> action)
    {
        Archetypes.Lock();
        
        Parallel.ForEach(Tables, Options,
            table =>
            {
                if (table.IsEmpty) return; //TODO: This wastes a scheduled thread.
                var m1 = table.GetStorage<C1>(Identity.None).AsMemory(0, table.Count);
                var m2 = table.GetStorage<C2>(Identity.None).AsMemory(0, table.Count);
                action(m1, m2);
            });

        Archetypes.Unlock();
    }
    #endregion

}