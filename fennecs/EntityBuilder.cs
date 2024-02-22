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
    public EntityBuilder AddRelation<T>(Entity targetEntity) where T : notnull, new()
    {
        world.AddRelation(entity, targetEntity, new T());
        return this;
    }

    public EntityBuilder AddRelation<T>(Entity targetEntity, T data)
    {
        world.AddRelation(entity, targetEntity, data);
        return this;
    }

    public EntityBuilder AddLink<T>(T target) where T : class
    {
        world.AddLink(entity, target);
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

    public EntityBuilder RemoveRelation<T>(Entity targetEntity)
    {
        world.RemoveRelation<T>(entity, targetEntity);
        return this;
    }

    /// <summary>
    /// Removes the Object Link with target.
    /// </summary>
    /// <param name="targetObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public EntityBuilder RemoveLink<T>(T targetObject) where T : class
    {
        world.RemoveLink(entity, targetObject);
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