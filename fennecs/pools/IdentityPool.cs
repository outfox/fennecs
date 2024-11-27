using System.Diagnostics;

namespace fennecs.pools;

/// <summary>
/// Generation and Identity management for Entities
/// </summary>
/// <remarks>
/// Not thread-safe (yet?). This is the responsibility of the World that uses the pool.
/// </remarks>
internal sealed class IdentityPool
{
    internal int Created { get; private set; }

    internal int Alive => Created - _recycled.Count;

    private readonly Queue<Identity> _recycled;

    private readonly byte _worldId;

    private int NewIndex => ++Created;

    public IdentityPool(byte worldId, int initialCapacity)
    {
        _worldId = worldId;
        _recycled = new(initialCapacity * 2);
        for (var i = 0; i < initialCapacity; i++) _recycled.Enqueue(new(_worldId, NewIndex));
    }


    internal Identity Spawn()
    { 
        return _recycled.TryDequeue(out var recycledIdentity)
            ? recycledIdentity
            : new(NewIndex, _worldId);
    }

    internal PooledList<Identity> Spawn(int count)
    {
        var identities = PooledList<Identity>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= count)
        {
            // Reuse all entities in the recycler.
            identities.AddRange(_recycled);
            _recycled.Clear();

            // If we don't have enough recycled Identities, create more.
            for (var i = 0; i < count - recycled; i++)
            {
                identities.Add(new(NewIndex));
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


    internal void Recycle(Identity identity) => _recycled.Enqueue(identity.Successor);

    internal void Recycle(ReadOnlySpan<Identity> toDelete)
    {
        foreach (var identity in toDelete) Recycle(identity);
    }
}