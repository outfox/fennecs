// SPDX-License-Identifier: MIT

using System.Collections;
using fennecs.CRUD;

namespace fennecs;

/// <summary>
///     <para>
///         <b>Query Base Class.</b>
///     </para>
///     <para>
///         It has no output Stream Types, and thus cannot be iterated in ways other than enumerating its Entities.
///     </para>
///     <para>
///         See <see cref="Stream{C}" /> Views with configurable output Stream Types for fast iteration.
///     </para>
/// </summary>
public sealed partial class Query : IEnumerable<Entity>, IDisposable, IBatchBegin
{
    /// <summary>
    ///     The sum of all distinct Entities currently matched by this Query.
    /// </summary>
    public int Count => Archetypes.Sum(t => t.Count);


    /// <summary>
    ///     Does this Query match ("contain") the Entity, and would enumerate it?
    /// </summary>
    /// <param name="entity">an entity</param>
    /// <returns>true if Entity is in the Query</returns>
    public bool Contains(Entity entity)
    {
        var meta = World.GetEntityMeta(entity);
        var table = meta.Archetype;
        return Archetypes.Contains(table);
    }

    /// <summary>
    ///     Does this Query match ("contain") a subset of the Type and Match Expression in its Stream Types?
    /// </summary>
    /// <param name="match">
    ///     Match Expression for the component type <see cref="Cross" />.
    ///     The default is <see cref="Entity.Plain"/>
    /// </param>
    /// <returns>true if the Query contains the Type with the given Match Expression</returns>
    public bool Contains<T>(Match match = default)
    {
        var expression = MatchExpression.Of<T>(match);
        return Archetypes.Any(a => expression.Matches(a.Signature));
    }


    /// <summary>
    ///     Does this Query match ("contain") a subset of the Type and Match Expression in its Stream Types?
    /// </summary>
    /// <param name="type">System.Type of the backing component data</param>
    /// <param name="match">
    ///     Match Expression for the component type <see cref="Cross" />.
    ///     The default is <see cref="Match.Plain"/>
    /// </param>
    /// <returns>true if the Query contains the Type with the given Match Expression</returns>
    public bool Contains(Type type, Match match = default)
    {
        var expression = MatchExpression.Of(type,match);
        return Archetypes.Any(a => expression.Matches(a.Signature));
    }


    internal void TrackArchetype(Archetype archetype)
    {
        Archetypes.Add(archetype);
    }


    internal void ForgetArchetype(Archetype archetype)
    {
        Archetypes.Remove(archetype);
    }


    #region Internals
    
    /// <summary>
    /// This query's currently matched Archetypes.
    /// (affected by filters)
    /// </summary>
    internal readonly HashSet<Archetype> Archetypes;

    /// <summary>
    /// The World this Query is associated with.
    /// The World will notify the Query of new matched Archetypes, or Archetypes to forget.
    /// </summary>
    internal World World { get; }

    /// <summary>
    ///  Mask for the Query. Used for matching (including/excluding/filtering) Archetypes.
    /// </summary>
    internal readonly Mask Mask;

    internal Query(World world, Mask mask,HashSet<Archetype> matchingTables)
    {
        Archetypes = matchingTables;
        World = world;
        Mask = mask;
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

            foreach (var table in Archetypes)
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
    public void Add<T>() where T : notnull, new() => Add(new T());


    /// <summary>
    ///     Adds the given Component (using specified data) to all Entities matched by this query.
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="data">the data to add</param>
    /// <exception cref="InvalidOperationException">if the Query does not rule out this Component type in a Filter Expression.</exception>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Add<T>(T data) where T : notnull => Batch().Add(data).Submit();


    /// <summary>
    ///     Removes the given Component from all Entities matched by this query.
    /// </summary>
    /// <exception cref="InvalidOperationException">if the Query does not rule out this Component type in a Filter Expression.</exception>
    /// <typeparam name="T">any Component type matched by the query</typeparam>
    public void Remove<T>() where T : notnull => Batch().Remove<T>().Submit();


    /// <inheritdoc />
    public Batch Batch() => new(Archetypes, World, Mask.Clone(), default, default);


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
        return new Batch(Archetypes, World, Mask.Clone(), addConflict, default);
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
        return new Batch(Archetypes, World, Mask.Clone(), default, removeConflict);
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
        return new(Archetypes, World, Mask.Clone(), addConflict, removeConflict);
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

        using var worldLock = World.Lock();

        var count = Count;
        if (count <= maxEntityCount) return;

        foreach (var archetype in Archetypes)
        {
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
        ObjectDisposedException.ThrowIf(Disposed, this);

        Disposed = true;

        World.RemoveQuery(this);
        Mask.Dispose();
    }

    private bool Disposed { get; set; }

    #endregion
}


// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
