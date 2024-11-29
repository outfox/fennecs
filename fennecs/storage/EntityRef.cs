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
    
    
    /// <inheritdoc cref="Entity.Alive"/>
    public bool Alive => Entity.Alive;
 
    public void Despawn() => Entity.Despawn();
}
