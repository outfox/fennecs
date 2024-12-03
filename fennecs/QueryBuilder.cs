using fennecs.pools;

namespace fennecs;

/// <summary>
/// A QueryBuilder provides a fluent API to construct Queries into a <see cref="fennecs.World"/>.
/// Queries use Query Expressions to Match Entities based on their Components, Relations, or Object Links.
/// </summary>
/// <typeparam name="QB">F-bound polymorphic / CRTP type reference to own type, so inheritors
/// can inherit all their methods with the appropriate return types</typeparam>
public abstract class QueryBuilderBase<QB> : IDisposable where QB : QueryBuilderBase<QB>
{
    #region Builder State

    private Mask _mask = MaskPool.Rent();
    private bool _disposed;

    private readonly World _world;

    /// <summary>
    /// A QueryBuilder provides a fluent API to construct Queries into a <see cref="fennecs.World"/>.
    /// Queries use Query Expressions to Match Entities based on their Components, Relations, or Object Links.
    /// </summary>
    /// <param name="world"><see cref="fennecs.World"/> to build queries for</param>
    /// <param name="streamTypes">list of types that must be guaranteed to be on the Query's Mask for Stream Creation</param>
    private protected QueryBuilderBase(World world, Span<MatchExpression> streamTypes)
    {
        _world = world;
        foreach (var type in streamTypes) _mask.Has(type);
        
        // TODO: need to agree with myself what I can do about including Entity or not.
        _mask.HasTypes.Add(MatchExpression.Of<Entity>(default));
    }

    #endregion

    #region Compilation Interface

    /// <summary>
    /// Builds (compiles) the Query from the current state of the QueryBuilder.
    /// </summary>
    /// <remarks>
    /// This method is covariant, so you will get the appropriate stream Query subclass
    /// depending on the Stream Types (type parameters) you passed to <see cref="fennecs.World.Query{C}()"/>
    /// or any of its overloads.
    /// </remarks>
    /// <returns>compiled query (you can compile more than one query from the same builder)</returns>
    public Query Compile() => _world.CompileQuery(_mask);

    #endregion

    #region Builder Interface

    /// <summary>
    /// QueryBuilder includes only Entities that have the given Component, Relation, or Object Link.
    /// </summary>
    /// <param name="match">defaults th Plain Components. Can be a match wildcard or specific relation target / or object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this or conflict with it</exception>
    public QB Has<T>(Match match = default)
    {
        _mask.Has(MatchExpression.Of<T>(match));
        return (QB)this;
    }

    
    /// <summary>
    /// QueryBuilder includes only Entities that have the given Component, Relation, or Object Link.
    /// </summary>
    /// <param name="link">an object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this or conflict with it</exception>
    public QB Has<T>(Link<T> link) where T : class
    {
        _mask.Has(MatchExpression.Of<T>(link));
        return (QB)this;
    }



    /// <summary>
    /// Exclude all Entities that have the given Component or Relation.
    /// </summary>
    /// <param name="match">defaults th Plain Components. Can be a match wildcard or specific relation target / or object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this or conflict with it</exception>
    public QB Not<T>(Match match = default)
    {
        _mask.Not(MatchExpression.Of<T>(match));
        return (QB)this;
    }

    /// <summary>
    /// Exclude all Entities that have the given Component or Relation.
    /// </summary>
    /// <param name="link">an object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this or conflict with it</exception>
    public QB Not<T>(Link<T> link) where T : class
    {
        _mask.Not(MatchExpression.Of<T>(link));
        return (QB)this;
    }

    /// <summary>
    /// Include Entities that have the given Component or Relation, or any other Relation that is
    /// given in other <see cref="Any{T}(Match)"/> calls.
    /// </summary>
    /// <param name="match">defaults th Plain Components. Can be a match wildcard or specific relation target / or object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this or conflict with it</exception>
    public QB Any<T>(Match match = default)
    {
        _mask.Any(MatchExpression.Of<T>(match));
        return (QB)this;
    }

