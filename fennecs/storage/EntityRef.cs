using fennecs.CRUD;

namespace fennecs.storage;

/// <summary>
/// A fast, reference to an Entity. Implicitly casts to and from <see cref="fennecs.Entity"/>, and exposes <see cref="IAddRemove{T}"/> methods and others.
/// </summary>
public readonly ref struct EntityRef(ref readonly Entity entity) : IEntity
{
    internal readonly ref readonly Entity Entity = ref entity;
    
    /// <inheritdoc />
    public bool Equals(Entity other) => Entity.Equals(other);

    /// <summary>
    /// Implicitly casts a <see cref="EntityRef"/> to its underlying <see cref="fennecs.Entity"/>.
    /// (to store or compare with other Entities)
    /// </summary>
    public static implicit operator Entity(EntityRef self) => self.Entity;
    
    /// <inheritdoc cref="Entity.Alive"/>
    public bool Alive => Entity.Alive;
 
    /// <inheritdoc />
    public void Despawn() => Entity.Despawn();

    /// <inheritdoc />
    public IReadOnlyList<Component> Components => Entity.Components;

    /// <inheritdoc />
    public Entity Add<C>(C component, Key key = default) where C : notnull => Entity.Add(component, key);

    /// <inheritdoc />
    public Entity Remove<C>(Key key = default) where C : notnull => Entity.Remove<C>(key);

    /// <inheritdoc />
    public Entity Add<C>(Key key = default) where C : notnull, new() => Entity.Add(key);

    /// <inheritdoc />
    public Entity Relate<R1>(Entity target) where R1 : notnull, new() => Entity.Relate<R1>(new(), target);

    /// <inheritdoc />
    public Entity Relate<R1>(R1 component, Entity target) where R1 : notnull => Entity.Relate(component, target);

    /// <inheritdoc />
    public Entity Unrelate<R1>(Entity target) where R1 : notnull => Entity.Unrelate<R1>(target);

    /// <inheritdoc />
    public Entity Link<L>(L link) where L : class => Entity.Link(link);

    /// <inheritdoc />
    public Entity Unlink<L>(L link) where L : class => Entity.Unlink(link);

    /// <inheritdoc />
    public bool Has<C>(Key key = default) where C : notnull => Entity.Has<C>(key);

    /// <inheritdoc />
    public bool Has<R1>(Entity relation) where R1 : notnull => Entity.Has<R1>(relation.Key);

    /// <inheritdoc />
    public bool Has<L>(L linkedObject) where L : class => Entity.Has<L>(Key.Of(linkedObject));

    /// <inheritdoc />
    public bool Has<L>(Link<L> link) where L : class => Entity.Has<L>(Key.Of(link));
}
