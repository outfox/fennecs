using System.Collections;
using System.Collections.Immutable;
using fennecs.pools;

namespace fennecs;

/// <inheritdoc cref="Stream{C0}"/>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
/// <typeparam name="C2">stream type</typeparam>
/// <typeparam name="C3">stream type</typeparam>
/// <typeparam name="C4">stream type</typeparam>
// ReSharper disable once NotAccessedPositionalProperty.Global
public partial record Stream<C0, C1, C2, C3, C4>(Query Query, Match Match0, Match Match1, Match Match2, Match Match3, Match Match4)
    : Stream<C0, C1, C2, C3>(Query, Match0, Match1, Match2, Match3), IEnumerable<(Entity, C0, C1, C2, C3, C4)>
    where C0 : notnull
    where C1 : notnull
    where C2 : notnull
    where C3 : notnull
    where C4 : notnull
{
    private readonly ImmutableArray<TypeExpression> _streamTypes =
        [TypeExpression.Of<C0>(Match0), TypeExpression.Of<C1>(Match1), TypeExpression.Of<C2>(Match2), TypeExpression.Of<C3>(Match3), TypeExpression.Of<C4>(Match4)];


    #region Blitters

    /// <inheritdoc cref="Stream{C0}.Blit(C0,Match)"/>
    public void Blit(C4 value, Match match = default)
    {
        using var worldLock = World.Lock();

        var typeExpression = TypeExpression.Of<C4>(match);

        foreach (var table in Filtered)
        {
            table.Fill(typeExpression, value);
        }
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc />
    public new IEnumerator<(Entity, C0, C1, C2, C3, C4)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0, C1, C2, C3, C4>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var (s0, s1, s2, s3, s4) = join.Select;
                for (var index = 0; index < table.Count; index++)
                {
                    yield return (table[index], s0[index], s1[index], s2[index], s3[index], s4[index]);
                    if (table.Version != snapshot) throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    #endregion

    private static void Unroll8(Span<C0> span0, Span<C1> span1, Span<C2> span2, Span<C3> span3, Span<C4> span4, ComponentAction<C0, C1, C2, C3, C4> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], ref span4[i]);
            action(ref span0[i + 1], ref span1[i + 1], ref span2[i + 1], ref span3[i + 1], ref span4[i + 1]);
            action(ref span0[i + 2], ref span1[i + 2], ref span2[i + 2], ref span3[i + 2], ref span4[i + 2]);
            action(ref span0[i + 3], ref span1[i + 3], ref span2[i + 3], ref span3[i + 3], ref span4[i + 3]);

            action(ref span0[i + 4], ref span1[i + 4], ref span2[i + 4], ref span3[i + 4], ref span4[i + 4]);
            action(ref span0[i + 5], ref span1[i + 5], ref span2[i + 5], ref span3[i + 5], ref span4[i + 5]);
            action(ref span0[i + 6], ref span1[i + 6], ref span2[i + 6], ref span3[i + 6], ref span4[i + 6]);
            action(ref span0[i + 7], ref span1[i + 7], ref span2[i + 7], ref span3[i + 7], ref span4[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(ref span0[i], ref span1[i], ref span2[i], ref span3[i], ref span4[i]);
        }
    }

    private static void Unroll8U<U>(U uniform, Span<C0> span0, Span<C1> span1, Span<C2> span2, Span<C3> span3, Span<C4> span4, UniformComponentAction<U, C0, C1, C2, C3, C4> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(uniform, ref span0[i], ref span1[i], ref span2[i], ref span3[i], ref span4[i]);
            action(uniform, ref span0[i + 1], ref span1[i + 1], ref span2[i + 1], ref span3[i + 1], ref span4[i + 1]);
            action(uniform, ref span0[i + 2], ref span1[i + 2], ref span2[i + 2], ref span3[i + 2], ref span4[i + 2]);
            action(uniform, ref span0[i + 3], ref span1[i + 3], ref span2[i + 3], ref span3[i + 3], ref span4[i + 3]);

            action(uniform, ref span0[i + 4], ref span1[i + 4], ref span2[i + 4], ref span3[i + 4], ref span4[i + 4]);
            action(uniform, ref span0[i + 5], ref span1[i + 5], ref span2[i + 5], ref span3[i + 5], ref span4[i + 5]);
            action(uniform, ref span0[i + 6], ref span1[i + 6], ref span2[i + 6], ref span3[i + 6], ref span4[i + 6]);
            action(uniform, ref span0[i + 7], ref span1[i + 7], ref span2[i + 7], ref span3[i + 7], ref span4[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(uniform, ref span0[i], ref span1[i], ref span2[i], ref span3[i], ref span4[i]);
        }
    }
}
