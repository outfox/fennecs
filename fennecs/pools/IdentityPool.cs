namespace fennecs.pools;

/// <summary>
/// Generation and Entity management for Entities
/// </summary>
/// <remarks>
/// Not thread-safe (yet?). This is the responsibility of the World that uses the pool.
/// </remarks>
internal sealed class IdentityPool
{
    internal uint Created { get; private set; }

    internal int Alive => (int) (Created - _recycled.Count);

    private readonly Queue<Entity.Id> _recycled;

    private readonly World.Id _world;

    private uint NextId => _world.Tag | Created++;

    public IdentityPool(World.Id world, int initialCapacity)
    {
        _world = world;
        _recycled = new(initialCapacity);
        for (var i = 0; i < initialCapacity; i++) _recycled.Enqueue(new(NextId));
    }

    internal Entity.Id Spawn()
    { 
        return _recycled.TryDequeue(out var recycledEntity)
            ? recycledEntity 
            : new(NextId);
    }

    internal PooledList<Entity.Id> Spawn(int amount)
    {
        var identities = PooledList<Entity.Id>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= amount)
        {
            // Reuse all entities in the recycler.
            identities.AddRange(_recycled);
            _recycled.Clear();

            // If we didn't have enough recycled Identities, create more.
            var remainder = amount - recycled;
            for (var i = 0; i < remainder; i++)
            {
                identities.Add(new(NextId));
            }
        }
        else
        {
            // Otherwise, take the requested amount from the recycled pool.
            for (var i = 0; i < amount; i++)
            {
                //TODO: Optimize this! Maybe with a range copy? (needs custom queue)
                identities.Add(_recycled.Dequeue());
            }
        }

        return identities;
    }
    
    internal void Recycle(Entity.Id id) => _recycled.Enqueue(id);
}