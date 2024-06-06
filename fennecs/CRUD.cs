namespace fennecs;

/// <summary>
/// Objects of this type can perform Add and Remove operations on entities or sets of entities.
/// </summary>
public interface IAddRemoveComponent<out SELF>
{
    /// <summary>
    /// Add a default, Plain newable component of type C to the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>() where C : notnull, new() => Add(new C());

    /// <summary>
    /// Add a Plain component with value of type C to the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(C value) where C : notnull;

    /// <summary>
    /// Add a Relation component backed by a value of type R to the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<R>(R value, Entity relation) where R : notnull;

    /// <summary>
    /// Add a Object Link component with an Object of type L to the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<L>(Link<L> link) where L : class;

    /// <summary>
    /// Remove a Plain component of type C from the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>() where C : notnull;

    /// <summary>
    /// Remove a Relation component of type R with the specified relation from the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<R>(Entity relation) where R : notnull;

    /// <summary>
    /// Remove an Object Link component with the specified linked object from the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<L>(L linkedObject) where L : class => Remove(Link<L>.With(linkedObject));

    /// <summary>
    /// Remove an Object Link component with the specified link from the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<L>(Link<L> link) where L : class;

    /// <summary>
    /// Remove any component that matches the specified Match from the entity.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF RemoveAny(Match match);
}

/// <summary>
/// Objects of this type can express the presence of components on an entity or set of entities.
/// </summary>
public interface IHasComponent<out SELF>
{
    /// <summary>
    /// Check if the entity has a Plain component of type C.
    /// </summary>
    /// <returns>true if the entity has the component; otherwise, false.</returns>
    public bool Has<C>() where C : notnull;

    /// <summary>
    /// Check if the entity has a Relation component of type R with the specified relation.
    /// </summary>
    /// <returns>true if the entity has the component with the specified relation; otherwise, false.</returns>
    public bool Has<R>(Entity relation) where R : notnull;

    /// <summary>
    /// Check if the entity has an Object Link component with the specified linked object.
    /// </summary>
    /// <returns>true if the entity has the component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class => Has(Link<L>.With(linkedObject));

    /// <summary>
    /// Check if the entity has an Object Link component with the specified link.
    /// </summary>
    /// <returns>true if the entity has the component with the specified link; otherwise, false.</returns>
    public bool Has<L>(Link<L> link) where L : class;

    /// <summary>
    /// Check if the entity has any component that matches the specified Match.
    /// </summary>
    /// <returns>true if the entity has any matching component; otherwise, false.</returns>
    public bool HasAny(Match match);
}