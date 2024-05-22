namespace fennecs;

public partial class World
{
    /// <summary>
    /// Creates a fluent Builder for a query with only the Identity component as its sole Stream Type.
    /// </summary>
    /// <remarks>
    /// A query with zero stream types seemed nonsensical. 💌 Feedback is welcome, what's your use case?
    /// </remarks>
    /// <returns><see cref="QueryBuilder{Identity}"/></returns>
    public QueryBuilder<Identity> Query()
    {
        return new QueryBuilder<Identity>(this);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with one output Stream Type.
    /// </summary>
    /// <remarks>
    /// Compile the Query from the builder using its <see cref="QueryBuilder.Build"/> method.
    /// </remarks>
    /// <typeparam name="C1">component type that runners of this query will have access to</typeparam>
    /// <returns><see cref="QueryBuilder{C1}"/></returns>
    public QueryBuilder<C1> Query<C1>() where C1 : notnull
    {
        return new QueryBuilder<C1>(this, Match.Any);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with one output Stream Type.
    /// A <see cref="Match"/> expression can be specified to limit the components matched to Stream Types, for instance:
    /// <see cref="Match.Any"/>, <see cref="Match.Entity"/>, <see cref="Match.Object"/>, <see cref="Match.Plain"/> or <see cref="Match.Target"/>.
    /// </summary>
    /// <remarks>
    /// This bakes the Match Expression into the compiled Query, which is slightly more performant than using Query.<see cref="fennecs.Query.Subset{T}"/> and much more performant than using Query.<see cref="fennecs.Query.Filtered"/>.
    /// </remarks>
    /// <param name="match">Match Expression</param>
    /// <typeparam name="C1">component type that runners of this query will have access to</typeparam>
    /// <returns><see cref="QueryBuilder{C1}"/></returns>
    public QueryBuilder<C1> Query<C1>(Identity match) where C1 : notnull
    {
        return new QueryBuilder<C1>(this, match);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with two output Stream Types.
    /// </summary>
    /// <remarks>
    /// Compile the Query from the builder using its <see cref="QueryBuilder.Build"/> method.
    /// </remarks>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2}"/></returns>
    public QueryBuilder<C1, C2> Query<C1, C2>() where C1 : notnull where C2 : notnull
    {
        return new QueryBuilder<C1, C2>(this, Match.Any, Match.Any);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with two output Stream Types.
    /// A <see cref="Match"/> expression can be specified to limit the components matched to Stream Types, for instance:
    /// <see cref="Match.Any"/>, <see cref="Match.Entity"/>, <see cref="Match.Object"/>, <see cref="Match.Plain"/> or <see cref="Match.Target"/>.
    /// </summary>
    /// <remarks>
    /// This bakes the Match Expression into the compiled Query, which is slightly more performant than using Query.<see cref="fennecs.Query.Subset{T}"/> and much more performant than using Query.<see cref="fennecs.Query.Filtered"/>.
    /// </remarks>
    /// <param name="match1">Match Expression for Stream Type 1</param>
    /// <param name="match2">Match Expression for Stream Type 2</param>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2}"/></returns>
    public QueryBuilder<C1, C2> Query<C1, C2>(Identity match1, Identity match2) where C1 : notnull where C2 : notnull
    {
        return new QueryBuilder<C1, C2>(this, match1, match2);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with three output Stream Types.
    /// </summary>
    /// <remarks>
    /// Compile the Query from the builder using its <see cref="QueryBuilder.Build"/> method.
    /// </remarks>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <typeparam name="C3">component Stream Type 2</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2, C3}"/></returns>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : notnull where C2 : notnull where C3 : notnull
    {
        return new QueryBuilder<C1, C2, C3>(this, Match.Any, Match.Any, Match.Any);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with three output Stream Types.
    /// A <see cref="Match"/> expression can be specified to limit the components matched to Stream Types, for instance:
    /// <see cref="Match.Any"/>, <see cref="Match.Entity"/>, <see cref="Match.Object"/>, <see cref="Match.Plain"/> or <see cref="Match.Target"/>.
    /// </summary>
    /// <remarks>
    /// This bakes the Match Expression into the compiled Query, which is slightly more performant than using Query.<see cref="fennecs.Query.Subset{T}"/> and much more performant than using Query.<see cref="fennecs.Query.Filtered"/>.
    /// </remarks>
    /// <param name="match1">Match Expression for Stream Type 1</param>
    /// <param name="match2">Match Expression for Stream Type 2</param>
    /// <param name="match3">Match Expression for Stream Type 3</param>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <typeparam name="C3">component Stream Type 3</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2, C3}"/></returns>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Identity match1, Identity match2, Identity match3) where C1 : notnull where C2 : notnull where C3 : notnull
    {
        return new QueryBuilder<C1, C2, C3>(this, match1, match2, match3);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with four output Stream Types.
    /// </summary>
    /// <remarks>
    /// Compile the Query from the builder using its <see cref="QueryBuilder.Build"/> method.
    /// </remarks>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <typeparam name="C3">component Stream Type 3</typeparam>
    /// <typeparam name="C4">component Stream Type 4</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2, C3, C4}"/></returns>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull
    {
        return new QueryBuilder<C1, C2, C3, C4>(this, Match.Any, Match.Any, Match.Any, Match.Any);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with four output Stream Types.
    /// A <see cref="Match"/> expression can be specified to limit the components matched to Stream Types, for instance:
    /// <see cref="Match.Any"/>, <see cref="Match.Entity"/>, <see cref="Match.Object"/>, <see cref="Match.Plain"/> or <see cref="Match.Target"/>.
    /// </summary>
    /// <remarks>
    /// This bakes the Match Expression into the compiled Query, which is slightly more performant than using Query.<see cref="fennecs.Query.Subset{T}"/> and much more performant than using Query.<see cref="fennecs.Query.Filtered"/>.
    /// </remarks>
    /// <param name="match1">Match Expression for Stream Type 1</param>
    /// <param name="match2">Match Expression for Stream Type 2</param>
    ///  <param name="match3">Match Expression for Stream Type 3</param>
    ///  <param name="match4">Match Expression for Stream Type 4</param>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    ///  <typeparam name="C3">component Stream Type 3</typeparam>
    ///  <typeparam name="C4">component Stream Type 4</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2, C3, C4}"/></returns>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Identity match1, Identity match2, Identity match3, Identity match4) where C2 : notnull where C1 : notnull where C3 : notnull where C4 : notnull
    {
        return new QueryBuilder<C1, C2, C3, C4>(this, match1, match2, match3, match4);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with five output Stream Types.
    /// </summary>
    /// <remarks>
    /// Compile the Query from the builder using its <see cref="QueryBuilder.Build"/> method.
    /// </remarks>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <typeparam name="C3">component Stream Type 3</typeparam>
    /// <typeparam name="C4">component Stream Type 4</typeparam>
    /// <typeparam name="C5">component Stream Type 5</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2, C3, C4, C5}"/></returns>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this, Match.Any, Match.Any, Match.Any, Match.Any, Match.Any);
    }


    /// <summary>
    /// Creates a fluent Builder for a query with five output Stream Types.
    /// A <see cref="Match"/> expression can be specified to limit the components matched to Stream Types, for instance:
    /// <see cref="Match.Any"/>, <see cref="Match.Entity"/>, <see cref="Match.Object"/>, <see cref="Match.Plain"/> or <see cref="Match.Target"/>.
    /// </summary>
    /// <remarks>
    /// This bakes the Match Expression into the compiled Query, which is slightly more performant than using Query.<see cref="fennecs.Query.Subset{T}"/> and much more performant than using Query.<see cref="fennecs.Query.Filtered"/>.
    /// </remarks>
    /// <param name="match1">Match Expression for Stream Type 1</param>
    /// <param name="match2">Match Expression for Stream Type 2</param>
    /// <param name="match3">Match Expression for Stream Type 3</param>
    /// <param name="match4">Match Expression for Stream Type 4</param>
    /// <param name="match5">Match Expression for Stream Type 5</param>
    /// <typeparam name="C1">component Stream Type 1</typeparam>
    /// <typeparam name="C2">component Stream Type 2</typeparam>
    /// <typeparam name="C3">component Stream Type 3</typeparam>
    /// <typeparam name="C4">component Stream Type 4</typeparam>
    /// <typeparam name="C5">component Stream Type 5</typeparam>
    /// <returns><see cref="QueryBuilder{C1, C2, C3, C4, C5}"/></returns>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Identity match1, Identity match2, Identity match3, Identity match4, Identity match5) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this, match1, match2, match3, match4, match5);
    }
}