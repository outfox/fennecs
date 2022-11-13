using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HypEcs;

public class QueryBuilder
{
    internal readonly Archetypes Archetypes;
    protected readonly Mask Mask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes)
    {
        Archetypes = archetypes;
        Mask = MaskPool.Get();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder Has<T>(Entity target = default)
    {
        var typeIndex = StorageType.Create<T>(target.Identity);
        Mask.Has(typeIndex);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder Has<T>(Type type)
    {
        var entity = Archetypes.GetTypeEntity(type);
        var typeIndex = StorageType.Create<T>(entity.Identity);
        Mask.Has(typeIndex);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder Not<T>(Entity target = default)
    {
        var typeIndex = StorageType.Create<T>(target.Identity);
        Mask.Not(typeIndex);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder Not<T>(Type type)
    {
        var entity = Archetypes.GetTypeEntity(type);
        var typeIndex = StorageType.Create<T>(entity.Identity);
        Mask.Not(typeIndex);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder Any<T>(Entity target = default)
    {
        var typeIndex = StorageType.Create<T>(target.Identity);
        Mask.Any(typeIndex);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder Any<T>(Type type)
    {
        var entity = Archetypes.GetTypeEntity(type);
        var typeIndex = StorageType.Create<T>(entity.Identity);
        Mask.Any(typeIndex);
        return this;
    }
}

public sealed class QueryBuilder<C> : QueryBuilder
    where C : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C> Has<T>(Type type)
    {
        return (QueryBuilder<C>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C> Not<T>(Type type)
    {
        return (QueryBuilder<C>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C> Any<T>(Type type)
    {
        return (QueryBuilder<C>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C> Build()
    {
        return (Query<C>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2> : QueryBuilder
    where C1 : struct
    where C2 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2> Build()
    {
        return (Query<C1, C2>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3> Build()
    {
        return (Query<C1, C2, C3>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3, C4>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>().Has<C4>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3, C4> Build()
    {
        return (Query<C1, C2, C3, C4>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4, C5> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3, C4, C5>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>().Has<C4>().Has<C5>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3, C4, C5> Build()
    {
        return (Query<C1, C2, C3, C4, C5>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4, C5, C6> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3, C4, C5, C6>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>().Has<C4>().Has<C5>().Has<C6>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3, C4, C5, C6> Build()
    {
        return (Query<C1, C2, C3, C4, C5, C6>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4, C5, C6, C7> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
    where C7 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3, C4, C5, C6, C7>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>().Has<C4>().Has<C5>().Has<C6>().Has<C7>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3, C4, C5, C6, C7> Build()
    {
        return (Query<C1, C2, C3, C4, C5, C6, C7>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
    where C7 : struct
    where C8 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3, C4, C5, C6, C7, C8>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>().Has<C4>().Has<C5>().Has<C6>().Has<C7>().Has<C8>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3, C4, C5, C6, C7, C8> Build()
    {
        return (Query<C1, C2, C3, C4, C5, C6, C7, C8>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> : QueryBuilder
    where C1 : struct
    where C2 : struct
    where C3 : struct
    where C4 : struct
    where C5 : struct
    where C6 : struct
    where C7 : struct
    where C8 : struct
    where C9 : struct
{
    static readonly Func<Archetypes, Mask, List<Table>, Query> CreateQuery =
        (archetypes, mask, matchingTables) => new Query<C1, C2, C3, C4, C5, C6, C7, C8, C9>(archetypes, mask, matchingTables);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QueryBuilder(Archetypes archetypes) : base(archetypes)
    {
        Has<C1>().Has<C2>().Has<C3>().Has<C4>().Has<C5>().Has<C6>().Has<C7>().Has<C8>().Has<C9>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9>)base.Has<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> Has<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9>)base.Has<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9>)base.Not<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> Not<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9>)base.Not<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9>)base.Any<T>(target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9> Any<T>(Type type)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5, C6, C7, C8, C9>)base.Any<T>(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<C1, C2, C3, C4, C5, C6, C7, C8, C9> Build()
    {
        return (Query<C1, C2, C3, C4, C5, C6, C7, C8, C9>)Archetypes.GetQuery(Mask, CreateQuery);
    }
}