// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C1, C2, C3, C4, C5>(Archetypes archetypes, Mask mask, List<Table> tables) : Query(archetypes, mask, tables)
{
    public RefValueTuple<C1, C2, C3, C4, C5> Get(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        var s1 = table.GetStorage<C1>(Identity.None);
        var s2 = table.GetStorage<C2>(Identity.None);
        var s3 = table.GetStorage<C3>(Identity.None);
        var s4 = table.GetStorage<C4>(Identity.None);
        var s5 = table.GetStorage<C5>(Identity.None);
        return new RefValueTuple<C1, C2, C3, C4, C5>(ref s1[meta.Row], ref s2[meta.Row],
            ref s3[meta.Row], ref s4[meta.Row], ref s5[meta.Row]);
    }

    #region Runners

     public void Run(QueryAction_CCCCC<C1, C2, C3, C4, C5> action)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            var s3 = table.GetStorage<C3>(Identity.None).AsSpan(0, table.Count);
            var s4 = table.GetStorage<C4>(Identity.None).AsSpan(0, table.Count);
            var s5 = table.GetStorage<C5>(Identity.None).AsSpan(0, table.Count);

            for (var i = 0; i < table.Count; i++) action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], ref s5[i]);
        }

        Archetypes.Unlock();
    }

    public void RunParallel(QueryAction_CCCCC<C1, C2, C3, C4, C5> action, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();

        var queued = 0;

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);
            var storage3 = table.GetStorage<C3>(Identity.None);
            var storage4 = table.GetStorage<C4>(Identity.None);
            var storage5 = table.GetStorage<C5>(Identity.None);
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
                    var s3 = storage3.AsSpan(part * partitionSize, partitionSize);
                    var s4 = storage4.AsSpan(part * partitionSize, partitionSize);
                    var s5 = storage5.AsSpan(part * partitionSize, partitionSize);
                    
                    for (var i = part * partitionSize; i < (part + 1) * partitionSize; i++)
                    {
                        action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], ref s5[i]);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Decrement(ref queued);
                }, partition, preferLocal: true);
            }

            //Optimization: Also process one partition right here on the calling thread.
            var s1 = storage1.AsSpan(0, partitionSize);
            var s2 = storage2.AsSpan(0, partitionSize);
            var s3 = storage3.AsSpan(0, partitionSize);
            var s4 = storage4.AsSpan(0, partitionSize);
            var s5 = storage5.AsSpan(0, partitionSize);
            for (var i = 0; i < partitionSize; i++)
            {
                action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], ref s5[i]);
            }
        }

        while (queued > 0) Thread.SpinWait(SpinTimeout);
        Archetypes.Unlock();
    }
    
    public void Run<U>(QueryAction_CCCCCU<C1, C2, C3, C4, C5, U> action, U uniform)
    {
        Archetypes.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            var s3 = table.GetStorage<C3>(Identity.None).AsSpan(0, table.Count);
            var s4 = table.GetStorage<C4>(Identity.None).AsSpan(0, table.Count);
            var s5 = table.GetStorage<C5>(Identity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++) action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], ref s5[i], uniform);
        }

        Archetypes.Unlock();
    }


    public void RunParallel<U>(QueryAction_CCCCCU<C1, C2, C3, C4, C5, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();
        var queued = 0;

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);
            var storage3 = table.GetStorage<C3>(Identity.None);
            var storage4 = table.GetStorage<C4>(Identity.None);
            var storage5 = table.GetStorage<C5>(Identity.None);
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
                    var s3 = storage3.AsSpan(part * partitionSize, partitionSize);
                    var s4 = storage4.AsSpan(part * partitionSize, partitionSize);
                    var s5 = storage5.AsSpan(part * partitionSize, partitionSize);

                    for (var i = part * partitionSize; i < (part + 1) * partitionSize; i++)
                    {
                        action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], ref s5[i], uniform);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Decrement(ref queued);
                }, partition, preferLocal: true);
            }

            //Optimization: Also process one partition right here on the calling thread.
            var s1 = storage1.AsSpan(0, partitionSize);
            var s2 = storage2.AsSpan(0, partitionSize);
            var s3 = storage3.AsSpan(0, partitionSize);
            var s4 = storage4.AsSpan(0, partitionSize);
            var s5 = storage5.AsSpan(0, partitionSize);
            for (var i = 0; i < partitionSize; i++)
            {
                action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], ref s5[i], uniform);
            }
        }

        while (queued > 0) Thread.SpinWait(SpinTimeout);
        Archetypes.Unlock();

    }


    public void Run(SpanAction_CCCCC<C1, C2, C3, C4, C5> action)
    {
        Archetypes.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            var s3 = table.GetStorage<C3>(Identity.None).AsSpan(0, table.Count);
            var s4 = table.GetStorage<C4>(Identity.None).AsSpan(0, table.Count);
            var s5 = table.GetStorage<C5>(Identity.None).AsSpan(0, table.Count);
            action(s1, s2, s3, s4, s5);
        }

        Archetypes.Unlock();
    }

    public void Raw(Action<Memory<C1>, Memory<C2>, Memory<C3>, Memory<C4>, Memory<C5>> action)
    {
        Archetypes.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var m1 = table.GetStorage<C1>(Identity.None).AsMemory(0, table.Count);
            var m2 = table.GetStorage<C2>(Identity.None).AsMemory(0, table.Count);
            var m3 = table.GetStorage<C3>(Identity.None).AsMemory(0, table.Count);
            var m4 = table.GetStorage<C4>(Identity.None).AsMemory(0, table.Count);
            var m5 = table.GetStorage<C5>(Identity.None).AsMemory(0, table.Count);
            action(m1, m2, m3, m4, m5);
        }

        Archetypes.Unlock();
    }

    public void RawParallel(Action<Memory<C1>, Memory<C2>, Memory<C3>, Memory<C4>, Memory<C5>> action)
    {
        Archetypes.Lock();
        Parallel.ForEach(Tables, Options,
            table =>
            {
                if (table.IsEmpty) return; //TODO: This wastes a scheduled thread.
                var m1 = table.GetStorage<C1>(Identity.None).AsMemory(0, table.Count);
                var m2 = table.GetStorage<C2>(Identity.None).AsMemory(0, table.Count);
                var m3 = table.GetStorage<C3>(Identity.None).AsMemory(0, table.Count);
                var m4 = table.GetStorage<C4>(Identity.None).AsMemory(0, table.Count);
                var m5 = table.GetStorage<C5>(Identity.None).AsMemory(0, table.Count);
                action(m1, m2, m3, m4, m5);
            });

        Archetypes.Unlock();
    }
    #endregion
}