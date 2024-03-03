// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public sealed class Mask : IDisposable
{
    internal readonly List<TypeExpression> HasTypes = new(8);
    internal readonly List<TypeExpression> NotTypes = new(8);
    internal readonly List<TypeExpression> AnyTypes = new(8);


    public void Has(TypeExpression typeExpression)
    {
        if (typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes))
        {
            throw new InvalidOperationException($"Type {typeExpression} is already (partially or fully) covered by this Query/Mask.");
        }

        if (typeExpression.Matches(NotTypes))
        {
            throw new InvalidOperationException($"Type {typeExpression} is already filtered out by this Query/Mask.");
        }

        HasTypes.Add(typeExpression);
    }


    public void Not(TypeExpression typeExpression)
    {
        if (typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes))
        {
            throw new InvalidOperationException($"Type {typeExpression} is already (partially or fully) covered by this Query/Mask.");
        }

        if (typeExpression.Matches(NotTypes))
        {
            throw new InvalidOperationException($"Type {typeExpression} is already filtered out by this Query/Mask.");
        }

        NotTypes.Add(typeExpression);
    }


    public void Any(TypeExpression typeExpression)
    {
        if (typeExpression.Matches(HasTypes) || typeExpression.Matches(AnyTypes))
        {
            throw new InvalidOperationException($"Type {typeExpression} is already (partially or fully) covered by this Query/Mask.");
        }

        if (typeExpression.Matches(NotTypes))
        {
            throw new InvalidOperationException($"Type {typeExpression} is already filtered out by this Query/Mask.");
        }

        AnyTypes.Add(typeExpression);
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


    public static implicit operator int(Mask self) => self.Key();

    public void Dispose() => MaskPool.Return(this);
}