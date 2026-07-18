using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A builder than spawns Entities after adding pre-configured Components.
/// </summary>
/// <remarks>
/// Call <see cref="Spawn(int)"/> to actually spawn the Entities.
/// </remarks>
public sealed class EntityTemplate : IDisposable, IAddRemove<EntityTemplate>
{
    #region Internals

    private readonly World _world;

    private PooledList<TypeExpression> _components = PooledList<TypeExpression>.Rent();
    private PooledList<object> _values = PooledList<object>.Rent();

    private bool _disposed;
    private bool _consumed;

    internal EntityTemplate(World world)
    {
        _world = world;
    }

    private void AssertMutable() => ObjectDisposedException.ThrowIf(_disposed || _consumed, this);

    private EntityTemplate AddComponent(TypeExpression type, object value)
    {
        AssertMutable();
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

    private EntityTemplate RemoveComponent(TypeExpression type)
    {
        AssertMutable();
        _values.RemoveAt(_components.IndexOf(type));
        _components.Remove(type);
        return this;
    }

    #endregion

    
    /// <inheritdoc cref="Entity.Add{T}()"/>
    /// <summary> Adds a Component of the given type to the Template's configuration state.
    /// If the EntityTemplate already contains a Component of the same type, it will be replaced.
    /// </summary>
    /// <returns>EntityTemplate (fluent interface)</returns>
    public EntityTemplate Add<T>(T component) where T : notnull
    {
        var type = Comp<T>.Plain.Expression;
        return AddComponent(type, component);
    }

    /// <inheritdoc />
    public EntityTemplate Add<T>(Entity target) where T : notnull, new() => Add(new T(), target);


    /// <inheritdoc />
    public EntityTemplate Add<C>() where C : notnull, new() => Add(new C());
    
    
    /// <inheritdoc />
    public EntityTemplate Add<R>(R value, Entity relation) where R : notnull
    {
        var type = TypeExpression.Of<R>(relation);
        return AddComponent(type, value);
    }
    
    
    /// <inheritdoc cref="Entity.Add{T}(Link{T})"/>
    public EntityTemplate Add<T>(Link<T> target) where T : class
    {
        var type = TypeExpression.Of<T>(target);
        return AddComponent(type, target.Object);
    }
    

    /// <inheritdoc cref="Entity.Remove{C}()"/>
    /// <summary>
    /// Removes the plain Component of the given type from the Template.
    /// </summary>
    /// <returns>EntityTemplate (fluent interface)</returns>
    public EntityTemplate Remove<T>() where T : notnull
    {
        var type = Comp<T>.Plain.Expression;
        return RemoveComponent(type);
    }

    /// <inheritdoc cref="Entity.Remove{C}()"/>
    /// <summary>
    /// Removes the Relation Component of the given type from the Template.
    /// </summary>
    /// <returns>EntityTemplate (fluent interface)</returns>
    public EntityTemplate Remove<T>(Entity entity) where T : notnull
    {
        var type = TypeExpression.Of<T>(entity);
        return RemoveComponent(type);
    }
    
    /// <inheritdoc />
    public EntityTemplate Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));
    
    /// <inheritdoc cref="Entity.Remove{C}()"/>
    /// <summary>
    /// Removes the Object Link component to the given Object from the Template.
    /// </summary>
    /// <returns>EntityTemplate (fluent interface)</returns>
    public EntityTemplate Remove<L>(Link<L> link) where L : class
    {
        return RemoveComponent(link.TypeExpression);
    }
    
    /// <summary>
    /// Declares a required plain Component of type <typeparamref name="C0"/>: every <c>Spawn</c> on the
    /// resulting template must provide a value for it, enforced at compile time.
    /// </summary>
    /// <remarks>
    /// Consumes this template and returns a wider <see cref="EntityTemplate{C0}"/> that inherits its
    /// configuration. The consumed template must not be used afterwards; disposing it is a no-op.
    /// </remarks>
    /// <exception cref="InvalidOperationException">if a Component of the same Type Expression is already baked in via Add</exception>
    public EntityTemplate<C0> Needs<C0>() where C0 : notnull => Needs<C0>(Comp<C0>.Plain.Expression, Match.Plain);

    /// <summary>
    /// Declares a required Relation Component of type <typeparamref name="C0"/> targeting <paramref name="relation"/>:
    /// the target is fixed in the template, the backing value is provided at <c>Spawn</c>.
    /// </summary>
    /// <inheritdoc cref="Needs{C0}()"/>
    public EntityTemplate<C0> Needs<C0>(Entity relation) where C0 : notnull => Needs<C0>(TypeExpression.Of<C0>(relation), Match.Relation(relation));

    private EntityTemplate<C0> Needs<C0>(TypeExpression expression, Match match) where C0 : notnull
    {
        AssertMutable();
        if (_components.Contains(expression))
            throw new InvalidOperationException($"Component {expression} is already configured on this template. Remove it first, or provide it at Spawn instead.");

        _consumed = true;
        var (components, values) = (_components, _values);
        _components = null!;
        _values = null!;
        return new EntityTemplate<C0>(_world, components, values, [expression], [match]);
    }

    /// <summary>
    /// Spawns a single Entity with the configured Components.
    /// </summary>
    /// <returns>the spawned Entity</returns>
    public Entity Spawn()
    {
        AssertMutable();
        Span<Entity> spawned = stackalloc Entity[1];
        _world.Spawn(spawned, _components, _values);
        return spawned[0];
    }

    /// <summary>
    /// Spawns <c>count</c> Entities with the configured Components.
    /// </summary>
    /// <param name="count">number of Entities to spawn</param>
    /// <returns>EntityTemplate (fluent interface)</returns>
    public EntityTemplate Spawn(int count)
    {
        AssertMutable();
        _world.Spawn(count, _components, _values);
        return this;
    }

    /// <summary>
    /// Spawns one Entity per element of <paramref name="destination"/> with the configured
    /// Components, and writes their handles into the span.
    /// </summary>
    /// <remarks>
    /// The handles remain valid indefinitely (until despawned) - they are plain
    /// <see cref="Entity"/> values, not views into World storage.
    /// </remarks>
    /// <param name="destination">span to fill; its length determines the number of Entities spawned</param>
    /// <returns>EntityTemplate (fluent interface)</returns>
    public EntityTemplate Spawn(Span<Entity> destination)
    {
        AssertMutable();
        _world.Spawn(destination, _components, _values);
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_consumed) return; // ownership moved to a wider template via Needs

        ObjectDisposedException.ThrowIf(_disposed, this);
        _disposed = true;

        _components.Dispose();
        _components = null!;

        _values.Dispose();
        _values = null!;
    }

}
