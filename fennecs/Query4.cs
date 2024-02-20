// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C1, C2, C3, C4>(World world, Mask mask, List<Table> tables) : Query(world, mask, tables)
{
    #region Runners

    public void Run(RefAction_CCCC<C1, C2, C3, C4> action)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            var s3 = table.GetStorage<C3>(Identity.None).AsSpan(0, table.Count);
            var s4 = table.GetStorage<C4>(Identity.None).AsSpan(0, table.Count);

            for (var i = 0; i < table.Count; i++) action(ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
        }

        World.Unlock();
    }

    public void RunParallel(RefAction_CCCC<C1, C2, C3, C4> action, int chunkSize = int.MaxValue)
    {
        World.Lock();

        using var countdown = new CountdownEvent(1);

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);
            var storage3 = table.GetStorage<C3>(Identity.None);
            var storage4 = table.GetStorage<C4>(Identity.None);
            var length = table.Count;

            var partitions = Math.Max(length / chunkSize, 1);
            var partitionSize = length / partitions;

            for (var partition = 0; partition < partitions; partition++)
            {
                countdown.AddCount();

                ThreadPool.QueueUserWorkItem(delegate(int part)
                {
                    var s1 = storage1.AsSpan(part * partitionSize, partitionSize);
                    var s2 = storage2.AsSpan(part * partitionSize, partitionSize);
                    var s3 = storage3.AsSpan(part * partitionSize, partitionSize);
                    var s4 = storage4.AsSpan(part * partitionSize, partitionSize);

                    for (var i = 0; i < s1.Length; i++)
                    {
                        action(ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    countdown.Signal();
                }, partition, preferLocal: true);
            }

            /*
            //Optimization: Also process one partition right here on the calling thread.
            var s1 = storage1.AsSpan(0, partitionSize);
            var s2 = storage2.AsSpan(0, partitionSize);
            var s3 = storage3.AsSpan(0, partitionSize);
            var s4 = storage4.AsSpan(0, partitionSize);
            for (var i = 0; i < partitionSize; i++)
            {
                action(ref s1[i], ref s2[i], ref s3[i], ref s4[i]);
            }
            */
        }

        countdown.Signal();
        countdown.Wait();
        World.Unlock();
    }

    public void Run<U>(RefAction_CCCCU<C1, C2, C3, C4, U> action, U uniform)
    {
        World.Lock();

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            var s3 = table.GetStorage<C3>(Identity.None).AsSpan(0, table.Count);
            var s4 = table.GetStorage<C4>(Identity.None).AsSpan(0, table.Count);
            for (var i = 0; i < table.Count; i++) action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], uniform);
        }

        World.Unlock();
    }


    public void RunParallel<U>(RefAction_CCCCU<C1, C2, C3, C4, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        World.Lock();
        using var countdown = new CountdownEvent(1);

        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var storage1 = table.GetStorage<C1>(Identity.None);
            var storage2 = table.GetStorage<C2>(Identity.None);
            var storage3 = table.GetStorage<C3>(Identity.None);
            var storage4 = table.GetStorage<C4>(Identity.None);
            var length = table.Count;

            var partitions = Math.Max(length / chunkSize, 1);
            var partitionSize = length / partitions;

            for (var partition = 0; partition < partitions; partition++)
            {
                countdown.AddCount();

                ThreadPool.QueueUserWorkItem(delegate(int part)
                {
                    var s1 = storage1.AsSpan(part * partitionSize, partitionSize);
                    var s2 = storage2.AsSpan(part * partitionSize, partitionSize);
                    var s3 = storage3.AsSpan(part * partitionSize, partitionSize);
                    var s4 = storage4.AsSpan(part * partitionSize, partitionSize);

                    for (var i = 0; i < s1.Length; i++)
                    {
                        action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], uniform);
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    countdown.Signal();
                }, partition, preferLocal: true);
            }

            /*
            //Optimization: Also process one partition right here on the calling thread.
            var s1 = storage1.AsSpan(0, partitionSize);
            var s2 = storage2.AsSpan(0, partitionSize);
            var s3 = storage3.AsSpan(0, partitionSize);
            var s4 = storage4.AsSpan(0, partitionSize);
            for (var i = 0; i < partitionSize; i++)
            {
                action(ref s1[i], ref s2[i], ref s3[i], ref s4[i], uniform);
            }
            */
        }

        countdown.Signal();
        countdown.Wait();
        World.Unlock();

    }


    public void Run(SpanAction_CCCC<C1, C2, C3, C4> action)
    {
        World.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var s1 = table.GetStorage<C1>(Identity.None).AsSpan(0, table.Count);
            var s2 = table.GetStorage<C2>(Identity.None).AsSpan(0, table.Count);
            var s3 = table.GetStorage<C3>(Identity.None).AsSpan(0, table.Count);
            var s4 = table.GetStorage<C4>(Identity.None).AsSpan(0, table.Count);
            action(s1, s2, s3, s4);
        }

        World.Unlock();
    }

    public void Raw(Action<Memory<C1>, Memory<C2>, Memory<C3>, Memory<C4>> action)
    {
        World.Lock();
        foreach (var table in Tables)
        {
            if (table.IsEmpty) continue;
            var m1 = table.GetStorage<C1>(Identity.None).AsMemory(0, table.Count);
            var m2 = table.GetStorage<C2>(Identity.None).AsMemory(0, table.Count);
            var m3 = table.GetStorage<C3>(Identity.None).AsMemory(0, table.Count);
            var m4 = table.GetStorage<C4>(Identity.None).AsMemory(0, table.Count);
            action(m1, m2, m3, m4);
        }

        World.Unlock();
    }

    public void RawParallel(Action<Memory<C1>, Memory<C2>, Memory<C3>, Memory<C4>> action)
    {
        World.Lock();
        Parallel.ForEach(Tables, Options,
            table =>
            {
                if (table.IsEmpty) return; //TODO: This wastes a scheduled thread.
                var m1 = table.GetStorage<C1>(Identity.None).AsMemory(0, table.Count);
                var m2 = table.GetStorage<C2>(Identity.None).AsMemory(0, table.Count);
                var m3 = table.GetStorage<C3>(Identity.None).AsMemory(0, table.Count);
                var m4 = table.GetStorage<C4>(Identity.None).AsMemory(0, table.Count);
                action(m1, m2, m3, m4);
            });

        World.Unlock();
    }

    #endregion
}