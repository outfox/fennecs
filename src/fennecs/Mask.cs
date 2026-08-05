// SPDX-License-Identifier: MIT

using System.Diagnostics;
using fennecs.pools;

namespace fennecs;

internal sealed class Mask : IDisposable
{
    internal readonly SortedSet<TypeExpression> HasTypes = [];
    internal readonly SortedSet<TypeExpression> NotTypes = [];
    internal readonly SortedSet<TypeExpression> AnyTypes = [];

    // Compiled lazily; invalidated on mutation (Masks are pooled and mutable).
    private MaskBits? _bits;

    internal MaskBits Bits => _bits ??= MaskBits.Of(this);


    public bool SafeForAddition(TypeExpression typeExpression) => Bits.Not.MatchesBidirectional(typeExpression);

    public bool SafeForRemoval(TypeExpression typeExpression) =>
        Bits.Has.MatchesBidirectional(typeExpression) || Bits.Any.MatchesBidirectional(typeExpression);


    public Mask Has(TypeExpression typeExpression)
    {
        HasTypes.Add(typeExpression);
        _bits = null;
        return this;
    }


    public Mask Not(TypeExpression typeExpression)
    {
        NotTypes.Add(typeExpression);
        _bits = null;
        return this;
    }


    public Mask Any(TypeExpression typeExpression)
    {
        AnyTypes.Add(typeExpression);
        _bits = null;
        return this;
    }


    public void Clear()
    {
        HasTypes.Clear();
        NotTypes.Clear();
        AnyTypes.Clear();
        _bits = null;
    }


    private int Key()
    {
        var hash = HashCode.Combine(HasTypes.Count);
        hash = HasTypes.Aggregate(hash, HashCode.Combine);
        hash = HashCode.Combine(hash, NotTypes.Count);
        hash = NotTypes.Aggregate(hash, HashCode.Combine);
        hash = HashCode.Combine(hash, AnyTypes.Count);
        hash = AnyTypes.Aggregate(hash, HashCode.Combine);

        return hash;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Key();

    /// <inheritdoc />
    public void Dispose()
    {
        MaskPool.Return(this);
    }


    /// <summary>
    /// Clones the mask and returns a new instance.
    /// </summary>
    /// <remarks>
    /// The new instance should be Disposed after use to re-cycle its resources to the internal pool.
    /// </remarks> 
    public Mask Clone()
    {
        var mask = MaskPool.Rent();
        mask.HasTypes.UnionWith(HasTypes);
        mask.NotTypes.UnionWith(NotTypes);
        mask.AnyTypes.UnionWith(AnyTypes);
        mask._bits = null;
        return mask;
    }
}
