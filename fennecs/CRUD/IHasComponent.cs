namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can express the presence of components on an entity or set of entities.
/// </summary>
public interface IHasTyped
{
    /// <summary>
    /// Check if the entity/entities has a Plain component of type C.
    /// </summary>
    /// <returns>true if the entity/entities has the component; otherwise, false.</returns>
    public bool Has<C>(Key key = default) where C : notnull;

    #region Convenience Defaults
    /// <summary>
    /// Check if the entity/entities has a Relation component of type R with the specified relation.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified relation; otherwise, false.</returns>
    public bool Has<R>(Entity relation) where R : notnull => Has<R>(Key.Of(relation));

    /// <summary>
    /// Check if the entity/entities has an Object Link component with the specified linked object.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class => Has<L>(Key.Of(linkedObject));

    /// <summary>
    /// Check if the entity/entities has an Object Link component with the specified link.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified link; otherwise, false.</returns>
    [Obsolete("Use Has<L>(L linkedobject) instead.")]
    public bool Has<L>(Link<L> link) where L : class => Has<L>(link.TypeExpression.Key);
    #endregion
}
