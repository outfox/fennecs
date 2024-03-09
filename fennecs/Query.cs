// SPDX-License-Identifier: MIT

using System.Collections;

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
public class Query : IEnumerable<Entity>, IDisposable
{
    /// <summary>
    ///     The sum of all distinct Entities currently matched by this Query.
    ///     Affected by Filters.
    /// </summary>
    public int Count => _trackedArchetypes.Sum(t => t.Count);
    
    #region Accessors
    /// <summary>
    ///     Gets a reference to the Component of type <typeparamref name="C" /> for the entity.
    /// </summary>
    /// <param name="entity">the entity to get the component from</param>
    /// <param name="match">Match Expression for the component type <see cref="Match" /></param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the Query's tables for Entity entity.</exception>
    public ref C Ref<C>(Entity entity, Identity match = default)
    {
        AssertNotDisposed();

        if (entity.World != World) throw new InvalidOperationException("Entity is not from this World.");
        World.AssertAlive(entity);

        if (!Contains<C>(match)) throw new TypeAccessException("Query does not match this Component type.");
        if (!Contains(entity)) throw new KeyNotFoundException("Entity not in Query.");

        //TODO: Maybe it's possible to lock the World for the lifetime of the ref?
        return ref World.GetComponent<C>(entity, match);
    }
    #endregion


    /// <summary>
    ///     Does this Query match ("contain") the Entity, and would enumerate it?
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>true if Entity is in the Query</returns>
    public bool Contains(Entity entity)
    {
        AssertNotDisposed();
        
        var meta = World.GetEntityMeta(entity);
        var table = meta.Archetype;
        return _trackedArchetypes.Contains(table);
    }


    /// <summary>
    ///     Does this Query match ("contain") a subset of the Type and Match Expression in its Stream Types?
    /// </summary>
    /// <param name="match">
    ///     Match Expression for the component type <see cref="Match" />.
    ///     The default is <see cref="Match.Plain" />
    /// </param>
    /// <returns>true if the Query contains the Type with the given Match Expression</returns>
    public bool Contains<T>(Identity match = default)
    {
        AssertNotDisposed();
        var typeExpression = TypeExpression.Of<T>(match);
        return typeExpression.Matches(StreamTypes);
    }


    internal void TrackArchetype(Archetype archetype)
    {
        _trackedArchetypes.Add(archetype);
        if (archetype.IsMatchSuperSet(StreamFilters)) Archetypes.Add(archetype);
    }


    internal void ForgetArchetype(Archetype archetype)
    {
        _trackedArchetypes.Remove(archetype);
        Archetypes.Remove(archetype);
    }


    #region Internals
    /// <summary>
    ///     Array of TypeExpressions for the Output Stream of this Query.
    ///     Mutated by Filter Expressions.
    /// </summary>
    internal readonly TypeExpression[] StreamTypes;

    /// <summary>
    ///  Filters for the Archetypes matched by the StreamTypes
    /// </summary>
    protected readonly List<TypeExpression> StreamFilters;

    /// <summary>
    ///     Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(initialCount: 1);

    private readonly List<Archetype> _trackedArchetypes;
    protected readonly List<Archetype> Archetypes;
    
    private protected readonly World World;
    protected internal readonly Mask Mask;

