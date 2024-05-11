namespace fennecs.pools;

internal class IdentityPool(int capacity = 4096)
{
    internal int Created { get; private set; }
    internal int Count => Created - _recycled.Count;

    private readonly Queue<Identity> _recycled = new(capacity);


    internal Identity Spawn()
    {
        return _recycled.TryDequeue(out var recycledIdentity) ? recycledIdentity : new Identity(++Created);
    }


    internal PooledList<Identity> Spawn(int requested)
    {
        var identities = PooledList<Identity>.Rent();
        var recycled = _recycled.Count;
        if (recycled <= requested)
        {
            identities.AddRange(_recycled);
            _recycled.Clear();
        }
        else
        {
            identities.AddRange(Enumerable.Range(0, requested - recycled).Select(_ => new Identity(++Created)));
        }

        return identities;
        //return _recycled.TryDequeue(out var recycledIdentity) ? recycledIdentity : new Identity(++Created);
    }


    internal void Recycle(Identity identity)
    {
        _recycled.Enqueue(identity.Successor);
    }
}