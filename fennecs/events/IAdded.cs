namespace fennecs.events;


/// <summary>
/// Interface handling events triggered when a component is added to an entity.
/// </summary>
/// <remarks>
/// fennecs may call this called more than once for differetn blocks of entities/components (but only once per change event per Entity)
/// </remarks>
public interface IAdded
{
    /// <summary>
    /// Called when the component has been added to the Entities.
    /// </summary>
    /// <param name="entities">the entities that have the component added</param>
    static abstract void Notify(ReadOnlySpan<Entity> entities);
}


/// <summary>
/// Interface handling events triggered when a component is added to an entity, providing the added values.
/// </summary>
/// <remarks>
/// fennecs may call this called more than once for differetn blocks of entities/components (but only once per change event per Entity)
/// </remarks>
public interface IAddedValues<C> where C : notnull
{
    /// <summary>
    /// Called when the component has been added to the Entities, passing the added values.
    /// </summary>
    /// <param name="entities">the entities that have the component added</param>
    /// <param name="values">the added values</param>
    /// <param name="key">secondarykey of the added component (same for all entities)</param>
    static abstract void Notify(ReadOnlySpan<Entity> entities, ReadOnlySpan<C> values, Key key);
}