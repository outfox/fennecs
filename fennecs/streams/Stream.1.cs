using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using fennecs.CRUD;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// A Stream is an accessor that allows for iteration over a Query's contents.
/// It exposes both the Runners as well as IEnumerable over a value tuple of the
/// Query's contents.
/// </summary>
/// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
// ReSharper disable once NotAccessedPositionalProperty.Global
public partial record Stream<C0> : 
    Stream,
    IEnumerable<(Entity, C0)>
    where C0 : notnull
{
    /// <summary>
    /// A Stream is an accessor that allows for iteration over a Query's contents.
    /// It exposes both the Runners as well as IEnumerable over a value tuple of the
    /// Query's contents.
    /// </summary>
    /// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
    internal Stream(Query Query, Match Match0) : base(Query)
    {
        this.Match0 = Match0;
        _streamTypes = [TypeExpression.Of<C0>(Match0)];
    }

    public Match Match0 { get; init; }

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
    /// <param name="match">default for Plain components, Entity for Relations, Identity.Of(Object) for ObjectLinks </param>
    public void Blit(C0 value, Match match = default)
    {
        var typeExpression = TypeExpression.Of<C0>(match);
        foreach (var table in Filtered) table.Fill(typeExpression, value);
    }

    #endregion

    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<(Entity, C0)> GetEnumerator()
    {
        foreach (var table in Filtered)
        {
            using var join = table.CrossJoin<C0>(_streamTypes.AsSpan());
            if (join.Empty) continue;
            var snapshot = table.Version;
            do
            {
                var s0 = join.Select;
                for (var index = 0; index < table.Count; index++)
                {
                    yield return (table[index], s0.Span[index]);
                    if (table.Version != snapshot) throw new InvalidOperationException("Collection was modified during iteration.");
                }
            } while (join.Iterate());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public void Deconstruct(out Query Query, out Match Match0)
    {
        Query = this.Query;
        Match0 = this.Match0;
    }
}
