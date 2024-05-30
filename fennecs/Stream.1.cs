using System.Collections;
using System.Collections.Immutable;

namespace fennecs;


/// <summary>
/// A Stream is an accessor that allows for iteration over a Query's contents.
/// It exposes both the Runners as well as IEnumerable over a value tuple of the
/// Query's contents.
/// </summary>
/// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
public record Stream<C0>(Query Query, Identity Match1) : IEnumerable<(Entity, C0)> where C0 : notnull
{
    private readonly ImmutableArray<TypeExpression> _streamTypes = [TypeExpression.Of<C0>(Match1)];

    /// <summary>
    /// The Archetypes that this Stream is iterating over.
    /// </summary>
    protected IReadOnlyList<Archetype> Archetypes => Query.Archetypes;
    
    /// <summary>
    /// The World this Stream is associated with.
    /// </summary>
    protected World World => Query.World;
    
    /// <summary>
    /// The Query this Stream is associated with.
    /// Can be re-inited via the with keyword.
    /// </summary>
    public Query Query { get; init; } = Query;

    /// <summary>
    /// The Match Target for the first Stream Type
    /// </summary>
    public Identity Match1 { get; init; } = Match1;

    /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
    public void For(RefAction<C0> action)
    {
        using var worldLock = World.Lock();
        foreach (var table in Archetypes)
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

    #region Blitters

    /// <summary>
    /// <para>Blit (write) a component value of a stream type to all entities matched by this query.</para>
    /// <para>🚀 Very fast!</para>
    /// </summary>
    /// <remarks>
    /// Each entity in the Query must possess the component type.
    /// Otherwise, consider using <see cref="Query.Add{T}()"/> with <see cref="Batch.AddConflict.Replace"/>. 
    /// </remarks>
    /// <param name="value">a component value</param>
    /// <param name="target">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks </param>
    public void Blit(C0 value, Identity target = default)
    {
        var typeExpression = TypeExpression.Of<C0>(target);

        foreach (var table in Archetypes)
        {
            table.Fill(typeExpression, value);
        }
    }
    
    #endregion


    #region IEnumerable

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
    
    #endregion
}
