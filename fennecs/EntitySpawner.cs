using fennecs.pools;

namespace fennecs;

/// <summary>
/// A builder than spawns Entities after adding pre-configured components.
/// </summary>
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
        if (_components.Contains(type)) throw new InvalidOperationException($"Component of Type {type} already added.");
        _components.Add(type);
        _values.Add(value);
        return this;
    }
    
    /// <inheritdoc cref="Entity.Add{T}()"/>
    public EntitySpawner Add<T>(T component) where T : notnull
    {
        var type = TypeExpression.Of<T>();
        return AddComponent(type, component);
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
    /// <param name="dispose">dispose the spawner after use</param>
    public void Spawn(int count = 1, bool dispose = true)
    {
        _world.Spawn(count, _components, _values);
        if (dispose) Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _components.Dispose();
        _values.Dispose();
    }
}
