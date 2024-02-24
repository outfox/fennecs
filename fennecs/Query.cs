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
public class Query : IEnumerable<Identity>, IDisposable
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
    public IEnumerator<Identity> GetEnumerator()
    {
        AssertNotDisposed();

        foreach (var table in Archetypes)
        {
            var snapshot = table.Version;
            for (var i = 0; i < table.Count; i++)
            {
                if (snapshot != table.Version)
                {
                    throw new InvalidOperationException("Query was modified while enumerating.");
                }

                yield return table.Identities[i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        AssertNotDisposed();
        
        return GetEnumerator();
    }
    #endregion
    
    public bool Contains(Identity identity)
    {
        AssertNotDisposed();
        
        var meta = World.GetEntityMeta(identity);
        var table = meta.Archetype;
        return Archetypes.Contains(table);
    }
    
    internal void AddTable(Archetype archetype)
    {
        AssertNotDisposed();
        
        Archetypes.Add(archetype);
    }

    public int Count => Archetypes.Sum(t => t.Count);

    public void Dispose()
    {
        AssertNotDisposed();
        
        Archetypes.Clear();
        disposed = true;
        World.RemoveQuery(this);
        Mask.Dispose();
    }

    
    internal static bool CrossJoin(Span<int> counter, Span<int> limiter)
    {
        // Loop through all counters, counting up to goal and wrapping until saturated
        // Example: 0-0-0 to 1-3-2:
        // 000 -> 010 -> 020 -> 001 -> 011 -> 021 -> 002 -> 012 -> 022 -> 032

        for (var i = 0; i < counter.Length; i++)
        {
            // Increment the current counter
            counter[i]++;

            // Successful increment?
            if (counter[i] < limiter[i]) return true;
            
            // Current counter reached its goal, reset it and move to the next
            counter[i] = 0;

            //Continue until last counter fills up
            if (i == counter.Length - 1) break;
        }
        
        return false;
    }


    

    protected void AssertNotDisposed()
    {
        if (!disposed) return;
        throw new ObjectDisposedException(nameof(Query));
    }
    
    private bool disposed { get; set; }
}

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
