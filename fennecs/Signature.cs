// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.Immutable;

namespace fennecs;

/// <summary>
/// Generic IImmutableSortedSet whose hash code is a combination of its elements' hashes.
/// </summary>
public readonly struct Signature<T> : IEquatable<Signature<T>>, IEnumerable<T>, IComparable<Signature<T>>
    where T : IComparable<T>
{
    private readonly ImmutableSortedSet<T> _set = ImmutableSortedSet<T>.Empty;
    private readonly int _hashCode;

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;


     
    /// <summary>
    /// Creates a new <see cref="Signature{T}"/> from the given values.
    /// </summary>
    /// <param name="values">constituent values of the signature; will be converted to an <see cref="ImmutableSortedSet{T}"/></param>
    public Signature(params T[] values) : this(values.ToImmutableSortedSet())
    {
    }


    /// <summary>
    /// Creates a new <see cref="Signature{T}"/> from the given values.
    /// </summary>
    /// <param name="set">a set of constituent values of the signature</param>
    /// <inheritdoc cref="ImmutableSortedSet{T}"/>
    public Signature(ImmutableSortedSet<T> set)
    {
        _set = set;
        Count = set.Count;
        _hashCode = BakeHash(_set);
    }

    
    /// <inheritdoc cref="ImmutableSortedSet{T}.Add"/>
    public Signature<T> Add(T value) => new(_set.Add(value));


    /// <inheritdoc cref="ImmutableSortedSet{T}.Clear"/>
    public Signature<T> Clear() => new(ImmutableSortedSet<T>.Empty);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Contains"/>
    public bool Contains(T item) => _set.Contains(item);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Except"/>
    public Signature<T> Except(IEnumerable<T> other) => new(_set.Except(other));


    /// <inheritdoc cref="ImmutableSortedSet{T}.Intersect"/>
    public Signature<T> Intersect(IEnumerable<T> other) => new(_set.Intersect(other));


    
    /// <inheritdoc cref="ImmutableSortedSet{T}.IsProperSubsetOf"/>
    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.IsProperSupersetOf"/>
    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.IsSubsetOf"/>
    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.IsSupersetOf"/>
    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    
    /// <inheritdoc cref="ImmutableSortedSet{T}.Overlaps"/>
    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Remove"/>
    public Signature<T> Remove(T value) => new(_set.Remove(value));


    /// <inheritdoc cref="ImmutableSortedSet{T}.SetEquals"/>
    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.SymmetricExcept"/>
    public Signature<T> SymmetricExcept(IEnumerable<T> other) => new(_set.SymmetricExcept(other));


    /// <inheritdoc cref="ImmutableSortedSet{T}.TryGetValue"/>
    public bool TryGetValue(T equalValue, out T actualValue) => _set.TryGetValue(equalValue, out actualValue);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Union"/>
    public Signature<T> Union(IEnumerable<T> other) => new(_set.Union(other));
    
    /// <inheritdoc cref="ImmutableSortedSet{T}.SetEquals"/>
    public bool Equals(Signature<T> other) => _set.SetEquals(other._set);


    /// <inheritdoc />
    // ReSharper disable once NotDisposedResourceIsReturned
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();


    /// <inheritdoc />
    public int CompareTo(Signature<T> other)
    {
        var minCount = Math.Min(_set.Count, other._set.Count);

        for (var i = 0; i < minCount; i++)
        {
            var cmp = _set.ElementAt(i).CompareTo(other._set.ElementAt(i));
            if (cmp != 0) return cmp;
        }
        
        return _set.Count.CompareTo(other._set.Count);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Signature<T> other && Equals(other);


    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private static int BakeHash(IEnumerable<T> source)
    {
        var code = new HashCode();
        foreach (var item in source) code.Add(item);
        return code.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => $"~{string.Join(",", _set)}~";

    /// <inheritdoc cref="SetEquals" />
    public static bool operator ==(Signature<T> left, Signature<T> right) => left.Equals(right);

    /// <inheritdoc cref="SetEquals" />
    public static bool operator !=(Signature<T> left, Signature<T> right) => !left.Equals(right);


    /// <summary>
    /// Number of constituent values in this signature.
    /// </summary>
    public int Count { get; }

    /// <inheritdoc cref="Enumerable.ElementAt{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Index)"/>
    public T this[int index] => _set.ElementAt(index);
}