namespace fennecs.pools;

public class IdentityPool(int capacity = 4096)
{
    public int Living { get; private set; }
    public int Count => Living - _recycled.Count;

    private readonly Queue<Entity> _recycled = new(capacity);

    public Entity Spawn()
    {
        if (_recycled.TryDequeue(out var recycledIdentity)) return recycledIdentity;
        
        return new Entity(++Living);
    }

    public void Despawn(Entity entity)
    {
        _recycled.Enqueue(entity.Successor);
    }
}