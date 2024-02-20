// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Refers to an entity, object, or virtual concept (e.g. any/none wildcard).
/// </summary>
/// <param name="value"></param>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Identity(ulong value) : IEquatable<Identity>, IComparable<Identity>
{
    [FieldOffset(0)] public readonly ulong Value = value;
    [FieldOffset(0)] public readonly int Id;
    
    [FieldOffset(4)] public readonly ushort Generation;
    [FieldOffset(4)] public readonly TypeID Decoration;

    [FieldOffset(6)] public readonly TypeID RESERVED = 0;

    [FieldOffset(0)] public readonly uint DWordLow;
    [FieldOffset(4)] public readonly uint DWordHigh;
    
    //public ulong Value => (uint) Id | (ulong) Generation << 32;

    public static readonly Identity None = new(0, 0);
    public static readonly Identity Any = new(0, TypeID.MaxValue);
    
    // Entity Reference.
    public bool IsEntity => Id > 0 && Generation > 0;

    // Tracked Object Reference.
    public bool IsObject => Decoration < 0;

    // Special Entities, such as None, Any, and others.
    public bool IsVirtual => Decoration >= 0 && Id <= 0;

    public static implicit operator Identity(Type type) => new(type);

    public Identity(int id, TypeID decoration = 1) : this((uint) id | (ulong) decoration << 32)
    {
    }
    
    internal Identity(TypeID typeId) : this(0, typeId)
    {
    }

    public Identity(Type type) : this(LanguageType.Identify(type))
    {
    }

    public static Identity Of<T>(T item) where T : class
    {
        return new(item.GetHashCode(), LanguageType<T>.TargetId);
    }
    
    public bool Equals(Identity other) => Id == other.Id && Generation == other.Generation;

    public int CompareTo(Identity other)
    {
        return Value.CompareTo(other.Value);
    }

    public override bool Equals(object? obj)
    {
        throw new InvalidCastException("Identity: Boxing equality comparisons disallowed. Use IEquatable<Identity>.Equals(Identity other) instead.");
        //return obj is Identity other && Equals(other); <-- second best option   
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }

    public static implicit operator Entity(Identity id) => new(id);
    public static bool operator ==(Identity left, Identity right) => left.Equals(right);
    public static bool operator !=(Identity left, Identity right) => !left.Equals(right);

    public Type Type => Id switch
    {
        <= 0 => LanguageType.Resolve(Decoration),
        _ => typeof(Entity),
    };

    public Identity Successor
    {
        get
        {
            if (!IsEntity) throw new InvalidCastException("Cannot reuse virtual Identities");
                
            var generationWrappedStartingAtOne = (TypeID) (Generation % (TypeID.MaxValue - 1) + 1);
            return new Identity(Id, generationWrappedStartingAtOne);
        }
    }

    public override string ToString()
    {
        if (this == None)
            return $"None";

        if (this == Any)
            return $"Any";

        return IsObject ? $"{Type}" : $"\u2756{Id:x8}:{Generation:D5}";
    }
}