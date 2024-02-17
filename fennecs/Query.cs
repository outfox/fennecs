// SPDX-License-Identifier: MIT

using System.Collections;

namespace fennecs;

public class Query(World world, Mask mask, List<Table> tables) : IEnumerable<Entity>, IEnumerable
{
    protected readonly ParallelOptions Options = new() {MaxDegreeOfParallelism = 24};

    private protected readonly List<Table> Tables = tables;
    private protected readonly World world = world;

    protected internal readonly Mask Mask = mask;
    

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
        return GetEnumerator();
    }
    #endregion
    
    public bool Contains(Entity entity)
    {
        var meta = world.GetEntityMeta(entity.Identity);
        var table = world.GetTable(meta.TableId);
        return Tables.Contains(table);
    }
    
    internal void AddTable(Table table)
    {
        Tables.Add(table);
    }

    public int Count => Tables.Sum(t => t.Count);
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


public delegate void SpanAction_C<C0>(Span<C0> c0);

public delegate void SpanAction_CC<C0, C1>(Span<C0> c0, Span<C1> c1);

public delegate void SpanAction_CCC<C0, C1, C2>(Span<C0> c0, Span<C1> c1, Span<C2> c2);

public delegate void SpanAction_CCCC<C0, C1, C2, C3>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3);

public delegate void SpanAction_CCCCC<C0, C1, C2, C3, C4>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, Span<C4> c4);


public delegate void SpanAction_CU<C0, U>(Span<C0> c0, U uniform);
public delegate void SpanAction_CCU<C0, C1, U>(Span<C0> c0, Span<C1> c1, U uniform);
public delegate void SpanAction_CCCU<C0, C1, C2, U>(Span<C0> c0, Span<C1> c1, Span<C2> c2, U uniform);
public delegate void SpanAction_CCCCU<C0, C1, C2, C3, U>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, U uniform); 
public delegate void SpanAction_CCCCCU<C0, C1, C2, C3, C4, U>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, Span<C4> c4, U uniform);


// ReSharper enable IdentifierTypo
// ReSharper enable InconsistentNaming

// ReSharper disable CommentTypo
/* TODO: These would be used for "early out" and search type algorithms.
 public delegate void RefAction_CS<C0>(ref C0 comp0, ParallelLoopState state);

public delegate void RefAction_CCS<C0, C1>(ref C0 comp0, ref C1 comp1, ParallelLoopState state);

public delegate void RefAction_CCCS<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2, ParallelLoopState state);

public delegate void RefAction_CCCCS<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ParallelLoopState state);

public delegate void RefAction_CCCCCS<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, ParallelLoopState state);


public delegate void RefAction_CUS<C0, in U>(ref C0 comp0, U uniform, ParallelLoopState state);

public delegate void RefAction_CCUS<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform, ParallelLoopState state);

public delegate void RefAction_CCCUS<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform, ParallelLoopState state);

public delegate void RefAction_CCCCUS<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform, ParallelLoopState state);

public delegate void RefAction_CCCCCUS<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform, ParallelLoopState state);
*/
// ReSharper enable CommentTypo
