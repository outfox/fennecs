using fennecs.pools;

namespace fennecs;

/// <summary>
/// A QueryBuilder provides a fluent API to define a Query.
/// </summary>
/// <typeparam name="QB">F-bound polymorphic / CRTP type reference to own type, so inheritors can inherit all their methods with the appropriate return types</typeparam>
/// <param name="world"><see cref="fennecs.World"/> to build queries for</param>
public abstract class QueryBuilderBase<QB>(World world) : IDisposable where QB : QueryBuilderBase<QB>
{
    private readonly Mask _mask = MaskPool.Rent();

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
    public Query Compile() => world.CompileQueryNew(_mask.Clone());

    #endregion

    #region Builder Interface

    /// <summary>
    /// Include only Entities that have the given Component, Relation, or Object Link.
    /// </summary>
    /// <param name="match">defaults th Plain Components. Can be a match wildcard or specific relation target / or object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this</exception>
    public QB Has<T>(Match match = default)
    {
        var typeExpression = TypeExpression.Of<T>(match);
        _mask.Has(typeExpression);
        return (QB)this;
    }


    /// <summary>
    /// Exclude all Entities that have the given Component or Relation.
    /// </summary>
    /// <param name="match">defaults th Plain Components. Can be a match wildcard or specific relation target / or object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this</exception>
    public QB Not<T>(Match match = default)
    {
        _mask.Not(TypeExpression.Of<T>(match));
        return (QB)this;
    }

    /// <summary>
    /// Include Entities that have the given Component or Relation, or any other Relation that is
    /// given in other <see cref="Any{T}(fennecs.Match)"/> calls.
    /// </summary>
    /// <param name="match">defaults th Plain Components. Can be a match wildcard or specific relation target / or object link</param>
    /// <typeparam name="T">component's backing type</typeparam>
    /// <returns>itself (fluent pattern)</returns>
    /// <exception cref="InvalidOperationException">if the StreamTypes already cover this</exception>
    public QB Any<T>(Match match = default)
    {
        _mask.Any(TypeExpression.Of<T>(match));
        return (QB)this;
    }

    /// <summary>
    /// Disable conflict checks for subsequent Query Expressions.
    /// </summary>
    /// <remarks>
    /// The builder will no longer throw exceptions if the inclusion or exclusion of a type would result in a guaranteed empty set,
    /// or if redundant statements are made. This is useful for programmatically created queries, where duplicate or conflicting
    /// Query Expressions can be intentional or impossible to prevent.
    /// </remarks>
    /// <returns>itself (fluent pattern)</returns>
    public QB Unchecked()
    {
        _mask.safety = false;
        return (QB)this;
    }

    #endregion

    # region IDisposable

    /// <inheritdoc cref="IDisposable"/>
    public void Dispose()
    {
        _mask.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class NewQueryBuilder(World world) : QueryBuilderBase<NewQueryBuilder>(world);

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class NewQueryBuilder<C1>(World world)
    : QueryBuilderBase<NewQueryBuilder<C1>>(world)
    where C1 : notnull
{
    private readonly Match _match1 = Match.Any;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match match1) : this(world) => _match1 = match1;

    /// <summary>
    /// Get a Stream View of the Query to iterate its entities.
    /// </summary>
    public Stream<C1> Stream()
    {
        var query = Compile();
        return query.Stream<C1>(_match1);
    }
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class NewQueryBuilder<C1, C2>(World world)
    : QueryBuilderBase<NewQueryBuilder<C1, C2>>(world)
    where C1 : notnull
    where C2 : notnull
{
    private readonly Match _match1 = Match.Any;
    private readonly Match _match2 = Match.Any;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match matchAll) : this(world) => _match1 = _match2 = matchAll;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match match1, Match match2) : this(world)
    {
        _match1 = match1;
        _match2 = match2;
    }

    /// <summary>
    /// Get a Stream View of the Query to iterate its entities.
    /// </summary>
    public Stream<C1, C2> Stream()
    {
        var query = Compile();
        return query.Stream<C1, C2>(_match1, _match2);
    }
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class NewQueryBuilder<C1, C2, C3>(World world)
    : QueryBuilderBase<NewQueryBuilder<C1, C2, C3>>(world)
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
{
    private readonly Match _match1 = Match.Any;
    private readonly Match _match2 = Match.Any;
    private readonly Match _match3 = Match.Any;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match matchAll) : this(world) => _match1 = _match2 = _match3 = matchAll;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match match1, Match match2, Match match3) : this(world)
    {
        _match1 = match1;
        _match2 = match2;
        _match3 = match3;
    }

    /// <summary>
    /// Get a Stream View of the Query to iterate its entities.
    /// </summary>
    public Stream<C1, C2, C3> Stream()
    {
        var query = Compile();
        return query.Stream<C1, C2, C3>(_match1, _match2, _match3);
    }
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class NewQueryBuilder<C1, C2, C3, C4>(World world)
    : QueryBuilderBase<NewQueryBuilder<C1, C2, C3, C4>>(world)
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
    where C4 : notnull
{
    private readonly Match _match1 = Match.Any;
    private readonly Match _match2 = Match.Any;
    private readonly Match _match3 = Match.Any;
    private readonly Match _match4 = Match.Any;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match matchAll) : this(world) => _match1 = _match2 = _match3 = _match4 = matchAll;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match match1, Match match2, Match match3, Match match4) : this(world)
    {
        _match1 = match1;
        _match2 = match2;
        _match3 = match3;
        _match4 = match4;
    }

    /// <summary>
    /// Get a Stream View of the Query to iterate its entities.
    /// </summary>
    public Stream<C1, C2, C3, C4> Stream()
    {
        var query = Compile();
        return query.Stream<C1, C2, C3, C4>(_match1, _match2, _match3, _match4);
    }
}

/// <inheritdoc cref="QueryBuilderBase{QB}"/>
public class NewQueryBuilder<C1, C2, C3, C4, C5>(World world)
    : QueryBuilderBase<NewQueryBuilder<C1, C2, C3, C4, C5>>(world)
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
    where C4 : notnull
    where C5 : notnull
{
    private readonly Match _match1 = Match.Any;
    private readonly Match _match2 = Match.Any;
    private readonly Match _match3 = Match.Any;
    private readonly Match _match4 = Match.Any;
    private readonly Match _match5 = Match.Any;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match matchAll) : this(world) => _match1 = _match2 = _match3 = _match4 = _match5 = matchAll;

    /// <inheritdoc cref="QueryBuilderBase{QB}"/>
    public NewQueryBuilder(World world, Match match1, Match match2, Match match3, Match match4, Match match5) : this(world)
    {
        _match1 = match1;
        _match2 = match2;
        _match3 = match3;
        _match4 = match4;
        _match5 = match5;
    }

    /// <summary>
    /// Get a Stream View of the Query to iterate its entities.
    /// </summary>
    public Stream<C1, C2, C3, C4, C5> Stream()
    {
        var query = Compile();
        return query.Stream<C1, C2, C3, C4, C5>(_match1, _match2, _match3, _match4, _match5);
    }
}