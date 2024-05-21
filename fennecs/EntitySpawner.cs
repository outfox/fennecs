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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="component"></param>
    /// <typeparam name="T"></typeparam>
    public EntitySpawner Add<T>(T component) where T : notnull
    {
        _components.Add(TypeExpression.Of<T>());
        _values.Add(component);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="component"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public EntitySpawner AddRelation<T>(T component, Identity target) where T : class
    {
        _components.Add(TypeExpression.Of<T>(target));
        _values.Add(component);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public EntitySpawner AddLink<T>(T target) where T : class
    {
        _components.Add(TypeExpression.Link(target));
        _values.Add(target);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public EntitySpawner Spawn(int count = 1)
    {
        _world.Spawn(count, _components, _values);
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _components.Dispose();
        _values.Dispose();
    }
}
