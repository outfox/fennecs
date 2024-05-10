// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

internal sealed class Mask : IDisposable
{
    internal readonly List<TypeExpression> HasTypes = new(8);
    internal readonly List<TypeExpression> NotTypes = new(8);
    internal readonly List<TypeExpression> AnyTypes = new(8);

    internal bool safety = true;
    
    
    public bool SafeForAddition(TypeExpression typeExpression) => NotTypes.Contains(typeExpression);
    public bool SafeForRemoval(TypeExpression typeExpression) => typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes);


    public void Has(TypeExpression typeExpression)
    {
        switch (safety)
        {
            case true when typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes):
                throw new InvalidOperationException($"Overlapping Has<C>: Type {typeExpression} is already (partially or fully) covered by this Query/Mask.");
            case true when typeExpression.Matches(NotTypes):
                throw new InvalidOperationException($"Conflicting Has<C>: Type {typeExpression} is already filtered out by this Query/Mask (result is always empty).");
            default:
                HasTypes.Add(typeExpression);
                break;
        }
    }


    public void Not(TypeExpression typeExpression)
    {
        switch (safety)
        {
            case true when typeExpression.Matches(NotTypes):
                throw new InvalidOperationException($"Duplicate Not<C>: Type {typeExpression} is already filtered out by this Query/Mask.");
            case true when typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes):
                throw new InvalidOperationException($"Conflicting Not<C>: Type {typeExpression} is already (partially or fully) included by this Query/Mask (result or sub-result is always empty).");
            default:
                NotTypes.Add(typeExpression);
                break;
        }
    }


    public void Any(TypeExpression typeExpression)
    {
        switch (safety)
        {
            case true when typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes):
                throw new InvalidOperationException($"Overlapping Any<C>: Type {typeExpression} is already (partially or fully) covered by this Query/Mask.");
            case true when typeExpression.Matches(NotTypes):
                throw new InvalidOperationException($"Conflicting Any<C>: Type {typeExpression} is already filtered out by this Query/Mask.");
            default:
                AnyTypes.Add(typeExpression);
                break;
        }
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

        foreach (var type in HasTypes)
        {
            hash = HashCode.Combine(hash, type);
        }

        HashCode.Combine(NotTypes.Count);

        foreach (var type in NotTypes)
        {
            hash = HashCode.Combine(hash, type);
        }

        HashCode.Combine(AnyTypes.Count);

        foreach (var type in AnyTypes)
        {
            hash = HashCode.Combine(hash, type);
        }

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
        mask.HasTypes.AddRange(HasTypes);
        mask.NotTypes.AddRange(NotTypes);
        mask.AnyTypes.AddRange(AnyTypes);
        return mask;
    }
}