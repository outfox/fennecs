using fennecs.CRUD;

namespace fennecs.storage;

/// <summary>
/// A fast <c>ref readonly</c> style reference to an Entity. It can be used to interact and modify components on the Entity. 
/// </summary>
/// <remarks>
/// Implicitly casts to and from <see cref="fennecs.Entity"/> if you need to store or compare with actual variables of that type.
/// </remarks>

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
    public Entity Add<C>(Key key = default) where C : notnull, new() => Entity.Add(new C(), key);

    /// <inheritdoc />
    public Entity Remove<C>(Key key = default) where C : notnull => Entity.Remove<C>(key);

    /// <inheritdoc />
    public Entity Remove(TypeExpression expression) => Entity.Remove(expression);

    /// <inheritdoc />
    public Entity Link<L>(L link) where L : class => Entity.Link(link);

    
    #region IHasComponent

    /// <inheritdoc />
    public bool Has<C>(Key key = default) where C : notnull => Entity.Has<C>(key);

    /// <inheritdoc />
    public bool Has<C>(Match match = default) where C : notnull => Entity.Has<C>(match);

    /// <inheritdoc />
    public bool Has(Type type, Key key = default) => Entity.Has(type, key);

    /// <inheritdoc />
    public bool Has(Type type, Match match = default) => Entity.Has(type, match);

    /// <inheritdoc />
    public bool Has(MatchExpression expression) => Entity.Has(expression);
    
    /// <inheritdoc />
    public bool Has(TypeExpression expression) => Entity.Has(expression);

    /// <inheritdoc />
    public bool Has<L>(L linkedObject) where L : class => Entity.Has<L>(Key.Of(linkedObject));

    #endregion
}