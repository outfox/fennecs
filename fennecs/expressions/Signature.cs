// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.Immutable;

namespace fennecs;

/// <summary>
/// <c>ImmutableSortedSet&lt;TypeExpression&gt;</c> whose hash code is a combination of its elements' hashes.
/// TODO:Convert to bloom filter + faster (frozen?) set (needs benchmark).
/// </summary>
public sealed record Signature : IEnumerable<TypeExpression>, IComparable<Signature>
{
    //private readonly Vector256<ulong> bloomQuick;
    //private readonly Vector256<ulong> bloomDetail;
    
    private readonly ImmutableSortedSet<TypeExpression> _set = ImmutableSortedSet<TypeExpression>.Empty;
    
    
    private readonly int _hashCode;

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;
    
    
    /// <summary>
    /// Check if this <see cref="Signature"/> matches the given <see cref="TypeExpression"/>.
    /// </summary>
    /// <remarks>
    /// Used for comparing archetypes among themselves.
    /// </remarks>
    public bool Matches(TypeExpression expression) => _set.Contains(expression);

    
    /// <summary>
    /// Check if this <see cref="Signature"/> matches the given <see cref="MatchExpression"/>.
    /// </summary>
    public bool Matches(MatchExpression expression) => expression.Matches(_set);


    /// <summary>
    /// Check if this <see cref="Signature"/> matches all of the given <see cref="MatchExpression"/>s.
    /// TODO: Optimize o(n²) using bloom filter.
    /// </summary>
    public bool MatchesAll(IReadOnlySet<MatchExpression> expressions) => expressions.All(Matches);
    
    /// <summary>
    /// Check if this <see cref="Signature"/> matches any of the given <see cref="MatchExpression"/>s.
    /// TODO: Optimize o(n²) using bloom filter.
    /// </summary>
    public bool MatchesAny(IReadOnlySet<MatchExpression> expressions) => expressions.Any(Matches);
    
    /// <summary>
    /// Check if this <see cref="Signature"/> matches none of the given <see cref="MatchExpression"/>s.
    /// TODO: Optimize using bloom filter.
    /// </summary>
    public bool MatchesNone(IReadOnlySet<MatchExpression> expressions) => !expressions.All(Matches);
    
    internal bool Matches(IReadOnlySet<Comp> subset) => Overlaps(subset.Select(component => component.Expression).ToImmutableSortedSet());
    
    
    /// <summary>
    /// Creates a new <see cref="Signature"/> from the given values.
    /// </summary>
    /// <param name="values">constituent values of the signature; will be converted to an <see cref="ImmutableSortedSet{T}"/></param>
    public Signature(params TypeExpression[] values) : this(values.ToImmutableSortedSet())
    {
    }


    /// <summary>
    /// Creates a new <see cref="Signature"/> from the given values.
    /// </summary>
    /// <param name="set">a set of constituent values of the signature</param>
    /// <inheritdoc cref="ImmutableSortedSet{T}"/>
    public Signature(ImmutableSortedSet<TypeExpression> set)
    {
        _set = set;
        Count = set.Count;
        _hashCode = BakeHash(_set);
    }


    /// <inheritdoc cref="ImmutableSortedSet{T}.Add"/>
    public Signature Add(TypeExpression value) => new(_set.Add(value));


    /// <inheritdoc cref="ImmutableSortedSet{T}.Clear"/>
    public Signature Clear() => new(ImmutableSortedSet<TypeExpression>.Empty);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Contains"/>
    public bool Contains(TypeExpression item) => _set.Contains(item);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Except"/>
    public Signature Except(IEnumerable<TypeExpression> other) => new(_set.Except(other));


    /// <inheritdoc cref="ImmutableSortedSet{T}.Intersect"/>
    public Signature Intersect(IEnumerable<TypeExpression> other) => new(_set.Intersect(other));


    
    /// <inheritdoc cref="ImmutableSortedSet{T}.IsProperSubsetOf"/>
    public bool IsProperSubsetOf(IEnumerable<TypeExpression> other) => _set.IsProperSubsetOf(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.IsProperSupersetOf"/>
    public bool IsProperSupersetOf(IEnumerable<TypeExpression> other) => _set.IsProperSupersetOf(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.IsSubsetOf"/>
    public bool IsSubsetOf(IEnumerable<TypeExpression> other) => _set.IsSubsetOf(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.IsSupersetOf"/>
    public bool IsSupersetOf(IEnumerable<TypeExpression> other) => _set.IsSupersetOf(other);

    
    /// <inheritdoc cref="ImmutableSortedSet{T}.Overlaps"/>
    public bool Overlaps(IEnumerable<TypeExpression> other) => _set.Overlaps(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Overlaps"/>
    public bool Overlaps(ImmutableSortedSet<TypeExpression> other) => _set.Overlaps(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Remove"/>
    public Signature Remove(TypeExpression value) => new(_set.Remove(value));


    /// <inheritdoc cref="ImmutableSortedSet{T}.SetEquals"/>
    public bool SetEquals(IEnumerable<TypeExpression> other) => _set.SetEquals(other);


    /// <inheritdoc cref="ImmutableSortedSet{T}.SymmetricExcept"/>
    public Signature SymmetricExcept(IEnumerable<TypeExpression> other) => new(_set.SymmetricExcept(other));


    /// <inheritdoc cref="ImmutableSortedSet{T}.TryGetValue"/>
    public bool TryGetValue(TypeExpression equalValue, out TypeExpression actualValue) => _set.TryGetValue(equalValue, out actualValue);


    /// <inheritdoc cref="ImmutableSortedSet{T}.Union"/>
    public Signature Union(IEnumerable<TypeExpression> other) => new(_set.Union(other));
    
    /// <inheritdoc cref="ImmutableSortedSet{T}.SetEquals"/>
    public bool Equals(Signature? other) => other != null && _set.SetEquals(other._set);


    /// <inheritdoc />
    // ReSharper disable once NotDisposedResourceIsReturned
    public IEnumerator<TypeExpression> GetEnumerator() => _set.GetEnumerator();


    /// <inheritdoc />
    public int CompareTo(Signature? other)
    {
        if (other == null) return 1;
        
        var minCount = Math.Min(_set.Count, other._set.Count);

        for (var i = 0; i < minCount; i++)
        {
            var cmp = _set.ElementAt(i).CompareTo(other._set.ElementAt(i));
            if (cmp != 0) return cmp;
        }
        
        return _set.Count.CompareTo(other._set.Count);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private static int BakeHash(IEnumerable<TypeExpression> source)
    {
        var code = new HashCode();
        foreach (var item in source) code.Add(item);
        return code.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => $"~{string.Join(",", _set)}~";

    /*
    /// <inheritdoc cref="SetEquals" />
    public static bool operator ==(Signature left, Signature right) => left.Equals(right);

    /// <inheritdoc cref="SetEquals" />
    public static bool operator !=(Signature left, Signature right) => !left.Equals(right);
    */

    /// <summary>
    /// Number of constituent values in this signature.
    /// </summary>
    public int Count { get; }

    /// <inheritdoc cref="Enumerable.ElementAt{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Index)"/>
    public TypeExpression this[int index] => _set.ElementAt(index);
}