using System.Runtime.CompilerServices;
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
    
    /// <inheritdoc />
    public Entity Add<C>() where C : notnull, new() => Entity.Add<C>();

    /// <inheritdoc />
    public Entity Add<C>(C component) where C : notnull => Entity.Add(component);

    /// <inheritdoc />
    public Entity Add<T>(Entity target) where T : notnull, new() => Entity.Add(target);

    /// <inheritdoc />
    public Entity Add<R1>(R1 component, Entity relation) where R1 : notnull => Entity.Add(component, relation);

    /// <inheritdoc />
    public Entity Add<L>(Link<L> link) where L : class => Entity.Add(link);

    /// <inheritdoc />
    public Entity Remove<C>(Match match = default) where C : notnull => Entity.Remove<C>(match);

    /// <inheritdoc />
    public Entity Remove<R1>(Entity relation) where R1 : notnull => Entity.Remove<R1>(relation);

    /// <inheritdoc />
    public Entity Remove<L>(L linkedObject) where L : class => Entity.Remove(linkedObject);

    /// <inheritdoc />
    public Entity Remove<L>(Link<L> link) where L : class => Entity.Remove(link);

    /// <inheritdoc cref="Entity.Despawn"/>
    public void Despawn() => Entity.Despawn();

    /// <inheritdoc />
    public override string ToString() => Entity.ToString();

    /// <inheritdoc />
    public bool Has<C>() where C : notnull
    {
        return Entity.Has<C>();
    }

    /// <inheritdoc />
    public bool Has<R1>(Entity relation) where R1 : notnull
    {
        return Entity.Has<R1>(relation);
    }

    /// <inheritdoc />
    public bool Has<L>(L linkedObject) where L : class
    {
        return Entity.Has(linkedObject);
    }

    /// <inheritdoc />
    public bool Has<L>(Link<L> link) where L : class
    {
        return Entity.Has(link);
    }
    
    /// <inheritdoc cref="Entity.Alive"/>
    public bool Alive => Entity.Alive;
}
