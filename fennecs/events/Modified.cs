namespace fennecs.events;

/// <summary>
/// Interface handling events triggered when a component on an entity is modified.
/// </summary>
/// <remarks>
/// Does not provide data about relations yet.
/// </remarks>
/// <typeparam name="C">any component type</typeparam>
public interface Modified<C> where C : notnull
{
    /// <summary>
    /// Takes a list of entities who had a component modified.
    /// </summary>
    delegate void EntityHandler(ReadOnlySpan<Entity> entities);

    /// <summary>
    /// Takes a list of entities and their original and updated values.
    /// </summary>
    delegate void EntityValueHandler(ReadOnlySpan<Entity> entities, ReadOnlySpan<C> original, ReadOnlySpan<C> updated);

    /// <summary>
    /// Event triggered when a component is modified, providing the original and updated values.
    /// </summary>
    /// <remarks>
    /// This triggers at the end of each chunk as it is being processed.
    /// Execution happens on the thread that is processing the chunk!
    /// </remarks>
    static event EntityHandler? Entities;

    /// <summary>
    /// Event triggered when a component is modified, providing the entity and the original and updated values.
    /// </summary>
    /// <remarks>
    /// This triggers at the end of each chunk as it is being processed.
    /// Execution happens on the thread that is processing the chunk!
    /// </remarks>
    static event EntityValueHandler? Values;

    /// <summary>
    /// Called by Streams after feedback from RW&lt;C&gt; is processed.
    /// </summary>
    internal static void Invoke(ReadOnlySpan<Entity> entities, ReadOnlySpan<C> original, ReadOnlySpan<C> updated)
    {
        Entities?.Invoke(entities);
        Values?.Invoke(entities, original, updated);
    }

    /// <summary>
    /// For testing purposes, clears the event handlers.
    /// </summary>
    internal static void Clear()
    {
        Entities = null;
        Values = null;
    }
}
