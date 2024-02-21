using fennecs.pools;

namespace fennecs;

public readonly struct EntityBuilder(World world, Entity entity) : IDisposable
{
    private readonly PooledList<World.DeferredOperation> _operations = PooledList<World.DeferredOperation>.Rent();

/*
 TODO: Introduce to this pattern.
_operations.Add(
    new World.DeferredOperation()
    {
        Operation = World.Operation.Add,
        IdIdentity = identity,
        Data = target,
    });
*/
    public EntityBuilder Link<T>(Entity target) where T : notnull, new()
    {
        world.Link(entity, target, new T());
        return this;
    }

    public EntityBuilder Link<T>(Entity target, T data)
    {
        world.Link(entity, target, data);
        return this;
    }

    public EntityBuilder Link<T>(T target) where T : class
    {
        world.Link(entity, target);
        return this;
    }

    public EntityBuilder Add<T>(T data)
    {
        world.AddComponent(entity, data);
        return this;
    }


    public EntityBuilder Add<T>() where T : new()
    {
        world.AddComponent(entity, new T());
        return this;
    }

    
    public EntityBuilder Remove<T>() 
    {
        world.RemoveComponent<T>(entity);
        return this;
    }
    
    public EntityBuilder Remove<T>(Entity target) 
    {
        world.Unlink<T>(entity, target);
        return this;
    }
    
    public Entity Id()
    {
        Dispose();
        return entity;
    }

    public void Dispose()
    {
        _operations.Dispose();
    }
}