// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using fennecs.pools;

namespace fennecs;

/// <summary>
///     <para>
///         <b>Query Base Class.</b>
///     </para>
///     <para>
///         It has no output Stream Types, and thus cannot be iterated in ways other than enumerating its Entities.
///     </para>
///     <para>
///         See <see cref="Query{C0}" /> through <see cref="Query{C0,C1,C2,C3,C4}" /> for Queries with configurable
///         output Stream Types for fast iteration.
///     </para>
/// </summary>
public partial class Query : IEnumerable<Entity>, IDisposable
{
    internal static int Concurrency => Math.Max(1, Environment.ProcessorCount - 2);

    /// <summary>
    ///     The sum of all distinct Entities currently matched by this Query.
    ///     Affected by Filters.
    /// </summary>
    public virtual int Count => Archetypes.Sum(t => t.Count);

    #region Accessors

    /// <summary>
    ///     Gets a reference to the Component of type <typeparamref name="C" /> for the entity.
    /// </summary>
    /// <param name="entity">the entity to get the component from</param>
    /// <param name="match">Match Expression for the component type <see cref="Cross" /></param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the Query's tables for <see cref="Entity"/> entity.</exception>
    public ref C Ref<C>(Entity entity, Match match)
    {
        //TODO: We should be able to do that with another intermediate type component.
        if (match.IsWildcard) throw new("Match expression must not be a wildcard.");
        if (entity.World != World) throw new InvalidOperationException("Entity is not from this World.");
        World.AssertAlive(entity);

        if (!Contains<C>(match)) throw new TypeAccessException("Query does not match this Component type.");
        if (!Contains(entity)) throw new KeyNotFoundException("Entity not in Query.");

        //TODO: Maybe it's possible to lock the World for the lifetime of the ref?
        return ref World.GetComponent<C>(entity, match);
    }

    /// <inheritdoc cref="Ref{C}(fennecs.Entity,fennecs.Match)"/>
    public ref C Ref<C>(Entity entity) => ref Ref<C>(entity, Match.Plain);

    #endregion

    
    /// <summary>
    ///     Does this Query match ("contain") the Entity, and would enumerate it?
    /// </summary>
    /// <param name="entity">an entity</param>
    /// <returns>true if Entity is in the Query</returns>
    public bool Contains(Entity entity)
    {
        var meta = World.GetEntityMeta(entity);
        var table = meta.Archetype;
        return _trackedArchetypes.Contains(table);
    }

    /// <summary>
    ///     Does this Query match ("contain") a subset of the Type and Match Expression in its Stream Types?
    /// </summary>
    /// <param name="match">
    ///     Match Expression for the component type <see cref="Cross" />.
    ///     The default is <see cref="Match.Plain"/>
    /// </param>
    /// <returns>true if the Query contains the Type with the given Match Expression</returns>
    public bool Contains<T>(Match match = default)
    {
        var typeExpression = TypeExpression.Of<T>(match);
        return typeExpression.Matches(StreamTypes);
    }


    internal void TrackArchetype(Archetype archetype)
    {
        _trackedArchetypes.Add(archetype);
        if (!archetype.Matches(_streamExclusions) && archetype.IsMatchSuperSet(_streamFilters)) Archetypes.Add(archetype);
    }


    internal void ForgetArchetype(Archetype archetype)
    {
        _trackedArchetypes.Remove(archetype);
        Archetypes.Remove(archetype);
    }

    /// <summary>
    /// Allocates and Pre-Initializes internal data structures for <see cref="Work{C1}"/>
    /// </summary>
    /// <remarks>
    /// This is only needed for benchmark situations and debugging where allocations
    /// might otherwise be made happen lazily only as the actual workload starts.
    /// </remarks>
    public virtual Query Warmup() => this;

    #region Internals

    /// <summary>
    ///     Array of TypeExpressions for the Output Stream of this Query.
    ///     Mutated by Filter Expressions.
    /// </summary>
    internal readonly ImmutableArray<TypeExpression> StreamTypes;

    /// <summary>
    ///  Filters for the Archetypes matched by the StreamTypes (must match)
    /// </summary>
    private readonly List<TypeExpression> _streamFilters;

    /// <summary>
    ///  Additional exclusions for the Archetypes matched by the StreamTypes (must not match)
    /// </summary>
    private readonly List<TypeExpression> _streamExclusions;

    /// <summary>
    ///     Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(initialCount: 1);

    /// <summary>
    /// All the archetypes that this query can potentially match.
    /// </summary>
    private readonly List<Archetype> _trackedArchetypes;

