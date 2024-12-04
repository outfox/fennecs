using System.Runtime.CompilerServices;

namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can express the presence of components on an entity or set of entities.
/// </summary>
public interface IHasComponent
{
    /// <summary>
    /// Check for presence of the component of the specified type, with an optional secondary key.
    /// </summary>
    /// <returns>true if the entity/entities has the component; otherwise, false.</returns>
    [OverloadResolutionPriority(1)]
    public bool Has<C>(Key key = default) where C : notnull;

    /// <summary>
    /// Check for presence of the component of the specified type, with an optional secondary key.
    /// </summary>
    /// <returns>true if the entity/entities has the component; otherwise, false.</returns>
    [OverloadResolutionPriority(1)]
    public bool Has(Type type, Key key = default);

    /// <summary>
    /// Check for presence of one or more component of the specified type, matching the given match expression.
    /// </summary>
    /// <returns>true if the entity/entities has one or more matching component; otherwise, false.</returns>
    public bool Has<C>(Match match) where C : notnull;

    /// <summary>
    /// Check for presence of one or more component of the specified type, matching the given match expression.
    /// </summary>
    /// <returns>true if the entity/entities has one or more matching component; otherwise, false.</returns>
    public bool Has(Type type, Match match);

    #region Convenience Defaults
    /// <summary>
    /// Check for presence of the Object Link component with the specified linked object.
    /// </summary>
    /// <returns>true if the entity/entities has the component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class => Has<L>(Key.Of(linkedObject));
    
    
    #endregion
}
