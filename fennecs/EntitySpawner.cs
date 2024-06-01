using fennecs.pools;

namespace fennecs;

/// <summary>
/// A builder than spawns Entities after adding pre-configured components.
/// </summary>
/// <remarks>
/// Call <see cref="Spawn"/> to actually spawn the Entities.
/// </remarks>
public sealed class EntitySpawner : IDisposable
{
    private readonly World _world;

    private readonly PooledList<TypeExpression> _components = PooledList<TypeExpression>.Rent();
    private readonly PooledList<object> _values = PooledList<object>.Rent();
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

    /// <inheritdoc cref="Entity.Add{T}()"/>
    /// <summary> Adds a component of the given type to the Spawner's configuration state.
    /// If the EntitySpawner already contains a component of the same type, it will be replaced.
    /// </summary>
    /// <returns>EntitySpawner (fluent interface)</returns>
    public EntitySpawner Add<T>(T component) where T : notnull
    {
        var type = TypeExpression.Of<T>();
        return AddComponent(type, component);
    }

    /// <inheritdoc cref="Entity.Add{T}()"/>
    /// <summary> Adds a component of the given type to the Spawner's configuration state.
    /// If the EntitySpawner already contains a component of the same type, it will be replaced.
    /// </summary>
    /// <returns>EntitySpawner (fluent interface)</returns>
    public EntitySpawner Add<T>() where T : new()
    {
        var type = TypeExpression.Of<T>();
        return AddComponent(type, new T());
    }

    /// <inheritdoc cref="Entity.AddRelation{T}(fennecs.Entity,T)"/>
    public EntitySpawner AddRelation<T>(T component, Identity target) where T : class
    {
        var type = TypeExpression.Of<T>(target);
        return AddComponent(type, component);
    }

    /// <inheritdoc cref="Entity.AddLink{T}"/>
    public EntitySpawner AddLink<T>(T target) where T : class
    {
        var type = TypeExpression.Link(target);
        return AddComponent(type, target);
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

    /// <summary>
    ///  Spawns <c>count</c> entities with the configured components and disposes the spawner.
    /// </summary>
    /// <param name="count">number of entities to spawn</param>
    [Obsolete("ue .Spawn() and .Dispose() instead.]")]
    public void SpawnOnce(int count = 1)
    {
        Spawn(count);
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _components.Dispose();
        _values.Dispose();
    }
}