    /// <summary>
    /// This query's currently matched Archetypes.
    /// (affected by filters)
    /// </summary>
    internal protected readonly List<Archetype> Archetypes;

    /// <summary>
    /// The World this Query is associated with.
    /// The World will notify the Query of new matched Archetypes, or Archetypes to forget.
    /// </summary>
    internal protected World World { get; init; }

    /// <summary>
    ///  Mask for the Query. Used for matching (including/excluding/filtering) Archetypes.
    /// </summary>
    internal readonly Mask Mask;

    /// <summary>
    /// A Read Only View of the Archetypes that this query "tracks", meaning:
    /// <ul>
    /// <li>it will match (enumerate) entities in them</li>
    /// <li>it can perform batch operations on them</li>
    /// <li>filters will only be applied to these archetypes (filters are subtractive)</li>
    /// </ul>
    /// </summary>
    /// <remarks>
    /// Does not exclude unmatched Archetypes (through Filter expressions), as Filters are applied on top.
    /// This is primarily debug information, left available as a public property, because it can be useful to understand the "weight" and range of a query.
    /// The world it will update this list when they are added or removed.
    /// </remarks>
    public IReadOnlyList<Archetype> TrackedArchetypes => _trackedArchetypes;


    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, IReadOnlyCollection<Archetype> archetypes)
    {
        _streamFilters = [];
        _streamExclusions = [];
        StreamTypes = [..streamTypes];
        _trackedArchetypes = archetypes.ToList();
        Archetypes = archetypes.ToList();
        World = world;
        Mask = mask;
    }

    /// <summary>
    /// Base constructor (TODO: Fix up required fields / refactor out stream filters)
    /// </summary>
    internal protected Query()
    {
        Archetypes = PooledList<Archetype>.Rent();
        World = default!;
        Mask = default!;
    }
    
    
    internal Query(World world, Mask mask, PooledList<Archetype> matchingTables)
    {
        Archetypes = matchingTables;
        World = world;
        Mask = mask;
    }

    #endregion


    #region Filtering

    /// <inheritdoc cref="Subset{T}"/>
    /// <param name="match">
    ///     a Match Expression that is narrower than the respective Stream Type's initial
    ///     Match Expression (e.g. if Query has Match.Any, Match.Plain or Match.Object would be useful here).
    /// </param>
    public void Subset<T>(Match match)
    {
        _streamFilters.Add(TypeExpression.Of<T>(match));
        FilterArchetypes();
    }

    /// <inheritdoc cref="Subset{T}"/>
    /// <summary>
    /// Specify a match expression to exclude certain relations.
    /// </summary>
    /// <param name="match">
    ///     a Match Expression that is narrower than the respective Stream Type's initial
    ///     Match Expression. If it is wider, the matched set will be empty. 
    /// </param>
    public void Exclude<T>(Match match)
    {
        _streamExclusions.Add(TypeExpression.Of<T>(match));
        FilterArchetypes();
    }

    private void FilterArchetypes()
    {
        Archetypes.Clear();
        foreach (var archetype in _trackedArchetypes)
        {
            if (!archetype.Matches(_streamExclusions) && archetype.IsMatchSuperSet(_streamFilters))
            {
                Archetypes.Add(archetype);
            }
        }
    }

    /// <summary>
    ///     Clears all <see cref="Subset{T}(fennecs.Match)"/> and <see cref="Exclude{T}(fennecs.Match)"/> filters on this Query, returning it to its initial state. See <see cref="Subset{T}(fennecs.Match)" />.
    /// </summary>
    public void ClearFilters()
    {
        _streamFilters.Clear();
        _streamExclusions.Clear();
        Archetypes.Clear();
        Archetypes.AddRange(_trackedArchetypes);
    }

    #endregion


    #region IEnumerable<Entity>

    /// <summary>
    ///     Enumerator over all the Entities in the Query (dependent on filter state).
    ///     Do not make modifications to the world affecting the Query while enumerating.
    /// </summary>
    /// <returns>
    ///     An enumerator over all the Entities in the Query.
    /// </returns>
    public IEnumerator<Entity> GetEnumerator()
    {

        foreach (var table in Archetypes)
        {
            foreach (var entity in table) yield return entity;
        }
    }


    /// <summary>
    ///     Enumerator over a subset of the Entities in the Query, which must also match the filters.
    ///     Do not make modifications to the world affecting the Query while enumerating.
    ///     This is a convenience method for filtering the Query without changing its filter state.
    ///     Any pre-existing filter state is being honored.
    /// </summary>
    /// <returns>
    ///     An enumerator over the Entities in the Query that match all provided <see cref="TypeExpression">TypeExpressions</see>.
    /// </returns>
    internal IEnumerable<Entity> Filtered(params TypeExpression[] filterExpressions)
    {
        foreach (var table in Archetypes)
        {
            if (!table.IsMatchSuperSet(filterExpressions)) continue;

            foreach (var entity in table) yield return entity;
        }
    }


    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion


    #region Random Access

    /// <summary>
    ///     Does this query match any entities?
    /// </summary>
    public bool IsEmpty => Count == 0;


    /// <summary>
    ///     Returns an Entity matched by this Query, selected at random.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">if the Query <see cref="IsEmpty" /></exception>
    public Entity Random()
    {
        if (Count == 0) throw new IndexOutOfRangeException("Query is empty.");
        return this[System.Random.Shared.Next(Count)];
    }


    /// <summary>
    ///     Returns the <see cref="Entity" /> at the given <em>momentary position</em> in the Query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         DO NOT use indexes to identify Entities across frames or World modifications.
    ///     </para>
    ///     <para>
    ///         Instead, use the Entities themselves.
    ///     </para>
    ///     <para>
    ///         The reason is that a Query can gain and lose both <b>Entities</b> and <b>Archetypes</b> over time.
    ///         This affects the <see cref="Count" /> of the Query, similar to how changing an <see cref="ICollection{T}" />
    ///         would change its <see cref="ICollection{T}.Count" /> and positions. Treat the Entity returned as a <em>momentary result</em>
    ///         for that index, which <em>should not be kept or tracked</em> across World modifications or even scopes.
    ///     </para>
    ///     <para>
    ///         The Entity returned is, of course, usable as expected.
    ///     </para>
    /// </remarks>
    /// <param name="index">a value between 0 and <see cref="Count" /></param>
    public Entity this[int index]
    {
        get
        {

            if (index < 0 || index >= Count) throw new IndexOutOfRangeException();

            using var worldLock = World.Lock();
            Entity result = default;

            foreach (var table in _trackedArchetypes)
            {
                if (index < table.Count)
                {
                    result = table[index];
                    break;
                }

                index -= table.Count;
            }

            return result;
        }
    }

    #endregion


    #region Bulk Operations

    /// <summary>
    ///     Adds a Component (using default constructor) to all Entities matched by this query.
    /// </summary>
    /// <inheritdoc cref="Add{T}(T)" />
    public void Add<T>() where T : new() => Add<T>(new T());


    /// <summary>
    ///     Adds the given Component (using specified data) to all Entities matched by this query.
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="data">the data to add</param>
    /// <exception cref="InvalidOperationException">if the Query does not rule out this Component type in a Filter Expression.</exception>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Add<T>(T data) => Batch().Add(data).Submit();


    /// <summary>
    ///     Removes the given Component from all Entities matched by this query.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the Query does not rule out this Component type in a Filter Expression.</exception>
    /// <typeparam name="T">any Component type matched by the query</typeparam>
    public void Remove<T>() => Batch().Remove<T>().Submit();


    /// <summary>
    /// Provide a Builder Struct that allows to enqueue multiple operations on the Entities matched by this Query.
    /// </summary>
    /// <remarks>
    /// (Add, Remove, etc.) If they were applied one by one, they would cause the Entities to no longer be matched
    /// after the first operation, and thus lead to undesired results.
    /// </remarks> 
    /// <returns>a BatchOperation that needs to be executed by calling <see cref="Batch.Submit"/></returns>
    public Batch Batch()
    {
        return new Batch(_trackedArchetypes, World, Mask.Clone(), default, default);
    }


    /// <summary>
    /// Provide a Builder Struct that allows to enqueue multiple operations on the Entities matched by this Query.
    /// Allows configuring custom handling of conflicts when adding components that might already be on some entities in the
    /// query, see <see cref="Batch.AddConflict"/> and <see cref="Batch.AddConflict"/>.
    /// </summary>
    /// <remarks>
    /// (Add, Remove, etc.) If they were applied one by one, they would cause the Entities to no longer be matched
    /// after the first operation, and thus lead to undesired results.
    /// </remarks> 
    /// <returns>a BatchOperation that needs to be executed by calling <see cref="Batch.Submit"/></returns>
    public Batch Batch(Batch.AddConflict addConflict)
    {
        return new Batch(_trackedArchetypes, World, Mask.Clone(), addConflict, default);
    }


    /// <summary>
    /// Provide a Builder Struct that allows to enqueue multiple operations on the Entities matched by this Query.
    /// Allows configuring custom handling of conflicts when adding components that might already be on some entities in the
    /// query, see <see cref="Batch.AddConflict"/> and <see cref="Batch.AddConflict"/>.
    /// </summary>
    /// <remarks>
    /// (Add, Remove, etc.) If they were applied one by one, they would cause the Entities to no longer be matched
    /// after the first operation, and thus lead to undesired results.
    /// </remarks> 
    /// <returns>a BatchOperation that needs to be executed by calling <see cref="Batch.Submit"/></returns>
    public Batch Batch(Batch.RemoveConflict removeConflict)
    {
        return new Batch(_trackedArchetypes, World, Mask.Clone(), default, removeConflict);
    }


    /// <summary>
    /// Provide a Builder Struct that allows to enqueue multiple operations on the Entities matched by this Query.
    /// Allows configuring custom handling of conflicts when adding components that might already be on some entities in the
    /// query, see <see cref="Batch.AddConflict"/> and <see cref="Batch.AddConflict"/>.
    /// </summary>
    /// <remarks>
    /// (Add, Remove, etc.) If they were applied one by one, they would cause the Entities to no longer be matched
    /// after the first operation, and thus lead to undesired results.
    /// </remarks> 
    /// <returns>a BatchOperation that needs to be executed by calling <see cref="Batch.Submit"/></returns>
    public Batch Batch(Batch.AddConflict addConflict, Batch.RemoveConflict removeConflict)
    {
        return new(_trackedArchetypes, World, Mask.Clone(), addConflict, removeConflict);
    }


    /// <inheritdoc cref="Despawn" />
    [Obsolete("Use Despawn() instead.")]
    public void Clear() => Despawn();


    /// <summary>
    /// Despawn all Entities matched by this Query.
    /// </summary>
    public void Despawn()
    {
        Truncate(0);
    }


    /// <summary>
    /// Despawn all Entities above the specified count in the Query, using the specified mode of distribution.
    /// The default is a balanced distribution (with rounding).
    /// </summary>
    /// <param name="maxEntityCount">number of entities to preserve</param>
    /// <param name="mode">
    /// <ul>
    /// <li><see cref="TruncateMode.Proportional"/> (default) Truncate matched Archetypes proportionally to their contents (approximation by rounding).</li>
    /// <li><see cref="TruncateMode.PerArchetype"/> Truncate each matched Archetype to the specified maximum count.
    /// This means a Query matching <c>n</c> Archetypes will have up to <c>n * maxEntityCount</c> Entities after this
    /// operation.</li>
    /// </ul>
    /// </param>
    public void Truncate(int maxEntityCount, TruncateMode mode = default)
    {
        //TODO: Make available as deferred operation.
        if (World.Mode != World.WorldMode.Immediate)
            throw new InvalidOperationException("Query.Truncate can only be used in Immediate mode.");

        var count = Count;
        if (count <= maxEntityCount) return;

        foreach (var archetype in Archetypes)
            switch (mode)
            {
                case TruncateMode.PerArchetype:
                    archetype.Truncate(maxEntityCount);
                    break;
                case TruncateMode.Proportional:
                default:
                    var ratio = (float)maxEntityCount / count;
                    archetype.Truncate((int)Math.Round(ratio * archetype.Count));
                    break;
            }
    }


    /// <summary>
    /// Strategies for Query Truncation <see cref="Query.Truncate"/>
    /// </summary>
    public enum TruncateMode
    {
        /// <summary>
        /// Truncate matched Archetypes proportionally to their contents (approximation by rounding).
        /// </summary>
        Proportional = default,
        /// <summary>
        /// Truncate each matched Archetype to the specified maximum count.
        /// </summary>
        PerArchetype,
    }

    #endregion


    #region IDisposable Implementation

    /// <summary>
    ///     Dispose the Query.
    /// </summary>
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        disposed = true;

        _trackedArchetypes.Clear();
        Archetypes.Clear();

        _streamExclusions.Clear();
        _streamFilters.Clear();


        World.RemoveQuery(this);
        Mask.Dispose();

        // Microsoft CA1816: Call GC.SuppressFinalize if the class does not have a finalizer
        GC.SuppressFinalize(this);
    }

    private bool disposed { get; set; }

    #endregion
}


// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
