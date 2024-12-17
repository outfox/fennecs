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

    #endregion


    /// <inheritdoc cref="IAddRemove{SELF}.Add{C}(C, fennecs.Key)" />
    public EntitySpawner Add<C>(C component, Key key = default) where C : notnull
    {
        var type = TypeExpression.Of<C>(key);
        if (_components.Contains(type))
        {
            // replace existing value
            _values[_components.IndexOf(type)] = component;
        }
        else
        {
            // add new value
            _components.Add(type);
            _values.Add(component);
        }

        return this;
    }

    /// <inheritdoc cref="IAddRemove{SELF}.Add{C}(fennecs.Key)" />
    public EntitySpawner Add<C>(Key key = default) where C : notnull, new() => Add<C>(new(), key);


    /// <inheritdoc />
    public EntitySpawner Remove(TypeExpression expression)
    {
        _values.RemoveAt(_components.IndexOf(expression));
        _components.Remove(expression);
        
        return this;
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