namespace fennecs.events;

/// <summary>
/// Interface exposing event handlers when a component is removed from an entity.
/// </summary>
/// <remarks>
/// Does not provide data about relations yet.
/// </remarks>
/// <typeparam name="C">any component type</typeparam>
public interface Removed<C> where C : notnull
{
    /// <summary>
    /// Takes a list of entities who had a component removed.
    /// </summary>
    delegate void EntityHandler(Span<Entity> entities);

    /// <summary>
    /// Takes a list of entities and their removed components' values.
    /// </summary>
    delegate void EntityValueHandler(Span<Entity> entities, Span<C> removed);

    /// <summary>
    /// Event triggered when a component is modified, providing the entities only.
    /// </summary>
    /// <remarks>
    /// This triggers at the end of each chunk as it is being processed.
    /// Execution happens on the thread that is processing the chunk!
    /// </remarks>
    static event EntityHandler? Entities;

    /// <summary>
    /// Event triggered when a component is modified, providing the entities and the removed values.
    /// </summary>
    /// <remarks>
    /// This triggers at the end of each chunk as it is being processed.
    /// Execution happens on the thread that is processing the chunk!
    /// </remarks>
    static event EntityValueHandler? Values;

    /// <summary>
    /// Called by Archetypes after entity migration.
    /// </summary>
    internal static void Invoke(Span<Entity> entities, Span<C> removed)
    {
        Entities?.Invoke(entities);
        Values?.Invoke(entities, removed);
    }
}
