namespace fennecs;

public partial class World
{
    /// <summary>
    /// <para>
    /// Creates a fluent Builder that can be used to configure and compile one or multiple Queries.
    /// </para>
    /// <para>
    /// ℹ️ QueryBuilders implement <see cref="IDisposable"/> to allow optimizing for resource pooling.
    /// </para>
    /// </summary>
    public QueryBuilder Query() => new(this);

    
    /// <inheritdoc cref="Query()"/>
    /// <remarks>
    /// <para>
    /// This and other builder with type Parameters tracks Match Expressions for potential Stream Types.
    /// They default to <see cref="Identity.Any"/>, but can be customized using the appropriate overloads.
    /// You may also narrow the matching down using additional Query Expressions.
    /// </para>
    /// <para>
    /// Call <see cref="QueryBuilder{C1}.Stream"/> to Compile and immediately return a Stream View for this Query.
    /// </para>
    /// </remarks>
    /// <typeparam name="C1">(C2 .. Cx) - component type(s) that the Stream View will expose</typeparam>
    /// <returns><see cref="QueryBuilder{C1}"/></returns>
// A set of generic QueryBuilder methods with type constraints for building queries with varying numbers of targets.
    public QueryBuilder<C1> Query<C1>() where C1 : notnull => new(this, Match.Plain);

    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1> Query<C1>(Match match) where C1 : notnull => new(this, match);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2> Query<C1, C2>() where C1 : notnull where C2 : notnull => new(this, Match.Plain);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2> Query<C1, C2>(Match matchAll) where C1 : notnull where C2 : notnull => new(this, matchAll,matchAll);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2> Query<C1, C2>(Match match1, Match match2) where C1 : notnull where C2 : notnull => new(this, match1, match2);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : notnull where C2 : notnull where C3 : notnull => new(this, Match.Plain);

    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Match matchAll) where C1 : notnull where C2 : notnull where C3 : notnull => new(this, matchAll, matchAll, matchAll);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Match match1, Match match2, Match match3) where C1 : notnull where C2 : notnull where C3 : notnull => new(this, match1, match2, match3);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => new(this, Match.Plain);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Match matchAll) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => new(this, matchAll, matchAll, matchAll, matchAll);
     
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Match match1, Match match2, Match match3, Match match4) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => new(this, match1, match2, match3, match4);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull => new(this, Match.Plain);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Match matchAll) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull => new(this, matchAll, matchAll, matchAll, matchAll, matchAll);
    
    /// <inheritdoc cref="Query{C1}()"/>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Match match1, Match match2, Match match3, Match match4, Match match5) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull => new(this, match1, match2, match3, match4, match5);
    
}