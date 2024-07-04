using fennecs.pools;

namespace fennecs;

/// <summary>
/// A builder than spawns Entities after adding pre-configured components.
/// </summary>
/// <remarks>
/// Call <see cref="Spawn"/> to actually spawn the Entities.
/// </remarks>
public sealed class EntitySpawner : IDisposable, IAddRemoveComponent<EntitySpawner>
{
    #region Internals

    private readonly World _world;

    private PooledList<TypeExpression> _components = PooledList<TypeExpression>.Rent();
    private PooledList<object> _values = PooledList<object>.Rent();

    private bool _disposed;

    internal EntitySpawner(World world)
    {
        _world = world;
    }

    private EntitySpawner AddComponent(TypeExpression type, object value)
    {
        if (_components.Contains(type))
        {
            // replace existing value
            _values[_components.IndexOf(type)] = value;
        }
        else
        {
            // add new value
            _components.Add(type);
            _values.Add(value);
        }
        return this;
    }

    private EntitySpawner RemoveComponent(TypeExpression type)
    {
        _values.RemoveAt(_components.IndexOf(type));
        _components.Remove(type);
        return this;
    }

    #endregion

    
    /// <inheritdoc cref="Entity.Add{T}()"/>
    /// <summary> Adds a component of the given type to the Spawner's configuration state.
    /// If the EntitySpawner already contains a component of the same type, it will be replaced.
    /// </summary>
    /// <returns>EntitySpawner (fluent interface)</returns>
    public EntitySpawner Add<T>(T component) where T : notnull
    {
        var type = Comp<T>.Plain.Expression;
        return AddComponent(type, component);
    }

    /// <inheritdoc />
    public EntitySpawner Add<T>(Entity target) where T : notnull, new() => Add(new T(), target);


    /// <inheritdoc />
    public EntitySpawner Add<C>() where C : notnull, new() => Add(new C());
    
    
    /// <inheritdoc />
    public EntitySpawner Add<R>(R value, Entity relation) where R : notnull
    {
        var type = TypeExpression.Of<R>(relation);
        return AddComponent(type, value);
    }
    
    
    /// <inheritdoc cref="Entity.Add{T}(Link{T})"/>
    public EntitySpawner Add<T>(Link<T> target) where T : class
    {
        var type = TypeExpression.Of<T>(target);
        return AddComponent(type, target.Object);
    }
    

    /// <inheritdoc cref="Entity.Remove{C}()"/>
    /// <summary>
    /// Removes the plain component of the given type from the Spawner.
    /// </summary>
    /// <returns>EntitySpawner (fluent interface)</returns>
    public EntitySpawner Remove<T>() where T : notnull
    {
        var type = Comp<T>.Plain.Expression;
        return RemoveComponent(type);
    }

    /// <inheritdoc cref="Entity.Remove{C}()"/>
    /// <summary>
    /// Removes the Relation component of the given type from the Spawner.
    /// </summary>
    /// <returns>EntitySpawner (fluent interface)</returns>
    public EntitySpawner Remove<T>(Entity entity) where T : notnull
    {
        var type = TypeExpression.Of<T>(entity);
        return RemoveComponent(type);
    }
    
    /// <inheritdoc />
    public EntitySpawner Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));
    
    /// <inheritdoc cref="Entity.Remove{C}()"/>
    /// <summary>
    /// Removes the Object Link component to the given Object from the Spawner.
    /// </summary>
    /// <returns>EntitySpawner (fluent interface)</returns>
    public EntitySpawner Remove<L>(Link<L> link) where L : class
    {
        return RemoveComponent(link.TypeExpression);
    }
    
    /// <summary>
    /// Spawns <c>count</c> entities with the configured components.
    /// </summary>
    /// <param name="count">number of entities to spawn</param>
    public EntitySpawner Spawn(int count = 1)
    {
        _world.Spawn(count, _components, _values);
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _disposed = true;

        _components.Dispose();
        _components = null!;

        _values.Dispose();
        _values = null!;
    }

}
