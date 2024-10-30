// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

internal sealed class Mask : IDisposable
{
    internal readonly HashSet<TypeExpression> HasTypes = [];
    internal readonly HashSet<TypeExpression> NotTypes = [];
    internal readonly HashSet<TypeExpression> AnyTypes = [];

    public bool SafeForAddition(TypeExpression typeExpression) => typeExpression.Matches(NotTypes);
    public bool SafeForRemoval(TypeExpression typeExpression) => typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes);


    public Mask Has(TypeExpression typeExpression)
    {
        HasTypes.Add(typeExpression);
        return this;
    }


    public Mask Not(TypeExpression typeExpression)
    {
        NotTypes.Add(typeExpression);
        return this;
    }


    public Mask Any(TypeExpression typeExpression)
    {
        AnyTypes.Add(typeExpression);
        return this;
    }


    public void Clear()
    {
        HasTypes.Clear();
        NotTypes.Clear();
        AnyTypes.Clear();
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
    public void Dispose() => MaskPool.Return(this);


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
        return mask;
    }
}