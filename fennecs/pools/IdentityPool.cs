﻿namespace fennecs.pools;

internal class IdentityPool
{
    internal int Created => _created;
    internal int Count => _created - _recycled.Count;

    private readonly Queue<Identity> _recycled;
    private int _created;
    
    private readonly byte _worldIndex;

    public IdentityPool(byte worldIndex, int initialCapacity = 65536)
    {
        _worldIndex = worldIndex;
        
        _recycled = new(initialCapacity * 2);
        for (var i = 0; i < initialCapacity; i++)
        {
            _recycled.Enqueue(new(_worldIndex, ++_created, 1));
        }
    }


    internal Identity Spawn()
    {
        if (_recycled.TryDequeue(out var recycledIdentity)) return recycledIdentity;

        var newIndex = Interlocked.Increment(ref _created);
        return new(_worldIndex, newIndex, 1);
    }


    internal PooledList<Identity> Spawn(int requested)
    {
        var identities = PooledList<Identity>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= requested)
        {
            // Reuse all entities in the recycler.
            identities.AddRange(_recycled);
            _recycled.Clear();

            // If we don't have enough recycled Identities, create more.
            for (var i = 0; i < requested - recycled; i++)
            {
                identities.Add(new(_worldIndex, ++_created, 1));
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


    internal void Recycle(Identity identity)
    {
        _recycled.Enqueue(identity.Successor);
    }

    internal void Recycle(ReadOnlySpan<Identity> toDelete)
    {
        foreach (var identity in toDelete) Recycle(identity);
    }
}
