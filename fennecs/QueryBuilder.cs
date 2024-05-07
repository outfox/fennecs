// SPDX-License-Identifier: MIT

// ReSharper disable MemberCanBePrivate.Global

using fennecs.pools;

namespace fennecs;

public class QueryBuilder : IDisposable
{
    internal readonly World World;
    protected readonly Mask Mask = MaskPool.Rent();

    protected readonly PooledList<TypeExpression> StreamTypes = PooledList<TypeExpression>.Rent();

    /* TODO: Implement deferred builder
    private List<ValueTuple<Type, Identity, object>> _has;
    private List<ValueTuple<Type, Identity, object>> _not;
    private List<ValueTuple<Type, Identity, object>> _any;
    */


    internal QueryBuilder(World world)
    {
        World = world;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    protected void Outputs<T>(Identity target = default)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        StreamTypes.Add(typeExpression);
        Mask.Has(typeExpression);
    }


    public virtual QueryBuilder Has<T>(Identity target = default)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        if (StreamTypes.Contains(typeExpression)) throw new InvalidOperationException($"Type {typeExpression} is already an output of this query.");

        Mask.Has(typeExpression);
        return this;
    }


    public virtual QueryBuilder Has<T>(T target) where T : class
    {
        Mask.Has(TypeExpression.Of<T>(Identity.Of(target)));
        return this;
    }


    public virtual QueryBuilder Not<T>(Identity target = default)
    {
        Mask.Not(TypeExpression.Of<T>(target));
        return this;
    }


    public virtual QueryBuilder Not<T>(T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));

        Mask.Not(typeExpression);
        return this;
    }


    public virtual QueryBuilder Any<T>(Identity target = default)
    {
        Mask.Any(TypeExpression.Of<T>(target));
        return this;
    }


    public virtual QueryBuilder Any<T>(T target) where T : class
    {
        Mask.Any(TypeExpression.Of<T>(Identity.Of(target)));
        return this;
    }


    public void Dispose()
    {
        Mask.Dispose();
        StreamTypes.Dispose();
    }
}


public sealed class QueryBuilder<C1> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match = default) : base(world)
    {
        Outputs<C1>(match);
    }


    public Query<C1> Build()
    {
        return (Query<C1>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    public override QueryBuilder<C1> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1>) base.Has<T>(target);
    }


    public override QueryBuilder<C1> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1>) base.Has(target);
    }


    public override QueryBuilder<C1> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1>) base.Not<T>(target);
    }


    public override QueryBuilder<C1> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1>) base.Not(target);
    }


    public override QueryBuilder<C1> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1>) base.Any<T>(target);
    }


    public override QueryBuilder<C1> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1>) base.Any(target);
    }
}


public sealed class QueryBuilder<C1, C2> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1, C2>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match1, Identity match2) : base(world)
    {
        Outputs<C1>(match1);
        Outputs<C2>(match2);
    }


    public Query Build()
    {
        return (Query<C1, C2>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    public override QueryBuilder<C1, C2> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Has(target);
    }


    public override QueryBuilder<C1, C2> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Not(target);
    }


    public override QueryBuilder<C1, C2> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Any(target);
    }
}


public sealed class QueryBuilder<C1, C2, C3> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1, C2, C3>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match1, Identity match2, Identity match3) : base(world)
    {
        Outputs<C1>(match1);
        Outputs<C2>(match2);
        Outputs<C3>(match3);
    }


    public Query<C1, C2, C3> Build()
    {
        return (Query<C1, C2, C3>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    public override QueryBuilder<C1, C2, C3> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2, C3> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Has(target);
    }


    public override QueryBuilder<C1, C2, C3> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2, C3> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Not(target);
    }


    public override QueryBuilder<C1, C2, C3> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2, C3> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Any(target);
    }
}


public sealed class QueryBuilder<C1, C2, C3, C4> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1, C2, C3, C4>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match1, Identity match2, Identity match3, Identity match4) : base(world)
    {
        Outputs<C1>(match1);
        Outputs<C2>(match2);
        Outputs<C3>(match3);
        Outputs<C4>(match4);
    }


    public Query<C1, C2, C3, C4> Build()
    {
        return (Query<C1, C2, C3, C4>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    public override QueryBuilder<C1, C2, C3, C4> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Has(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Not(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Any(target);
    }
}


public sealed class QueryBuilder<C1, C2, C3, C4, C5> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1, C2, C3, C4, C5>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match1, Identity match2, Identity match3, Identity match4, Identity match5) : base(world)
    {
        Outputs<C1>(match1);
        Outputs<C2>(match2);
        Outputs<C3>(match3);
        Outputs<C4>(match4);
        Outputs<C5>(match5);
    }


    public Query<C1, C2, C3, C4, C5> Build()
    {
        return (Query<C1, C2, C3, C4, C5>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Has(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Not(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Any(target);
    }
}