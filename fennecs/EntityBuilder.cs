using fennecs.pools;

namespace fennecs;

/// <summary>
/// Provides a fluent interface for constructing and modifying Entities within a world.
/// </summary>
public readonly struct EntityBuilder(World world, Entity entity) : IDisposable
{
    private readonly PooledList<World.DeferredOperation> _operations = PooledList<World.DeferredOperation>.Rent();

    /// <summary>
    /// Adds a relation of a specific type, with specific data, between the current entity and the target entity.
    /// The relation is backed by the Component data of the relation. Entities with the same relations are placed
    /// in the same Archetype for faster enumeration and processing as a group.
    ///
    /// The Component data is instantiated / initialized via the default constructor of the relation type.
    /// </summary>
    /// <typeparam name="T">Any value or reference type. The type of the relation to be added.</typeparam>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <param name="targetEntity">The entity with which to establish the relation.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder AddRelation<T>(Entity targetEntity) where T : notnull, new() => AddRelation(targetEntity, new T());

    /// <summary>
    /// Adds a relation of a specific type, with specific data, between the current entity and the target entity.
    /// The relation is backed by the Component data of the relation. Entities with the same relations are placed
    /// in the same Archetype for faster enumeration and processing as a group.
    /// </summary>
    /// <typeparam name="T">Any value or reference type. The type of the relation to be added.</typeparam>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <param name="targetEntity">The entity with which to establish the relation.</param>
    /// <param name="data">The data associated with the relation.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder AddRelation<T>(Entity targetEntity, T data)
    {
        if (!targetEntity.IsEntity) throw new InvalidOperationException("May only relate to a virtual entity.");
        world.AddRelation(entity, targetEntity, data);
        return this;
    }

    /// <summary>
    /// Adds a object link to the current entity.
    /// Object links, in addition to making the object available as a Component,
    /// place all Entities with a link to the same object into a single Archetype,
    /// which can optimize processing them in queries.
    /// </summary>
    /// <remarks>
    /// Beware of Archetype fragmentation! 
    /// You can end up with a large number of Archetypes with few Entities in them,
    /// which negatively impacts processing speed and memory usage.
    /// Try to keep the size of your Archetypes as large as possible for maximum performance.
    /// </remarks>
    /// <typeparam name="T">Any reference type. The type the object to be linked with the entity.</typeparam>
    /// <param name="target">The target of the link.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder AddLink<T>(T target) where T : class
    {
        world.AddLink(entity, target);
        return this;
    }

    /// <summary>
    /// Adds a Component of a specific type, with specific data, to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <param name="data">The data associated with the Component.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder Add<T>(T data)
    {
        world.AddComponent(entity, data);
        return this;
    }

    /// <summary>
    /// Adds a Component of a specific type to the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be added.</typeparam>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder Add<T>() where T : new() => Add(new T());

    
    /// <summary>
    /// Removes a Component of a specific type from the current entity.
    /// </summary>
    /// <typeparam name="T">The type of the Component to be removed.</typeparam>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder Remove<T>() 
    {
        world.RemoveComponent<T>(entity);
        return this;
    }

    /// <summary>
    /// Removes a relation of a specific type between the current entity and the target entity.
    /// </summary>
    /// <typeparam name="T">The type of the relation to be removed.</typeparam>
    /// <param name="targetEntity">The entity from which the relation will be removed.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder RemoveRelation<T>(Entity targetEntity)
    {
        world.RemoveRelation<T>(entity, targetEntity);
        return this;
    }

    /// <summary>
    /// Removes the link of a specific type with the target object.
    /// </summary>
    /// <typeparam name="T">The type of the link to be removed.</typeparam>
    /// <param name="targetObject">The target object from which the link will be removed.</param>
    /// <returns>The current instance of EntityBuilder, allowing for method chaining.</returns>
    public EntityBuilder RemoveLink<T>(T targetObject) where T : class
    {
        world.RemoveLink(entity, targetObject);
        return this;
    }

    /// <summary>
    /// Completes the building process, returns the entity, and disposes of the builder.
    /// </summary>
    /// <returns>The built or modified entity.</returns>
    public Entity Id()
    {
        Dispose();
        return entity;
    }

    /// <summary>
    /// Disposes of the EntityBuilder, releasing any pooled resources.
    /// </summary>
    public void Dispose()
    {
        _operations.Dispose();
    }
}
