// SPDX-License-Identifier: MIT

// ReSharper disable MemberCanBePrivate.Global

using fennecs.pools;

namespace fennecs;

public class QueryBuilder : IDisposable
{
    internal readonly World World;
    protected readonly Mask Mask = MaskPool.Rent();

    /* TODO: Implement deferred builder
    private List<ValueTuple<Type, Identity, object>> _has;
    private List<ValueTuple<Type, Identity, object>> _not;
    private List<ValueTuple<Type, Identity, object>> _any;
    */

    internal QueryBuilder(World world)
    {
        World = world;
    }

    public virtual QueryBuilder Has<T>(Entity target = default)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        Mask.Has(typeExpression);
        return this;
    }


    public virtual QueryBuilder Has<T>(T target) where T : class
    {
        Mask.Has(TypeExpression.Create<T>(Entity.Of(target)));
        return this;
    }


    public virtual QueryBuilder Not<T>(Entity target = default)
    {
        Mask.Not(TypeExpression.Create<T>(target));
        return this;
    }

    public virtual QueryBuilder Not<T>(T target) where T : class
    {
        Mask.Not(TypeExpression.Create<T>(Entity.Of(target)));
        return this;
    }


    public virtual QueryBuilder Any<T>(Entity target = default)
    {
        Mask.Any(TypeExpression.Create<T>(target));
        return this;
    }


    public virtual QueryBuilder Any<T>(T target) where T : class
    {
        Mask.Any(TypeExpression.Create<T>(Entity.Of(target)));
        return this;
    }

    public void Dispose()
    {
        Mask.Dispose();
    }

}

public sealed class QueryBuilder<C> : QueryBuilder
{
    private static readonly Func<World, Mask, List<Archetype>, Query> CreateQuery =
        (world, mask, matchingTables) => new Query<C>(world, mask, matchingTables);


    internal QueryBuilder(World world, Entity match = default) : base(world)
    {
        Has<C>(match);
    }


    public override QueryBuilder<C> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C>) base.Has<T>(target);
    }

    
    public override QueryBuilder<C> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C>) base.Has(target);
    }


    public override QueryBuilder<C> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C>) base.Not<T>(target);
    }


    public override QueryBuilder<C> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C>) base.Not(target);
    }


    public override QueryBuilder<C> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C>) base.Any<T>(target);
    }


    public override QueryBuilder<C> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C>) base.Any(target);
    }


    public Query<C> Build()
    {
        return (Query<C>) World.GetQuery(Mask, CreateQuery);
    }
}


public sealed class QueryBuilder<C1, C2> : QueryBuilder
{
    private static readonly Func<World, Mask, List<Archetype>, Query> CreateQuery =
        (world, mask, matchingTables) => new Query<C1, C2>(world, mask, matchingTables);


    internal QueryBuilder(World world, Entity match1, Entity match2) : base(world)
    {
        Has<C1>(match1).Has<C2>(match2);
    }


    public override QueryBuilder<C1, C2> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Has(target);
    }


    public override QueryBuilder<C1, C2> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Not(target);
    }


    public override QueryBuilder<C1, C2> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Any(target);
    }


    public Query<C1, C2> Build()
    {
        return (Query<C1, C2>) World.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3> : QueryBuilder
{
    private static readonly Func<World, Mask, List<Archetype>, Query> CreateQuery =
        (world, mask, matchingTables) => new Query<C1, C2, C3>(world, mask, matchingTables);


    internal QueryBuilder(World world, Entity match1, Entity match2, Entity match3) : base(world)
    {
        Has<C1>(match1).Has<C2>(match2).Has<C3>(match3);
    }


    public override QueryBuilder<C1, C2, C3> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2, C3> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Has(target);
    }


    public override QueryBuilder<C1, C2, C3> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2, C3> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Not(target);
    }


    public override QueryBuilder<C1, C2, C3> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2, C3> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Any(target);
    }


    public Query<C1, C2, C3> Build()
    {
        return (Query<C1, C2, C3>) World.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4> : QueryBuilder
{
    private static readonly Func<World, Mask, List<Archetype>, Query> CreateQuery =
        (world, mask, matchingTables) => new Query<C1, C2, C3, C4>(world, mask, matchingTables);


    internal QueryBuilder(World world, Entity match1, Entity match2, Entity match3, Entity match4) : base(world)
    {
        Has<C1>(match1).Has<C2>(match2).Has<C3>(match3).Has<C4>(match4);
    }


    public override QueryBuilder<C1, C2, C3, C4> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Has(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Not(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Any(target);
    }


    public Query<C1, C2, C3, C4> Build()
    {
        return (Query<C1, C2, C3, C4>) World.GetQuery(Mask, CreateQuery);
    }
}

public sealed class QueryBuilder<C1, C2, C3, C4, C5> : QueryBuilder
{
    private static readonly Func<World, Mask, List<Archetype>, Query> CreateQuery =
        (world, mask, matchingTables) => new Query<C1, C2, C3, C4, C5>(world, mask, matchingTables);
    
    
    internal QueryBuilder(World world, Entity match1, Entity match2, Entity match3, Entity match4, Entity match5) : base(world)
    {
        Has<C1>(match1).Has<C2>(match2).Has<C3>(match3).Has<C4>(match4).Has<C5>(match5);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Has<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Has<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Has(target);
    }

    public override QueryBuilder<C1, C2, C3, C4, C5> Not<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Not<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Not(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Any<T>(Entity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Any<T>(target);
    }


    public override QueryBuilder<C1, C2, C3, C4, C5> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Any(target);
    }


    public Query<C1, C2, C3, C4, C5> Build()
    {
        return (Query<C1, C2, C3, C4, C5>) World.GetQuery(Mask, CreateQuery);
    }
}