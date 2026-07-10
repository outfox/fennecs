// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// The Query and Stream surface shared by <see cref="Aspect"/> and <see cref="fennecs.World"/>.
/// A World exposes this surface by delegating to its <see cref="fennecs.World.Main"/> Aspect
/// (resolving other Aspects from the queried types), so code can accept either.
/// </summary>
public interface IAspect : Streamable
{
    /// <summary>
    /// The name of this Aspect (a World's own name doubles as its identity here).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The World whose Entities this Aspect stores components for.
    /// </summary>
    World World { get; }

    /// <summary>
    /// Universal Query, matching all Entities in this Aspect.
    /// </summary>
    Query All { get; }

    /// <summary>
    /// The number of Entities.
    /// </summary>
    int Count { get; }

    /// <inheritdoc cref="fennecs.World.Query()"/>
    QueryBuilder Query();

    /// <inheritdoc cref="fennecs.World.Query{C1}()"/>
    QueryBuilder<C1> Query<C1>() where C1 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1}(Match)"/>
    QueryBuilder<C1> Query<C1>(Match match) where C1 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2}()"/>
    QueryBuilder<C1, C2> Query<C1, C2>() where C1 : notnull where C2 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2}(Match,Match)"/>
    QueryBuilder<C1, C2> Query<C1, C2>(Match match1, Match match2) where C1 : notnull where C2 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3}()"/>
    QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : notnull where C2 : notnull where C3 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3}(Match,Match,Match)"/>
    QueryBuilder<C1, C2, C3> Query<C1, C2, C3>(Match match1, Match match2, Match match3) where C1 : notnull where C2 : notnull where C3 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4}()"/>
    QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4}(Match,Match,Match,Match)"/>
    QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>(Match match1, Match match2, Match match3, Match match4) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4,C5}()"/>
    QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull;

    /// <inheritdoc cref="fennecs.World.Query{C1,C2,C3,C4,C5}(Match,Match,Match,Match,Match)"/>
    QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>(Match match1, Match match2, Match match3, Match match4, Match match5) where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull where C5 : notnull;
}
