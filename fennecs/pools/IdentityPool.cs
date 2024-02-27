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


    internal void Recycle(Identity identity)
    {
        _recycled.Enqueue(identity.Successor);
    }
}