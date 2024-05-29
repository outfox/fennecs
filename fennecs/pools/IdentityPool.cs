namespace fennecs.pools;

internal class IdentityPool
{
    internal int Created { get; private set; }
    internal int Count => Created - _recycled.Count;

    private readonly Queue<Identity> _recycled;

    public IdentityPool(int initialCapacity = 65536)
    {
        _recycled = new(initialCapacity * 2);
        for (var i = 0; i < initialCapacity; i++)
        {
            _recycled.Enqueue(new(++Created));
        }
    }


    internal Identity Spawn()
    {
        return _recycled.TryDequeue(out var recycledIdentity) ? recycledIdentity : new(++Created);
    }


    internal PooledList<Identity> Spawn(int requested)
    {
        var identities = PooledList<Identity>.Rent();
        var recycled = _recycled.Count;

        if (recycled < requested)
        {
            // If we don't have enough recycled Identities, create more.
            identities.AddRange(_recycled);
            _recycled.Clear();

            for (var i = 0; i < requested - recycled; i++)
            {
                identities.Add(new(++Created));
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
        //TODO: Optimize this!
        foreach (var identity in toDelete) Recycle(identity);
    }
}
