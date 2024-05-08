// SPDX-License-Identifier: MIT

// ReSharper disable MemberCanBePrivate.Global

using fennecs.pools;

namespace fennecs;

/// <summary>
/// Fluent builder interface which compiles Queries via its Build() method.
/// </summary>
/// <para>
/// A QueryBuilder serves to specify inclusion/exclusion criteria for entities and
/// their components, and then compiles them into fast Queries via its Build() method.
/// </para>
/// <para>
/// Example use:
/// <code>
/// <![CDATA[
/// var world = new fennecs.World();
/// 
/// var selectedHealthBars = world.Query<HP, HPBar>()
///     .Has<Selected>()
///     .Any<Player>()
///     .Any<NPC>()
///     .Not<Disabled>()
///     .Build();
///
/// 
/// selectedHealthBars.For(
///     (ref HP hp, ref HPBar bar) =>
///     {
///         bar.Fill = hp.Cur/hp.Max;
///     });
/// ]]>
/// </code>
/// </para>
/// <remarks>
/// Compilation is reasonably fast, and cached.
/// A Query with the same Mask of criteria will be pulled from the cache if it was already compiled. 
/// You can compile multiple queries from the same builder (adding more criteria as you go).
/// </remarks>
public abstract class QueryBuilder : IDisposable
{
    #region Internals

    internal readonly World World;
    internal readonly Mask Mask = MaskPool.Rent();

    private protected readonly PooledList<TypeExpression> StreamTypes = PooledList<TypeExpression>.Rent();

    internal QueryBuilder(World world)
    {
        World = world;
    }

    private protected void Outputs<T>(Identity target = default)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        StreamTypes.Add(typeExpression);
        Mask.Has(typeExpression);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Mask.Dispose();
        StreamTypes.Dispose();
    }

    #endregion

    
    #region Public API

    /// <summary>
    /// Builds (compiles) the Query from the current state of the QueryBuilder.
    /// </summary>
    /// <remarks>
    /// This method is covariant, so you will get the appropriate stream Query subclass
    /// depending on the Stream Types (type parameters) you passed to <see cref="fennecs.World.Query{C}()"/>
    /// or any of its overloads.
    /// </remarks>
    /// <returns>compiled query (you can compile more than one query from the same builder)</returns>
    public abstract Query Build();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual QueryBuilder Has<T>(Identity target = default)
    {
        var typeExpression = TypeExpression.Of<T>(target);
        if (StreamTypes.Contains(typeExpression)) throw new InvalidOperationException($"Type {typeExpression} is already an output of this query.");

        Mask.Has(typeExpression);
        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual QueryBuilder Has<T>(T target) where T : class
    {
        Mask.Has(TypeExpression.Of<T>(Identity.Of(target)));
        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual QueryBuilder Not<T>(Identity target = default)
    {
        Mask.Not(TypeExpression.Of<T>(target));
        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual QueryBuilder Not<T>(T target) where T : class
    {
        var typeExpression = TypeExpression.Of<T>(Identity.Of(target));

        Mask.Not(typeExpression);
        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual QueryBuilder Any<T>(Identity target = default)
    {
        Mask.Any(TypeExpression.Of<T>(target));
        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual QueryBuilder Any<T>(T target) where T : class
    {
        Mask.Any(TypeExpression.Of<T>(Identity.Of(target)));
        return this;
    }

    #endregion
}

/// <inheritdoc />
public sealed class QueryBuilder<C1> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match = default) : base(world)
    {
        Outputs<C1>(match);
    }


    /// <inheritdoc />
    public override Query<C1> Build()
    {
        return (Query<C1>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1>) base.Has<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1>) base.Has(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1>) base.Not<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1>) base.Not(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1>) base.Any<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1>) base.Any(target);
    }
}

/// <inheritdoc />
public sealed class QueryBuilder<C1, C2> : QueryBuilder
{
    private static readonly Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> CreateQuery =
        (world, streamTypes, mask, matchingTables) => new Query<C1, C2>(world, streamTypes, mask, matchingTables);


    internal QueryBuilder(World world, Identity match1, Identity match2) : base(world)
    {
        Outputs<C1>(match1);
        Outputs<C2>(match2);
    }


    /// <inheritdoc />
    public override Query<C1, C2> Build()
    {
        return (Query<C1, C2>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Has<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Has(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Not<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Not(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2>) base.Any<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2>) base.Any(target);
    }
}

/// <inheritdoc />
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


    /// <inheritdoc />
    /// <inheritdoc />
    public override Query<C1, C2, C3> Build()
    {
        return (Query<C1, C2, C3>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Has<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Has(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Not<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Not(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3>) base.Any<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3>) base.Any(target);
    }
}

/// <inheritdoc />
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


    /// <inheritdoc />
    public override Query<C1, C2, C3, C4> Build()
    {
        return (Query<C1, C2, C3, C4>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Has<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Has(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Not<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Not(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Any<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4>) base.Any(target);
    }
}

/// <inheritdoc />
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


    /// <inheritdoc />
    public override Query<C1, C2, C3, C4, C5> Build()
    {
        return (Query<C1, C2, C3, C4, C5>) World.GetQuery(StreamTypes, Mask, CreateQuery);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4, C5> Has<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Has<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4, C5> Has<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Has(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4, C5> Not<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Not<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4, C5> Not<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Not(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4, C5> Any<T>(Identity target = default)
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Any<T>(target);
    }


    /// <inheritdoc />
    public override QueryBuilder<C1, C2, C3, C4, C5> Any<T>(T target) where T : class
    {
        return (QueryBuilder<C1, C2, C3, C4, C5>) base.Any(target);
    }
}