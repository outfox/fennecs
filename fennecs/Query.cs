// SPDX-License-Identifier: MIT

namespace fennecs;

public class Query(Archetypes archetypes, Mask mask, List<Table> tables)
{
    protected readonly ParallelOptions Options = new() {MaxDegreeOfParallelism = 16};
    protected const int SpinTimeout = 420; // ~10 microseconds

    private protected readonly List<Table> Tables = tables;
    private protected readonly Archetypes Archetypes = archetypes;
    
    protected internal readonly Mask Mask = mask;

    public bool Has(Entity entity)
    {
        var meta = Archetypes.GetEntityMeta(entity.Identity);
        var table = Archetypes.GetTable(meta.TableId);
        return Tables.Contains(table);
    }
    
    internal void AddTable(Table table)
    {
        Tables.Add(table);
    }
}

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

public delegate void QueryAction_C<C0>(ref C0 comp0);
public delegate void QueryAction_CC<C0, C1>(ref C0 comp0, ref C1 comp1);
public delegate void QueryAction_CCC<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
public delegate void QueryAction_CCCC<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);
public delegate void QueryAction_CCCCC<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);


public delegate void QueryAction_CU<C0, in U>(ref C0 comp0, U uniform);
public delegate void QueryAction_CCU<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);
public delegate void QueryAction_CCCU<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);
public delegate void QueryAction_CCCCU<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);
public delegate void QueryAction_CCCCCU<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);


public delegate void SpanAction_C<C0>(Span<C0> c0);
public delegate void SpanAction_CC<C0, C1>(Span<C0> c0, Span<C1> c1);
public delegate void SpanAction_CCC<C0, C1, C2>(Span<C0> c0, Span<C1> c1, Span<C2> c2);
public delegate void SpanAction_CCCC<C0, C1, C2, C3>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3);
public delegate void SpanAction_CCCCC<C0, C1, C2, C3, C4>(Span<C0> c0, Span<C1> c1, Span<C2> c2, Span<C3> c3, Span<C4> c4);

// ReSharper enable IdentifierTypo
// ReSharper enable InconsistentNaming

// ReSharper disable CommentTypo
/* TODO: These would be used for "early out" and search type algorithms.
 public delegate void QueryAction_CS<C0>(ref C0 comp0, ParallelLoopState state);

public delegate void QueryAction_CCS<C0, C1>(ref C0 comp0, ref C1 comp1, ParallelLoopState state);

public delegate void QueryAction_CCCS<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2, ParallelLoopState state);

public delegate void QueryAction_CCCCS<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ParallelLoopState state);

public delegate void QueryAction_CCCCCS<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, ParallelLoopState state);


public delegate void QueryAction_CUS<C0, in U>(ref C0 comp0, U uniform, ParallelLoopState state);

public delegate void QueryAction_CCUS<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform, ParallelLoopState state);

public delegate void QueryAction_CCCUS<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform, ParallelLoopState state);

public delegate void QueryAction_CCCCUS<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform, ParallelLoopState state);

public delegate void QueryAction_CCCCCUS<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform, ParallelLoopState state);
*/
// ReSharper enable CommentTypo
