// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics;

namespace fennecs;

/// <summary>
/// <para>
/// <b>Query Base Class.</b>
/// </para>
/// <para>
/// It has no output Stream Types, and thus cannot be iterated in ways other than enumerating its Entities.
/// </para>
/// <para>
/// See <see cref="Query{C0}"/> through <see cref="Query{C0,C1,C2,C3,C4}"/> for Queries with configurable
/// output Stream Types for fast iteration.
/// </para>
/// </summary>
public class Query : IEnumerable<Entity>, IDisposable
{
    #region Internals

    /// <summary>
    /// Array of TypeExpressions for the Output Stream of this Query.
    /// </summary>
    internal readonly TypeExpression[] StreamTypes;

    /// <summary>
    /// Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(1);

    private protected readonly List<Archetype> Archetypes;
    private protected readonly World World;
    protected internal readonly Mask Mask;


    internal Query(World world, List<TypeExpression> streamTypes, Mask mask, List<Archetype> archetypes)
    {
        StreamTypes = streamTypes.ToArray();
        Archetypes = archetypes;
        World = world;
        Mask = mask;
    }

    #endregion


    #region Accessors

    /// <summary>
    /// Gets a reference to the Component of type <typeparamref name="C"/> for the entity.
    /// </summary>
    /// <param name="entity">the entity to get the component from</param>
    /// <param name="match">Match Expression for the component type <see cref="Match"/></param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, reference to the Component</returns>
    /// <remarks>The reference may be left dangling if changes to the world are made after acquiring it. Use with caution.</remarks>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the Query's tables for Entity entity.</exception>
    public ref C Ref<C>(Entity entity, Identity match = default)
    {
        AssertNotDisposed();
        World.AssertAlive(entity);
        
        if (!Contains<C>(match)) throw new KeyNotFoundException("Query does not covert the Component type.");
        if (!Contains(entity)) throw new KeyNotFoundException("Entity not in Query.");
        //TODO: Maybe it's possible to lock the World for the lifetime of the ref?
        return ref World.GetComponent<C>(entity, match);
    }

    #endregion


    #region IEnumerable<Entity>

    /// <summary>
    /// Enumerator over all the Entities in the Query.
    /// Do not make modifications to the world affecting the Query while enumerating.
    /// </summary>
    /// <returns>
    ///  An enumerator over all the Entities in the Query.
    /// </returns>
    public IEnumerator<Entity> GetEnumerator()
    {
        AssertNotDisposed();
        foreach (var table in Archetypes)
        foreach (var entity in table)
            yield return entity;
    }


    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        AssertNotDisposed();
        return GetEnumerator();
    }

    #endregion


    /// <summary>
    /// Does this Query match ("contain") the Entity, and would enumerate it?
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>true if Entity is in the Query</returns>
    public bool Contains(Entity entity)
    {
        AssertNotDisposed();

        var meta = World.GetEntityMeta(entity);
        var table = meta.Archetype;
        return Archetypes.Contains(table);
    }


    /// <summary>
    /// Does this Query match ("contain") a subset of the Type and Match Expression in its Stream Types?
    /// </summary>
    /// <param name="match">Match Expression for the component type <see cref="Match"/>.
    /// The default is <see cref="Match.Plain"/></param>
    /// <returns>true if the Query contains the Type with the given Match Expression</returns>
    public bool Contains<T>(Identity match = default)
    {
        AssertNotDisposed();
        var typeExpression = TypeExpression.Create<T>(match);
        return typeExpression.Matches(StreamTypes);
    }



    /// <summary>
    /// The sum of all distinct Entities currently matched by this Query. 
    /// </summary>
    public int Count => Archetypes.Sum(t => t.Count);


    internal void AddTable(Archetype archetype)
    {
        AssertNotDisposed();
        Archetypes.Add(archetype);
    }


    #region Random Access

    /// <summary>
    /// Does this query match any entities?
    /// </summary>
    public bool IsEmpty => Count == 0;


    /// <summary>
    /// Returns an Entity matched by this Query, selected at random.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">if the Query <see cref="IsEmpty"/></exception>
    public Entity Random()
    {
        AssertNotDisposed();
        if (Count == 0) throw new IndexOutOfRangeException("Query is empty.");
        return this[System.Random.Shared.Next(Count)];
    }


    /// <summary>
    /// Returns the <see cref="Entity"/> at the given <em>momentary position</em> in the Query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DO NOT use indexes to identify Entities across frames or World modifications.
    /// </para>
    /// <para>
    /// Instead, use the Entities themselves.
    /// </para>
    /// <para>
    /// The reason is that a Query can gain and lose both <b>Entities</b> and <b>Archetypes</b> over time.
    /// This affects the <see cref="Count"/> of the Query, similar to how changing an <see cref="ICollection{T}"/>
    /// would change its <see cref="ICollection{T}.Count"/> and positions. Treat the Entity returned as a <em>momentary result</em>
    /// for that index, which <em>should not be kept or tracked</em> across World modifications or even scopes.
    /// </para>
    /// <para>
    /// The Entity returned is, of course, usable as expected.
    /// </para>
    /// </remarks>
    /// <param name="index">a value between 0 and <see cref="Count"/></param>
    public Entity this[int index]
    {
        get
        {
            AssertNotDisposed();

            if (index < 0 || index >= Count) throw new IndexOutOfRangeException();

            using var lck = World.Lock;
            foreach (var table in Archetypes)
            {
                if (index < table.Count)
                {
                    var result = table[index];

                    return result;
                }

                index -= table.Count;
            }

            Debug.Fail("Query not empty, but no entity found.");

            return default;
        }
    }

    #endregion


    #region IDisposable Implementation

    /// <summary>
    /// Dispose the Query.
    /// </summary>
    public void Dispose()
    {
        // Microsoft CA1816: Call GC.SuppressFinalize if the class does not have a finalizer
        GC.SuppressFinalize(this);

        AssertNotDisposed();

        Archetypes.Clear();
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