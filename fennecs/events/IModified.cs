namespace fennecs.events;

/// <summary>
/// Interface handling events triggered when a component on an entity is modified.
/// </summary>
/// <remarks>
/// fennecs may call this called more than once for differetn blocks of entities/components (but only once per change event per Entity)
/// </remarks>
public interface IModified<C> : ICommit where C : notnull
{
    /// <summary>
    /// Called when the component is modified.
    /// </summary>
    /// <param name="entities">the entities that have the component added</param>
    /// <param name="newValues">the new values of the component</param>
    /// <param name="oldValues">the old values of the component (empty for newly added components)</param>
    /// <param name="key">secondarykey of the added component (same for all entities)</param>
    static abstract void Notify(ReadOnlySpan<Entity> entities, ReadOnlySpan<C> newValues, ReadOnlySpan<C> oldValues, Key key);
}
