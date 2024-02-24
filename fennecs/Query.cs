// SPDX-License-Identifier: MIT

using System.Collections;

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
    /// <summary>
    /// TypeExpression for the Output Stream of this Query.
    /// </summary>
    internal readonly TypeExpression[] StreamTypes;

    /// <summary>
    /// Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(1);

    private protected readonly List<Archetype> Archetypes;
    private protected readonly World World;
    protected internal readonly Mask Mask;

    internal Query(World world, List<TypeExpression> streamTypes,  Mask mask, List<Archetype> archetypes)
    {
        StreamTypes = streamTypes.ToArray();
        Archetypes = archetypes;
        World = world;
        Mask = mask;
    }

    /// <summary>
    /// Gets a reference to the Component of type <typeparamref name="C"/> for the entity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="C">any Component type</typeparam>
    /// <returns>ref C, the Component.</returns>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the Query's tables for Entity entity.</exception>
    public ref C Ref<C>(Identity identity, Identity target = default)
    {
        AssertNotDisposed();
        //TODO: Returning this ref should lock the world for the ref's scope?
        //TODO: This is just a facade for World.GetComponent, should it be removed?
        return ref World.GetComponent<C>(identity, target);
    }


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
            foreach (var entity in table) yield return entity;
    }

    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        AssertNotDisposed();
        return GetEnumerator();
    }
    #endregion

    /// <summary>
    /// True this Query matches ("contains") the Entity, and would enumerate it.
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
    /// The sum of all distinct Entities currently matched by this Query. 
    /// </summary>
    public int Count => Archetypes.Sum(t => t.Count);
    
    
    internal void AddTable(Archetype archetype)
    {
        AssertNotDisposed();
        Archetypes.Add(archetype);
    }

    #region IDisposable Implementation

    /// <summary>
    /// Dispose the Query.
    /// </summary>
    public void Dispose()
    {
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
