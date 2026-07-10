// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public partial class Aspect : IAspect
{
    #region IAspect

    /// <summary>
    /// Universal Query, matching all Entities that are members of this Aspect.
    /// </summary>
    public Query All
    {
        get
        {
            using var mask = MaskPool.Rent();
            mask.Has(TypeExpression.Of<Identity>(Match.Plain));
            return CompileQuery(mask);
        }
    }


    /// <inheritdoc cref="fennecs.World.Query()"/>
    public QueryBuilder Query() => new QueryBuilder(World).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1}()"/>
    public QueryBuilder<C1> Query<C1>() where C1 : notnull => new QueryBuilder<C1>(World, Match.Plain).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1}(Match)"/>
    public QueryBuilder<C1> Query<C1>(Match match) where C1 : notnull => new QueryBuilder<C1>(World, match).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2}()"/>
    public QueryBuilder<C1, C2> Query<C1, C2>() where C1 : notnull where C2 : notnull => new QueryBuilder<C1, C2>(World, Match.Plain).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2}(Match,Match)"/>
    public QueryBuilder<C1, C2> Query<C1, C2>(Match match1, Match match2) where C1 : notnull where C2 : notnull => new QueryBuilder<C1, C2>(World, match1, match2).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3}()"/>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : notnull where C2 : notnull where C3 : notnull => new QueryBuilder<C1, C2, C3>(World, Match.Plain).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3}(Match,Match,Match)"/>
    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Match match1, Match match2, Match match3) where C1 : notnull where C2 : notnull where C3 : notnull => new QueryBuilder<C1, C2, C3>(World, match1, match2, match3).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4}()"/>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => new QueryBuilder<C1, C2, C3, C4>(World, Match.Plain).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4}(Match,Match,Match,Match)"/>
    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Match match1, Match match2, Match match3, Match match4) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull => new QueryBuilder<C1, C2, C3, C4>(World, match1, match2, match3, match4).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4,C5}()"/>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull => new QueryBuilder<C1, C2, C3, C4, C5>(World, Match.Plain).Within(this);

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4,C5}(Match,Match,Match,Match,Match)"/>
    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Match match1, Match match2, Match match3, Match match4, Match match5) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull => new QueryBuilder<C1, C2, C3, C4, C5>(World, match1, match2, match3, match4, match5).Within(this);

    #endregion


    #region Streamable

    /// <inheritdoc cref="fennecs.World.Stream{C}(Match)"/>
    public Stream<C> Stream<C>(Match match = default) where C : notnull => Query<C>(match).Stream();

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1}(Match,Match)"/>
    public Stream<C0, C1> Stream<C0, C1>(Match match0, Match match1) where C0 : notnull where C1 : notnull
        => Query<C0, C1>(match0, match1).Stream();

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1}(Match)"/>
    public Stream<C0, C1> Stream<C0, C1>(Match matchAll = default) where C0 : notnull where C1 : notnull
        => Stream<C0, C1>(matchAll, matchAll);

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1,C2}(Match,Match,Match)"/>
    public Stream<C0, C1, C2> Stream<C0, C1, C2>(Match match0, Match match1, Match match2) where C0 : notnull where C1 : notnull where C2 : notnull
        => Query<C0, C1, C2>(match0, match1, match2).Stream();

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1,C2}(Match)"/>
    public Stream<C0, C1, C2> Stream<C0, C1, C2>(Match matchAll = default) where C0 : notnull where C1 : notnull where C2 : notnull
        => Stream<C0, C1, C2>(matchAll, matchAll, matchAll);

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1,C2,C3}(Match,Match,Match,Match)"/>
    public Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match match0, Match match1, Match match2, Match match3) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull
        => Query<C0, C1, C2, C3>(match0, match1, match2, match3).Stream();

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1,C2,C3}(Match)"/>
    public Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match matchAll = default) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull
        => Stream<C0, C1, C2, C3>(matchAll, matchAll, matchAll, matchAll);

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1,C2,C3,C4}(Match,Match,Match,Match,Match)"/>
    public Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match match0, Match match1, Match match2, Match match3, Match match4)
        where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull
        => Query<C0, C1, C2, C3, C4>(match0, match1, match2, match3, match4).Stream();

    /// <inheritdoc cref="fennecs.World.Stream{C0,C1,C2,C3,C4}(Match)"/>
    public Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match matchAll = default) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull
        => Stream<C0, C1, C2, C3, C4>(matchAll, matchAll, matchAll, matchAll, matchAll);

    #endregion
}
