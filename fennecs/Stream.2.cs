using System.Collections;
using System.Collections.Immutable;

namespace fennecs;

/// <summary>
/// A Stream is an accessor that allows for iteration over a Query's contents.
/// It exposes both the Runners as well as IEnumerable over a value tuple of the
/// Query's contents.
/// </summary>
/// <typeparam name="C0">stream type</typeparam>
/// <typeparam name="C1">stream type</typeparam>
public record Stream<C0, C1> : Stream<C0>, IEnumerable<(Entity, C0, C1)> where C0 : notnull where C1 : notnull
{
    /// <summary>
    /// A Stream is an accessor that allows for iteration over a Query's contents.
    /// </summary>
    public Stream(Query Query, Identity match1, Identity match2) : base(Query, match1)
    {
        _streamTypes = [TypeExpression.Of<C0>(match1), TypeExpression.Of<C1>(match2)];
    } 
    
    private readonly ImmutableArray<TypeExpression> _streamTypes;

    
    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(RefAction<C0, C1> action)
    {
        using var worldLock = World.Lock();

        foreach (var table in Archetypes)
        {
            using var join = table.CrossJoin<C0, C1>(_streamTypes);
            if (join.Empty) continue;

            do
            {
                var (s0, s1) = join.Select;
                var span0 = s0.Span;
                var span1 = s1.Span;

                Unroll8(span0, span1, action);
            } while (join.Iterate());
        }
    }

    
    #region IEnumerable
    
    /// <inheritdoc />
    public new IEnumerator<(Entity, C0, C1)> GetEnumerator()
    {
        using var worldLock = World.Lock();
        foreach (var table in Query.Archetypes)
        {

            using var join = table.CrossJoin<C0, C1>(_streamTypes);
            if (join.Empty) continue;

            var identities = table.IdentityStorage;

            do
            {
                var (s0, s1) = join.Select;
                for (var index = 0; index < s0.Count; index++)
                {
                    var identity = identities[index];
                    yield return (new(World, identity), s0.Span[index], s1.Span[index]);
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    #endregion
    
    
    private static void Unroll8(Span<C0> span0, Span<C1> span1, RefAction<C0, C1> action)
    {
        var c = span0.Length / 8 * 8;
        for (var i = 0; i < c; i += 8)
        {
            action(ref span0[i], ref span1[i]);
            action(ref span0[i + 1], ref span1[i + 1]);
            action(ref span0[i + 2], ref span1[i + 2]);
            action(ref span0[i + 3], ref span1[i + 3]);

            action(ref span0[i + 4], ref span1[i + 4]);
            action(ref span0[i + 5], ref span1[i + 5]);
            action(ref span0[i + 6], ref span1[i + 6]);
            action(ref span0[i + 7], ref span1[i + 7]);
        }

        var d = span0.Length;
        for (var i = c; i < d; i++)
        {
            action(ref span0[i], ref span1[i]);
        }
    }
}
