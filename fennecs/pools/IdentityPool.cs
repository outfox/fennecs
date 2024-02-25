namespace fennecs.pools;

public class IdentityPool(int capacity = 4096)
{
    public int Living { get; private set; }
    public int Count => Living - _recycled.Count;

    private readonly Queue<Identity> _recycled = new(capacity);


    public Identity Spawn()
    {
        if (_recycled.TryDequeue(out var recycledIdentity)) return recycledIdentity;

        return new Identity(++Living);
    }


    public void Despawn(Identity identity)
    {
        _recycled.Enqueue(identity.Successor);
    }
}