    public IReadOnlyList<Archetype> TrackedArchetypes => _trackedArchetypes;
    public IReadOnlyList<Archetype> CurrentArchetypes => Archetypes;


    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, IReadOnlyCollection<Archetype> archetypes)
    {
        StreamFilters = new List<TypeExpression>();
        StreamTypes = streamTypes.ToArray();
        _trackedArchetypes = archetypes.ToList();
        Archetypes = archetypes.ToList();
        World = world;
        Mask = mask;
    }
    #endregion


    #region Filtering
    /// <summary>
    ///     Adds a subset filter to this Query, reducing the Stream Types to a subset of the initial Stream Types.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can be used to narrow a query with Wildcard Match Expressions in its Stream Types, e.g. to match only
    ///         a specific Object Link in a Query that matches all Object Links of a given type.
    ///         Call this method repeatedly to set multiple filters.
    ///     </para>
    ///     <para>
    ///         Clear the filter with <see cref="ClearStreamFilter" />.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">any component type that is present in the Query's Stream Types</typeparam>
    /// <param name="match">
    ///     a Match Expression that is narrower than the respective Stream Type's initial
    ///     Match Expression.
    /// </param>
    /// <exception cref="InvalidOperationException">if the requested filter doesn't match any of the Query's Archetypes</exception>
    public void AddFilter<T>(Identity match)
    {
        StreamFilters.Add(TypeExpression.Of<T>(match));
        Archetypes.Clear();
        foreach (var archetype in _trackedArchetypes)
        {
            if (archetype.IsMatchSuperSet(StreamFilters)) Archetypes.Add(archetype);
        }
    }


    /// <summary>
    ///     Clears all narrowing filters on this Query, returning it to its initial state. See <see cref="AddFilter{T}" />.
    /// </summary>
    public void ClearStreamFilter()
    {
        StreamFilters.Clear();
        Archetypes.Clear();
        Archetypes.AddRange(_trackedArchetypes);
    }
    #endregion


    #region IEnumerable<Entity>
    /// <summary>
    ///     Enumerator over all the Entities in the Query.
    ///     Do not make modifications to the world affecting the Query while enumerating.
    /// </summary>
    /// <returns>
    ///     An enumerator over all the Entities in the Query.
    /// </returns>
    public IEnumerator<Entity> GetEnumerator()
    {
        AssertNotDisposed();

        foreach (var table in _trackedArchetypes)
        {
            if (!table.IsMatchSuperSet(StreamFilters)) continue;

            foreach (var entity in table)
                yield return entity;
        }
    }


    /// <summary>
    ///     Enumerator over a subset of the Entities in the Query, which must also match the filters.
    ///     Do not make modifications to the world affecting the Query while enumerating.
    /// </summary>
    /// <returns>
    ///     An enumerator over the Entities in the Query that match all provided <see cref="TypeExpression">TypeExpressions</see>.
    /// </returns>
    public IEnumerable<Entity> Filtered(params TypeExpression[] filterExpressions)
    {
        AssertNotDisposed();

        foreach (var table in _trackedArchetypes)
            if (table.IsMatchSuperSet(filterExpressions))
                foreach (var entity in table)
                    yield return entity;
    }


    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        AssertNotDisposed();
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
        AssertNotDisposed();
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
            AssertNotDisposed();

            if (index < 0 || index >= Count) throw new IndexOutOfRangeException();

            using var worldLock = World.Lock;
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
    public void Add<T>(T data) => Batch(fennecs.Batch.AddConflict.Disallow, fennecs.Batch.RemoveConflict.Disallow).Add(data).Submit();


    /// <summary>
    ///     Removes the given Component from all Entities matched by this query.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the Query does not rule out this Component type in a Filter Expression.</exception>
    /// <typeparam name="T">any Component type matched by the query</typeparam>
    public void Remove<T>() => Batch(fennecs.Batch.AddConflict.Disallow, fennecs.Batch.RemoveConflict.Disallow).Remove<T>().Submit();


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
        return new Batch(_trackedArchetypes, World, Mask.Clone(), addConflict, removeConflict);
    }


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
    /// <li><see cref="TruncateMode.Proportional"/>: (default) Truncate matched Archetypes proportionally to their contents (approximation by rounding).</li>
    /// <li><see cref="TruncateMode.PerArchetype"/>: Truncate each matched Archetype to the specified maximum count.
    /// This means a Query matching <c>n</c> Archetypes will have up to <c>n * maxEntityCount</c> Entities after this
    /// operation.</li>
    /// </ul>
    /// </param>
    public void Truncate(int maxEntityCount, TruncateMode mode = default)
    {
        //TODO: Make available as deferred operation.
        if (World.Mode != World.WorldMode.Immediate)
            throw new InvalidOperationException("Truncate can only be used in Immediate mode.");
        
        var count = Count;
        if (count <= maxEntityCount) return;

        foreach (var archetype in _trackedArchetypes)
            switch (mode)
            {
                case TruncateMode.PerArchetype:
                    archetype.Truncate(maxEntityCount);
                    break;
                case TruncateMode.Proportional:
                default:
                    var ratio = (float) maxEntityCount / count;
                    archetype.Truncate((int) Math.Round(ratio * archetype.Count));
                    break;
            }
    }


    public enum TruncateMode
    {
        Proportional = default,
        PerArchetype,
    }
    #endregion


    #region IDisposable Implementation
    /// <summary>
    ///     Dispose the Query.
    /// </summary>
    public void Dispose()
    {
        // Microsoft CA1816: Call GC.SuppressFinalize if the class does not have a finalizer
        GC.SuppressFinalize(this);

        AssertNotDisposed();

        _trackedArchetypes.Clear();
        disposed = true;
        World.RemoveQuery(this);
        Mask.Dispose();
    }


    protected void AssertNotDisposed()
    {
        if (!disposed) return;
        throw new ObjectDisposedException(nameof(Query));
    }


    private bool disposed { get; set; }
    #endregion
}


// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo