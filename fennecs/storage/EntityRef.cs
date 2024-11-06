using fennecs.CRUD;

namespace fennecs.storage;

/// <summary>
/// A fast, reference to an Entity. Implicitly casts to and from <see cref="Entity"/>, and exposes <see cref="IAddRemove{T}"/> methods and others.
/// </summary>
public readonly ref struct EntityRef(ref readonly Entity entity) : IEntity
{
    private readonly ref readonly Entity _entity = ref entity;
    
    /// <summary>
    /// Implicitly casts a <see cref="EntityRef"/> to its underlying <see cref="fennecs.Entity"/>.
    /// (to store or compare with other Entities)
    /// </summary>
    public static implicit operator Entity(EntityRef self) => self._entity;
    
    /// <summary>
    /// Implicitly casts a <see cref="EntityRef"/> to its underlying <see cref="fennecs.Entity"/>.
    /// (to store or compare with other Entities)
    /// </summary>
    public static implicit operator EntityRef(in Entity entity) => new(in entity);

    /// <inheritdoc />
    public Entity Add<C>() where C : notnull, new() => _entity.Add<C>();

    /// <inheritdoc />
    public Entity Add<C>(C component) where C : notnull => _entity.Add(component);

    /// <inheritdoc />
    public Entity Add<T>(Entity target) where T : notnull, new() => _entity.Add(target);

    /// <inheritdoc />
    public Entity Add<R1>(R1 component, Entity relation) where R1 : notnull => _entity.Add(component, relation);

    /// <inheritdoc />
    public Entity Add<L>(Link<L> link) where L : class => _entity.Add(link);

    /// <inheritdoc />
    public Entity Remove<C>(Match match = default) where C : notnull => _entity.Remove<C>(match);

    /// <inheritdoc />
    public Entity Remove<R1>(Entity relation) where R1 : notnull => _entity.Remove<R1>(relation);

    /// <inheritdoc />
    public Entity Remove<L>(L linkedObject) where L : class => _entity.Remove(linkedObject);

    /// <inheritdoc />
    public Entity Remove<L>(Link<L> link) where L : class => _entity.Remove(link);

    /// <inheritdoc cref="Entity.Despawn"/>
    public void Despawn() => _entity.Despawn();
}
