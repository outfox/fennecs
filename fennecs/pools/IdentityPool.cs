using System.Diagnostics;

namespace fennecs.pools;

/// <summary>
/// Generation and Entity management for Entities
/// </summary>
/// <remarks>
/// Not thread-safe (yet?). This is the responsibility of the World that uses the pool.
/// </remarks>
internal sealed class EntityPool
{
    internal int Created { get; private set; }

    internal int Alive => Created - _recycled.Count;

    private readonly Queue<Entity> _recycled;

    private readonly World.Id _worldId;

    private int NewIndex => ++Created;

    public EntityPool(World.Id worldId, int initialCapacity)
    {
        _worldId = worldId;
        _recycled = new(initialCapacity * 2);
        for (var i = 0; i < initialCapacity; i++) _recycled.Enqueue(new(_worldId, NewIndex));
    }


    internal Entity Spawn()
    { 
        return _recycled.TryDequeue(out var recycledEntity)
            ? recycledEntity
            : new(_worldId, NewIndex);
    }

    internal PooledList<Entity> Spawn(int count)
    {
        var identities = PooledList<Entity>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= count)
        {
            // Reuse all entities in the recycler.
            identities.AddRange(_recycled);
            _recycled.Clear();

            // If we don't have enough recycled Identities, create more.
            for (var i = 0; i < count - recycled; i++)
            {
                identities.Add(new(_worldId, NewIndex));
            }
        }
        else
        {
            // Otherwise, take the requested amount from the recycled pool.
            for (var i = 0; i < count; i++)
            {
                //TODO: Optimize this!
                identities.Add(_recycled.Dequeue());
            }
        }

        return identities;
    }


    internal void Recycle(Entity entity) => _recycled.Enqueue(entity.Successor);

    internal void Recycle(ReadOnlySpan<Entity> toDelete)
    {
        foreach (var entity in toDelete) Recycle(entity);
    }
}