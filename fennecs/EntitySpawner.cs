using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A builder than spawns Entities after adding pre-configured components.
/// </summary>
/// <remarks>
/// Call <see cref="Spawn"/> to actually spawn the Entities.
/// </remarks>
public sealed class EntitySpawner : IDisposable, IAddRemove<EntitySpawner>
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

    
    /// <inheritdoc cref="IAddRemove{SELF}.Add{C}(fennecs.Key)" />
    public EntitySpawner Add<T>(T component, Key key = default) where T : notnull
    {
        var type = TypeExpression.Of<T>(key);
        return AddComponent(type, component);
    }

    /// <inheritdoc cref="IAddRemove{SELF}.Add{C}(C,fennecs.Key)" />
    public EntitySpawner Add<C>(Key key = default) where C : notnull, new() => Add(new C(), key);
    

    /// <inheritdoc cref="IAddRemove{SELF}.Relate{R}(fennecs.Entity)" />
    public EntitySpawner Relate<T>(Entity target) where T : notnull, new() => Add(new T(), target.Key);

    /// <inheritdoc cref="IAddRemove{SELF}.Relate{R}(fennecs.Entity)" />
    public EntitySpawner Relate<T>(T component, Entity target) where T : notnull => Add(component, target.Key);


    
    /// <inheritdoc cref="IAddRemove{SELF}.Remove{R}(fennecs.Key)" />
    public EntitySpawner Remove<T>(Key match = default) where T : notnull
    {
        var type = Comp<T>.Matching(match).Expression;
        return RemoveComponent(type);
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
