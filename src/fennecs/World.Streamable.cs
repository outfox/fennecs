namespace fennecs;

public partial class World : Streamable
{
    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C> Stream<C>(Match match = default) where C : notnull => Query<C>(match).Stream();

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1> Stream<C0, C1>(Match match0, Match match1) where C0 : notnull where C1 : notnull
        => Query<C0, C1>(match0, match1).Stream();

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1> Stream<C0, C1>(Match matchAll = default) where C0 : notnull where C1 : notnull
        => Stream<C0, C1>(matchAll, matchAll);

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1,C2}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1, C2> Stream<C0, C1, C2>(Match match0, Match match1, Match match2) where C0 : notnull where C1 : notnull where C2 : notnull
        => Query<C0, C1, C2>(match0, match1, match2).Stream();

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1,C2}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1, C2> Stream<C0, C1, C2>(Match matchAll = default) where C0 : notnull where C1 : notnull where C2 : notnull
        => Stream<C0, C1, C2>(matchAll, matchAll, matchAll);

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1,C2,C3}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match match0, Match match1, Match match2, Match match3) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull
        => Query<C0, C1, C2, C3>(match0, match1, match2, match3).Stream();

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1,C2,C3}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1, C2, C3> Stream<C0, C1, C2, C3>(Match matchAll = default) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull
        => Stream<C0, C1, C2, C3>(matchAll, matchAll, matchAll, matchAll);

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1,C2,C3,C4}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match match0, Match match1, Match match2, Match match3, Match match4)
        where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull
        => Query<C0, C1, C2, C3, C4>(match0, match1, match2, match3, match4).Stream();

    /// <summary>Compile an internal Query specifically for the requested Stream Types, and return a <see cref="fennecs.Stream{C0,C1,C2,C3,C4}">Stream</see> View for it.</summary>
    /// <inheritdoc />
    public Stream<C0, C1, C2, C3, C4> Stream<C0, C1, C2, C3, C4>(Match matchAll = default) where C0 : notnull where C1 : notnull where C2 : notnull where C3 : notnull where C4 : notnull
        => Stream<C0, C1, C2, C3, C4>(matchAll, matchAll, matchAll, matchAll, matchAll);
}
