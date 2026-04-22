namespace fennecs;

/// <summary>
/// Streamable Objects allow creating <see cref="Stream{C}">Stream</see> Views of the Query or World, for one or more Stream Types.
/// </summary>
/// <remarks>
/// Notable Streamables are <see cref="Query">Queries</see> and <see cref="World">Worlds</see>.
/// </remarks>
public interface Streamable
{
    /// <summary>
    /// Creates a Stream View of the Query with 1 Stream Type.
    /// </summary>
    /// <param name="match">Match Target for the Component (defaults to Any)</param>
    /// <typeparam name="C">Component stream type</typeparam>
    /// <returns>Stream View</returns>
    Stream<C> Stream<C>(Match match = default)
        where C : notnull;
    
    /// <summary>
    /// Creates a Stream View of the Query with multiple Stream Types. Individual Match Targets may be specified for each type.
    /// </summary>
    /// <param name="match0">1st Component Match Target</param>
    /// <param name="match1">2nd Component Match Target</param>
    /// <typeparam name="C0">1st Component stream type</typeparam>
    /// <typeparam name="C1">2nd Component stream type</typeparam>
    /// <returns>Stream View</returns>
    Stream<C0, C1> Stream<C0, C1>(Match match0, Match match1)
        where C0 : notnull
        where C1 : notnull;
    
    /// <summary>
    /// Creates a Stream View of the Query with multiple Stream Types
    /// </summary>
    /// <param name="matchAll">Match Target for ALL Components (defaults to Any)</param>
    /// <typeparam name="C0">1st Component stream type</typeparam>
    /// <typeparam name="C1">2nd Component stream type</typeparam>
    /// <returns>Stream View</returns>
    Stream<C0, C1> Stream<C0, C1>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull;

    /// <param name="match0">1st Component Match Target</param>
    /// <param name="match1">2nd Component Match Target</param>
    /// <param name="match2">3nd Component Match Target</param>
    /// <typeparam name="C0">1st Component stream type</typeparam>
    /// <typeparam name="C1">2nd Component stream type</typeparam>
    /// <typeparam name="C2">3rd Component stream type</typeparam>
    Stream<C0, C1, C2> Stream<C0, C1, C2>(Match match0, Match match1, Match match2)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull;
    /// <inheritdoc cref="Streamable.Stream{C0,C1}(fennecs.Match)"/>
    /// <typeparam name="C0">1st Component stream type</typeparam>
    /// <typeparam name="C1">2nd Component stream type</typeparam>
    /// <typeparam name="C2">3rd Component stream type</typeparam>
    Stream<C0, C1, C2> Stream<C0, C1, C2>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull;

    /// <param name="match0">1st Component Match Target</param>
    /// <param name="match1">2nd Component Match Target</param>
    /// <param name="match2">3nd Component Match Target</param>
    /// <param name="match3">4nd Component Match Target</param>
    /// <typeparam name="C0">1st Component stream type</typeparam>
    /// <typeparam name="C1">2nd Component stream type</typeparam>
    /// <typeparam name="C2">3rd Component stream type</typeparam>
    /// <typeparam name="C3">4th Component stream type</typeparam>
    Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match match0, Match match1, Match match2, Match match3)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull;

    /// <typeparam name="C0">1st component stream type</typeparam>
    /// <typeparam name="C1">2nd component stream type</typeparam>
    /// <typeparam name="C2">3rd component stream type</typeparam>
    /// <typeparam name="C3">4th component stream type</typeparam>
    Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull;

    /// <param name="match0">1st Component Match Target</param>
    /// <param name="match1">2nd Component Match Target</param>
    /// <param name="match2">3nd Component Match Target</param>
    /// <param name="match3">4nd Component Match Target</param>
    /// <param name="match4">5th Component Match Target</param>
    /// <typeparam name="C0">1st component stream type</typeparam>
    /// <typeparam name="C1">2nd component stream type</typeparam>
    /// <typeparam name="C2">3rd component stream type</typeparam>
    /// <typeparam name="C3">4th component stream type</typeparam>
    /// <typeparam name="C4">5th component stream type</typeparam>
    Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match match0, Match match1, Match match2, Match match3, Match match4)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull
        where C4 : notnull;

    /// <typeparam name="C0">1st component stream type</typeparam>
    /// <typeparam name="C1">2nd component stream type</typeparam>
    /// <typeparam name="C2">3rd component stream type</typeparam>
    /// <typeparam name="C3">4th component stream type</typeparam>
    /// <typeparam name="C4">5th component stream type</typeparam>
    Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull
        where C4 : notnull;
}
