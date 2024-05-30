using System.Collections;

namespace fennecs;


/// <summary>
/// A Stream is an accessor that allows for iteration over a Query's contents.
/// </summary>
/// <typeparam name="C0">component type to stream. if this type is not in the query, the stream will always be length zero.</typeparam>
public readonly record struct Stream<C0>(Query Query, Identity match) : IEnumerable<(Entity, C0)>
{
    /// <inheritdoc />
    public IEnumerator<(Entity, C0)> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