    /// <summary>
    /// Include Entities that have the given Component or Relation, or any other Relation that is
    /// given in other <see cref="Any{T}(Match)"/> calls.
    /// </summary>
    /// <param name="link">an object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this or conflict with it</exception>
    public QB Any<T>(Link<T> link) where T : class
    {
        _mask.Any(MatchExpression.Of<T>(link));
        return (QB)this;
    }
    
    #endregion

    # region IDisposable

    /// <inheritdoc cref="IDisposable"/>
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        _disposed = true;
        _mask.Dispose();
        _mask = null!;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IDisposable"/>
    ~QueryBuilderBase() => Dispose();

    #endregion
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class QueryBuilder(World world) : QueryBuilderBase<QueryBuilder>(world, []);

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class QueryBuilder<C1>(World world, Match match1 = default)
    : QueryBuilderBase<QueryBuilder<C1>>(world, [MatchExpression.Of<C1>(match1)])
    where C1 : notnull
{
    /// <include file='_docs.xml' path='members/member[@name="T:Stream"]'/>
    public Stream<C1> Stream() => Compile().Stream<C1>(match1);
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class QueryBuilder<C1, C2>(World world, Match match1, Match match2)
    : QueryBuilderBase<QueryBuilder<C1, C2>>(world, [MatchExpression.Of<C1>(match1), MatchExpression.Of<C2>(match2)])
    where C1 : notnull
    where C2 : notnull
{
    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public QueryBuilder(World world, Match matchAll = default) : this(world, matchAll, matchAll) { }

    /// <include file='_docs.xml' path='members/member[@name="T:Stream"]'/>
    public Stream<C1, C2> Stream() => Compile().Stream<C1, C2>(match1, match2);
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class QueryBuilder<C1, C2, C3>(World world, Match match1, Match match2, Match match3)
    : QueryBuilderBase<QueryBuilder<C1, C2, C3>>(world, [MatchExpression.Of<C1>(match1), MatchExpression.Of<C2>(match2), MatchExpression.Of<C3>(match3)])
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
{
    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public QueryBuilder(World world, Match matchAll = default) : this(world, matchAll, matchAll, matchAll) { }

    /// <include file='_docs.xml' path='members/member[@name="T:Stream"]'/>
    public Stream<C1, C2, C3> Stream() => Compile().Stream<C1, C2, C3>(match1, match2, match3);
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class QueryBuilder<C1, C2, C3, C4>(World world, Match match1, Match match2, Match match3, Match match4)
    : QueryBuilderBase<QueryBuilder<C1, C2, C3, C4>>(world, [ MatchExpression.Of<C1>(match1), MatchExpression.Of<C2>(match2), MatchExpression.Of<C3>(match3), MatchExpression.Of<C4>(match4) ])
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
    where C4 : notnull
{
    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public QueryBuilder(World world, Match matchAll = default) : this(world, matchAll, matchAll, matchAll, matchAll) { }

    /// <include file='_docs.xml' path='members/member[@name="T:Stream"]'/>
    public Stream<C1, C2, C3, C4> Stream() => Compile().Stream<C1, C2, C3, C4>(match1, match2, match3, match4);
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class QueryBuilder<C1, C2, C3, C4, C5>(World world, Match match1, Match match2, Match match3, Match match4, Match match5)
    : QueryBuilderBase<QueryBuilder<C1, C2, C3, C4, C5>>(world, [ MatchExpression.Of<C2>(match2), MatchExpression.Of<C3>(match3), MatchExpression.Of<C4>(match4), MatchExpression.Of<C5>(match5) ])
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
    where C4 : notnull
    where C5 : notnull
{
    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public QueryBuilder(World world, Match matchAll = default) : this(world, matchAll, matchAll, matchAll, matchAll, matchAll) { }

    /// <include file='_docs.xml' path='members/member[@name="T:Stream"]'/>
    public Stream<C1, C2, C3, C4, C5> Stream() => Compile().Stream<C1, C2, C3, C4, C5>(match1, match2, match3, match4, match5);
}
