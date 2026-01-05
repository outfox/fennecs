namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can express the presence of Components on an Entity or set of Entities.
/// </summary>
public interface IHasTyped
{
    /// <summary>
    /// Check if the Entity/Entities has a Plain Component of type C.
    /// </summary>
    /// <returns>true if the Entity/Entities has the Component; otherwise, false.</returns>
    public bool Has<C>() where C : notnull;

    /// <summary>
    /// Check if the Entity/Entities has a Relation Component of type R with the specified relation.
    /// </summary>
    /// <returns>true if the Entity/Entities has the Component with the specified relation; otherwise, false.</returns>
    public bool Has<R>(Entity relation) where R : notnull;

    /// <summary>
    /// Check if the entity/Entities has an Object Link Component with the specified linked object.
    /// </summary>
    /// <returns>true if the entity/Entities has the Component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class;

    /// <summary>
    /// Check if the entity/Entities has an Object Link Component with the specified link.
    /// </summary>
    /// <returns>true if the entity/Entities has the component with the specified link; otherwise, false.</returns>
    public bool Has<L>(Link<L> link) where L : class;
}
