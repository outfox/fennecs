using System.Collections.Immutable;
using fennecs.CRUD;

namespace fennecs;

/// <summary>
/// Base type for Streams.
/// </summary>
public record Stream(Query Query) : IBatchBegin
{
    /// <summary>
    /// Archetypes, or Archetypes that match the Stream's Subset and Exclude filters.
    /// </summary>
    protected HashSet<Archetype> Filtered => Subset.IsEmpty && Exclude.IsEmpty
        ? Archetypes
        : [..Archetypes.Where(a => (Subset.IsEmpty || a.Signature.Matches(Subset)) && !a.Signature.Matches(Exclude))];

    /// <summary>
    /// Creates a builder for a Batch Operation on the Stream's underyling Query.
    /// </summary>
    /// <returns>fluent builder</returns>
    public Batch Batch() => Query.Batch();

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public Batch Batch(Batch.AddConflict add) => Query.Batch(add);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public Batch Batch(Batch.RemoveConflict remove) => Query.Batch(remove);

    /// <inheritdoc cref="fennecs.Query.Batch()"/>
    public Batch Batch(Batch.AddConflict add, Batch.RemoveConflict remove) => Query.Batch(add, remove);


    /// <summary>
    /// The number of entities that match the underlying Query.
    /// </summary>
    public int Count => Filtered.Sum(f => f.Count);


    /// <summary>
    /// The Archetypes that this Stream is iterating over.
    /// </summary>
    protected HashSet<Archetype> Archetypes => Query.Archetypes;

    /// <summary>
    /// The World this Stream is associated with.
    /// </summary>
    protected World World => Query.World;

    /// <summary>
    /// The Query this Stream is associated with.
    /// Can be re-inited via the with keyword.
    /// </summary>
    public Query Query { get; } = Query;

    /// <summary>
    /// Subset Stream Filter - if not empty, only entities with these components will be included in the Stream. 
    /// </summary>
    public ImmutableSortedSet<Comp> Subset { get; init; } = [];

    /// <summary>
    /// Exclude Stream Filter - any entities with these components will be excluded from the Stream. (none if empty)
    /// </summary>
    public ImmutableSortedSet<Comp> Exclude { get; init; } = [];
    
    
    #region Query Forwarding

    /// <inheritdoc cref="fennecs.Query.Truncate"/>
    public void Truncate(int targetSize, Query.TruncateMode mode = default)
    {
        Query.Truncate(targetSize, mode);
    }

    /// <inheritdoc cref="fennecs.Query.Despawn"/>
    public void Despawn()
    {
        foreach (var archetype in Filtered) archetype.Truncate(0);
    }

    #endregion

}