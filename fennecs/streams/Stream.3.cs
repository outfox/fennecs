﻿using System.Collections;

namespace fennecs;

/// <inheritdoc cref="Stream{C0}"/>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
/// <typeparam name="C2">stream type</typeparam>
/// // ReSharper disable once NotAccessedPositionalProperty.Global
public partial record Stream<C0, C1, C2> : Stream, IEnumerable<(Entity, C0, C1, C2)>
    where C0 : notnull
    where C1 : notnull
    where C2 : notnull
{
    /// <inheritdoc cref="Stream{C0}"/>
    /// // ReSharper disable once NotAccessedPositionalProperty.Global
    internal Stream(Query Query, Match match0, Match match1, Match match2) : base(Query)
    {
        StreamTypes = [MatchExpression.Of<C0>(match0), MatchExpression.Of<C1>(match1), MatchExpression.Of<C2>(match2)];
    }


    #region Blitters

    /// <inheritdoc cref="Stream{C0}.Blit(C0,Match)"/>
    public void Blit(C0 value, Match match = default) => Filtered.Fill(match, value);

    /// <inheritdoc cref="Stream{C0}.Blit(C0,Match)"/>
    public void Blit(C1 value, Match match = default) => Filtered.Fill(match, value);

    /// <inheritdoc cref="Stream{C0}.Blit(C0,Match)"/>
    public void Blit(C2 value, Match match = default) => Filtered.Fill(match, value);

    #endregion


    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<(Entity, C0, C1, C2)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2>(StreamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var (s0, s1, s2) = join.Select;
                for (var index = 0; index < table.Count; index++)
                {
                    yield return (table[index], s0[index], s1[index], s2[index]);
                    if (table.Version != snapshot)
                        throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}