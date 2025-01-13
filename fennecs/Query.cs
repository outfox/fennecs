// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
public sealed partial class Query : IEnumerable<Entity>, IDisposable, IBatchBegin, IAddRemove<Query> 
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
        // No Query can match a despawned Entity, or an entity from a different World.
        if (!World.IsAlive(entity)) return false;
        
        var meta = World.GetEntityMeta(entity);
        var table = meta.Archetype;
        return Archetypes.Contains(table);
    }

    /// <summary>
    ///     Does this Query match ("contain") a subset of the Type and Match Expression in its Stream Types?
    /// </summary>
    /// <param name="match">
    ///     Match Expression for the component type <see cref="Cross" />.
    ///     The default is <see cref="Key.Plain"/>
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
    ///     The default is <see cref="Key.Plain"/>
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

    /// <inheritdoc />
    /// <remarks>
    /// This creates a batch and immediately submits it. Use <see cref="Batch()"/> to create a batch manually.
    /// </remarks>
    public Query Add<T>(T data, Key key = default) where T : notnull
    {
        Batch().Add(data, key).Submit();
        return this;
    }

    
    /// <inheritdoc />
    /// <remarks>
    /// This creates a batch and immediately submits it. Use <see cref="Batch()"/> to create a batch manually.
    /// </remarks>
    public Query Add<T>(Key key = default) where T : notnull, new() => Add(new T(), key);


    /// <inheritdoc />
    public Query Link<L>(L link) where L : class => Add(link, Key.Of(link));

    /// <inheritdoc />
    public Query Remove<C>(Match match = default) where C : notnull => Remove(MatchExpression.Of<C>(match));
    

    /// <inheritdoc />
    /// <remarks>
    /// This creates a batch and immediately submits it. Use <see cref="Batch()"/> to create a batch manually.
    /// </remarks>
    public Query Remove(MatchExpression expression)
    {
        Batch().Remove(expression).Submit();
        return this;
    }

    /// <inheritdoc />
    public Query Remove<C>(C target) where C : class => Remove<C>(Key.Of(target));

    /// <inheritdoc />
    /// <remarks>
    /// This creates a batch and immediately submits it. Use <see cref="Batch()"/> to create a batch manually.
    /// </remarks>


    /// <inheritdoc />
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    [OverloadResolutionPriority(1)]
    public Batch Batch(Batch.AddConflict addConflict = default, Batch.RemoveConflict removeConflict = default) => new(Archetypes, World, Mask.Clone(), addConflict, removeConflict);
    
    /// <inheritdoc />
    public Batch Batch(Batch.RemoveConflict removeConflict = default) => Batch(default, removeConflict);


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

    #region Blitters
    
    /// <summary>
    /// <para>Blit (write, fill) a component value of a type to all entities matched by this query that have that component.</para>
    /// <para>🚀 Very fast!</para>
    /// </summary>
    /// <remarks>
    /// To speed this up further, consider making the component a required component for this (or a separate) Query. (use <see cref="QueryBuilderBase{QB}.Has{T}(fennecs.Match)"/>).
    /// </remarks>
    /// <param name="value">a component value</param>
    /// <param name="match">optional, leave at default for Plain components, use Entity.Any or specific Entity for Relations, or any other Key</param>
    public void Blit<C>(C value, Match match = default) where C : notnull => Archetypes.Fill(match, value);
    
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
