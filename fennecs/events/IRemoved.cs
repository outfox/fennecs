namespace fennecs.events;

/// <summary>
/// Interface handling events triggered when a component is removed from an entity.
/// </summary>
/// <remarks>
/// fennecs may call this called more than once for differetn blocks of entities/components (but only once per change event per Entity)
/// </remarks>
public interface IRemoved
{
    /// <summary>
    /// Called when the component has been removed from the Entities.
    /// </summary>
    /// <param name="entities">the entities that have the component removed</param>
    static abstract void Notify(ReadOnlySpan<Entity> entities);
}


/// <summary>
/// Interface handling events triggered when a component is removed from an entity, providing the removed values.
/// </summary>
/// <remarks>
/// fennecs may call this called more than once for differetn blocks of entities/components (but only once per change event per Entity)
/// </remarks>
public interface IRemovedValues<C> where C : notnull
{
    /// <summary>
    /// Called when the component has been removed from the Entities, passing the removed values.
    /// </summary>
    /// <param name="entities">the entities that have the component removed</param>
    /// <param name="values">the removed values</param>
    /// <param name="key">secondary key of the removed component (same for all entities)</param>
    static abstract void Notify(ReadOnlySpan<Entity> entities, ReadOnlySpan<C> values, Key key);
}
