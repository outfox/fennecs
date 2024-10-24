namespace fennecs.pools;

internal class IdentityPool //TODO: Rename to EntityPool
{
    internal int Created => _created;
    internal int Count => _created - _recycled.Count;

    private readonly Queue<Entity> _recycled;
    private int _created;
    
    private readonly byte _worldIndex;

    public IdentityPool(byte worldIndex, int initialCapacity = 65536)
    {
        _worldIndex = worldIndex;
        
        _recycled = new(initialCapacity * 2);
        for (var index = 0; index < initialCapacity; index++)
        {
            _recycled.Enqueue(new(_worldIndex, index));
        }
    }


    internal Entity Spawn()
    {
        if (_recycled.TryDequeue(out var entity)) return entity;

        var newIndex = Interlocked.Increment(ref _created);
        return new(_worldIndex, newIndex);
    }


    internal PooledList<Entity> Spawn(int requested)
    {
        var identities = PooledList<Entity>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= requested)
        {
            // Reuse all entities in the recycler.
            identities.AddRange(_recycled);
            _recycled.Clear();

            // If we don't have enough recycled Identities, create more.
            for (var i = 0; i < requested - recycled; i++)
            {
                identities.Add(new(_worldIndex, Interlocked.Increment(ref _created)));
            }
        }
        else
        {
            // Otherwise, take the requested amount from the recycled pool.
            for (var i = 0; i < requested; i++)
            {
                //TODO: Optimize this!
                identities.Add(_recycled.Dequeue());
            }
        }

        return identities;
    }


    internal void Recycle(Entity entity)
    {
        _recycled.Enqueue(entity.Successor);
    }

    internal void Recycle(ReadOnlySpan<Entity> toDelete)
    {
        foreach (var entity in toDelete) Recycle(entity);
    }
}
