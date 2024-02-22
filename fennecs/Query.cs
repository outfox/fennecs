// SPDX-License-Identifier: MIT

using System.Collections;

namespace fennecs;

public class Query : IEnumerable<Entity>, IDisposable
{
    /// <summary>
    /// Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(1);

    private protected readonly List<Archetype> Archetypes;
    private protected readonly World World;
    protected internal readonly Mask Mask;

    internal Query(World world, Mask mask, List<Archetype> archetypes)
    {
        Archetypes = archetypes;
        World = world;
        Mask = mask;
    }

    private Query()
    {
        throw new NotSupportedException("Query cannot be created on its own.");
    }

    /// <summary>
    /// Gets a reference to the component of type <typeparamref name="C"/> for the entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="target"></param>
    /// <typeparam name="C">any component type</typeparam>
    /// <returns>ref C, the component.</returns>
    /// <exception cref="KeyNotFoundException">If no C or C(Target) exists in any of the query's tables for Entity entity.</exception>
    public ref C Ref<C>(Entity entity, Entity target = default)
    {
        AssertNotDisposed();
        //TODO: Returning this ref should lock the world for the ref's scope?
        //TODO: This is just a facade for World.GetComponent, should it be removed?
        return ref World.GetComponent<C>(entity, target);
    }


    #region IEnumerable<Entity>

    /// <summary>
    /// Enumerator over all the entities in the query.
    /// Do not make modifications to the world affecting the query while enumerating.
    /// </summary>
    /// <returns>
    ///  An enumerator over all the entities in the query.
    /// </returns>
    public IEnumerator<Entity> GetEnumerator()
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
    
    public bool Contains(Entity entity)
    {
        AssertNotDisposed();
        
        var meta = World.GetEntityMeta(entity);
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

    
    internal static bool FullJoin(ref Span<int> counters, ref Span<int> goals)
    {
        // Loop through all counters, counting up to goal and wrapping until saturated
        // Example: 0-0-0 to 1-3-2:
        // 000 -> 010 -> 020 -> 001 -> 011 -> 021 -> 002 -> 012 -> 022 -> 032

        for (var i = 0; i < counters.Length; i++)
        {
            // Increment the current counter
            counters[i]++;

            // Successful increment?
            if (counters[i] < goals[i]) return true;
            
            // Current counter reached its goal, reset it and move to the next
            counters[i] = 0;

            //Continue until last counter fills up
            if (i == counters.Length - 1) break;
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
