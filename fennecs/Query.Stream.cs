namespace fennecs;

/// <summary>
/// Stream Factory Methods
/// </summary>
public partial class Query
{
    /// <summary>
    /// Creates a Stream View of the Query with 1 Stream Type.
    /// </summary>
    /// <param name="match">Match Target for the Component (defaults to Any)</param>
    /// <typeparam name="C">component stream type</typeparam>
    /// <returns>Stream View</returns>
    public Stream<C> Stream<C>(Match match = default)
        where C : notnull
        => new(this, match);

    /// <summary>
    /// Creates a Stream View of the Query with multiple Stream Types. Individual Match Targets may be specified for each type.
    /// </summary>
    /// <param name="match0">1st Component Match Target</param>
    /// <param name="match1">2nd Component Match Target</param>
    /// <typeparam name="C0">1st component stream type</typeparam>
    /// <typeparam name="C1">2nd component stream type</typeparam>
    /// <returns>Stream View</returns>
    public Stream<C0, C1> Stream<C0, C1>(Match match0, Match match1)
        where C0 : notnull
        where C1 : notnull
        => new(this, match0, match1);


    /// <summary>
    /// Creates a Stream View of the Query with multiple Stream Types
    /// </summary>
    /// <param name="matchAll">Match Target for ALL Components (defaults to Any)</param>
    /// <typeparam name="C0">1st component stream type</typeparam>
    /// <typeparam name="C1">2nd component stream type</typeparam>
    /// <returns>Stream View</returns>
    public Stream<C0, C1> Stream<C0, C1>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        => new(this, matchAll, matchAll);


    //These Pragmas help with inheritance for XMLdoc 
#pragma warning disable CS1712 // Type parameter has no matching typeparam tag in the XML comment (but other type parameters do)
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

    /// <inheritdoc cref="Stream{C0,C1}(Match, Match)"/>
    /// <param name="match2">3nd Component Match Target</param>
    /// <typeparam name="C2">3rd component stream type</typeparam>
    public Stream<C0, C1, C2> Stream<C0, C1, C2>(Match match0, Match match1, Match match2)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        => new(this, match0, match1, match2);

    /// <inheritdoc cref="Stream{C0,C1}(Match)"/>
    /// <typeparam name="C2">3rd component stream type</typeparam>
    public Stream<C0, C1, C2> Stream<C0, C1, C2>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        => new(this, matchAll, matchAll, matchAll);


    /// <inheritdoc cref="Stream{C0,C1,C2}(Match,Match,Match)"/>
    /// <param name="match3">4nd Component Match Target</param>
    /// <typeparam name="C3">4th component stream type</typeparam>
    public Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match match0, Match match1, Match match2, Match match3)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull
        => new(this, match0, match1, match2, match3);

    /// <inheritdoc cref="Stream{C0,C1,C2}(Match)"/>
    /// <typeparam name="C3">4th component stream type</typeparam>
    public Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull
        => new(this, matchAll, matchAll, matchAll, matchAll);

    /// <inheritdoc cref="Stream{C0,C1,C2, C3}(Match,Match,Match,Match)"/>
    /// <param name="match4">5th Component Match Target</param>
    /// <typeparam name="C4">5th component stream type</typeparam>
    public Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match match0, Match match1, Match match2, Match match3, Match match4)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull
        where C4 : notnull
        => new(this, match0, match1, match2, match3, match4);

    
    /// <inheritdoc cref="Stream{C0,C1,C2, C3}(Match)"/>
    /// <typeparam name="C4">5th component stream type</typeparam>
    public Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match matchAll = default)
        where C0 : notnull
        where C1 : notnull
        where C2 : notnull
        where C3 : notnull
        where C4 : notnull
        => new(this, matchAll, matchAll, matchAll, matchAll, matchAll);

#pragma warning restore CS1712 // Type parameter has no matching typeparam tag in the XML comment (but other type parameters do)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
}
