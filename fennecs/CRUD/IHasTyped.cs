namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can express the presence of components on an entity or set of entities.
/// </summary>
public interface IHasTyped
{
    /// <summary>
    /// Check for presence of an component of the specified type, with an optional secondary key.
    /// </summary>
    /// <returns>true if the entity/entities has the component; otherwise, false.</returns>
    public bool Has<C>(Key key = default) where C : notnull;

    #region Convenience Defaults
    /// <summary>
    /// Check for presence of an Object Link component with the specified linked object.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class => Has<L>(Key.Of(linkedObject));
    #endregion
}
