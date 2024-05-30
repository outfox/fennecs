using System.Collections;

namespace fennecs;


/// <summary>
/// A Stream is an accessor that allows for iteration over a Query's contents.
/// </summary>
/// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
public readonly record struct Stream<C0> : IEnumerable<(Entity, C0)>
{
    /// <summary>
    /// A Stream is an accessor that allows for iteration over a Query's contents.
    /// </summary>
    public Stream(Query Query, Identity match) 
    {
        this.Query = Query;
        _streamTypes = [ TypeExpression.Of<C0>(match)];
    }
    
    private readonly TypeExpression[] _streamTypes;
    
    private World World => Query.World;
    /// <summary>
    /// 
    /// </summary>
    private Query Query { get; init; }
    
    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(RefAction<C0> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Query.Archetypes)
        {

            using var join = table.CrossJoin<C0>(_streamTypes);
            if (join.Empty) continue;

            do
            {
                var s0 = join.Select;
                var span0 = s0.Span;
                // foreach is faster than for loop & unroll
                foreach (ref var c0 in span0) action(ref c0);
            } while (join.Iterate());
        }
    }


    /// <inheritdoc />
    public IEnumerator<(Entity, C0)> GetEnumerator()
    {
        using var worldLock = World.Lock();
        foreach (var table in Query.Archetypes)
        {

            using var join = table.CrossJoin<C0>(_streamTypes);
            if (join.Empty) continue;

            var identities = table.IdentityStorage;

            do
            {
                var s0 = join.Select;
                for (var index = 0; index < s0.Count; index++)
                {
                    var identity = identities[index];
                    yield return (new(World, identity), s0.Span[index]);
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
