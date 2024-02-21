// SPDX-License-Identifier: MIT

using System.Collections;

namespace fennecs;

public class Query(World world, Mask mask, List<Table> tables) : IEnumerable<Entity>, IDisposable
{
    protected readonly ParallelOptions Options = new() {MaxDegreeOfParallelism = 24};
    
    /// <summary>
    /// Countdown event for parallel runners.
    /// </summary>
    protected readonly CountdownEvent Countdown = new(1);

    protected readonly List<Table> Tables = tables;
    protected readonly World World = world;
    protected internal readonly Mask Mask = mask;

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

        foreach (var table in Tables)
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
        var table = World.GetTable(meta.TableId);
        return Tables.Contains(table);
    }
    
    internal void AddTable(Table table)
    {
        AssertNotDisposed();
        
        Tables.Add(table);
    }

    public int Count => Tables.Sum(t => t.Count);

    public void Dispose()
    {
        AssertNotDisposed();
        
        Tables.Clear();
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
}

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

public delegate void RefAction_C<C0>(ref C0 comp0);
public delegate void RefAction_CC<C0, C1>(ref C0 comp0, ref C1 comp1);
public delegate void RefAction_CCC<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void RefAction_CCCC<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);
public delegate void RefAction_CCCCC<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);


public delegate void RefAction_CU<C0, in U>(ref C0 comp0, U uniform);
public delegate void RefAction_CCU<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);
public delegate void RefAction_CCCU<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);
public delegate void RefAction_CCCCU<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);
public delegate void RefAction_CCCCCU<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);


public delegate void MemoryAction_C<C0>(Memory<C0> c0);

public delegate void MemoryAction_CC<C0, C1>(Memory<C0> c0, Memory<C1> c1);

public delegate void MemoryAction_CCC<C0, C1, C2>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2);

public delegate void MemoryAction_CCCC<C0, C1, C2, C3>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3);

public delegate void MemoryAction_CCCCC<C0, C1, C2, C3, C4>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4);



public delegate void MemoryAction_CU<C0, U>(Memory<C0> c0, in U uniform);

public delegate void MemoryAction_CCU<C0, C1, in U>(Memory<C0> c0, Memory<C1> c1, U uniform);

public delegate void MemoryAction_CCCU<C0, C1, C2, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, U uniform);

public delegate void MemoryAction_CCCCU<C0, C1, C2, C3, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, U uniform);

public delegate void MemoryAction_CCCCCU<C0, C1, C2, C3, C4, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4, U uniform);


public delegate void SpanAction_C<C0>(Span<C0> c0);

public delegate void SpanAction_CC<C0, C1>(Span<C0> c0, Span<C1> c1);

public delegate void SpanAction_CCC<C0, C1, C2>(Span<C0> c0, Span<C1> c1, Span<C2> c2);

public delegate void SpanAction_CCCC<C0, C1, C2, C3>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3);

public delegate void SpanAction_CCCCC<C0, C1, C2, C3, C4>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, Span<C4> c4);


public delegate void SpanAction_CU<C0, in U>(Span<C0> c0, U uniform);
public delegate void SpanAction_CCU<C0, C1, in U>(Span<C0> c0, Span<C1> c1, U uniform);
public delegate void SpanAction_CCCU<C0, C1, C2, in U>(Span<C0> c0, Span<C1> c1, Span<C2> c2, U uniform);
public delegate void SpanAction_CCCCU<C0, C1, C2, C3, in U>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, U uniform); 
public delegate void SpanAction_CCCCCU<C0, C1, C2, C3, C4, in U>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, Span<C4> c4, U uniform);
