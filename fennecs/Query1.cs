// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query<C1>(Archetypes archetypes, Mask mask, List<Table> tables) : Query(archetypes, mask, tables)
{
    private readonly CountdownEvent _countdown = new(1);
    
    public ref C1 this[Entity entity] => ref Ref(entity);

    /// <summary>
    /// Gets a reference to the component of type <typeparamref name="C1"/> for the entity.
    /// </summary>
    /// <param name="entity">The entity to get the component from.</param>
    /// <typeparam name="C1">The type of the component to get.</typeparam>
    /// <returns>A reference to the component of type <typeparamref name="C1"/> for the entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to get a reference to
    /// <see cref="Entity"/> itself, because its immutability is crucial for the integrity of the tables.</exception>
    public ref C1 Ref(Entity entity)
    {
        if (typeof(C1) == typeof(Entity))
        {
            throw new TypeAccessException("Can't request a mutable ref to type <Entity>.");
        }
        return ref archetypes.GetComponent<C1>(entity);
    }


    #region Runners

    public void ForEach(RefAction_C<C1> action)
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

    public void ForEach<U>(RefAction_CU<C1, U> action, U uniform)
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

        using var jobs = PooledList<Work<C1>>.Rent();

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

        Archetypes.Unlock();
    }

    public void Job<U>(RefAction_CU<C1, U> action, U uniform, int chunkSize = int.MaxValue)
    {
        Archetypes.Lock();
        _countdown.Reset();

        using var jobs = PooledList<UniformWork<C1, U>>.Rent();

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