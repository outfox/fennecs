// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs;

public sealed class Mask : IDisposable
{
    internal readonly List<TypeExpression> HasTypes = new(8);
    internal readonly List<TypeExpression> NotTypes = new(8);
    internal readonly List<TypeExpression> AnyTypes = new(8);
    
    public void Has(TypeExpression type)
    {
        HasTypes.Add(type);
    }
    
    public void Not(TypeExpression type)
    {
        NotTypes.Add(type);
    }
    
    public void Any(TypeExpression type)
    {
        AnyTypes.Add(type);
    }

    public void Clear()
    {
        HasTypes.Clear();
        NotTypes.Clear();
        AnyTypes.Clear();
    }

    public int Key()
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

    /*
    public bool Equals(Mask? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Mask other && Equals(other);
    }
    */
    
    public void Dispose() => MaskPool.Return(this);
}
