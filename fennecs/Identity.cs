// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Identity(ulong value) : IEquatable<Identity>, IComparable<Identity>
{
    [FieldOffset(0)] public readonly ulong Value = value;
    [FieldOffset(0)] public readonly int Id;
    [FieldOffset(4)] public readonly ushort Generation;
    [FieldOffset(4)] public readonly ushort Decoration;

    [FieldOffset(6)] public readonly short RESERVED = 0;

    [FieldOffset(0)] public readonly uint DWordLow;
    [FieldOffset(4)] public readonly uint DWordHigh;
    
    //public ulong Value => (uint) Id | (ulong) Generation << 32;

    public static readonly Identity None = new(0, 0);
    public static readonly Identity Any = new(-1, ushort.MaxValue);
    
    public bool IsEntity => Id > 0;
    public bool IsVirtual => !IsEntity;
    public bool IsType => IsVirtual && Decoration is > 0 and < ushort.MaxValue;

    public static implicit operator Identity(Type type) => new(type);

    public Identity(int id, ushort generation = 1) : this((uint) id | (ulong) generation << 32)
    {
    }

    internal Identity(ushort typeId) : this(0, typeId)
    {
    }

    public Identity(Type type) : this(LanguageType.Identify(type))
    {
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
            if (IsVirtual) throw new InvalidCastException("Cannot reuse virtual Identities");
            var generationWrappedStartingAtOne = (ushort) (Generation % (ushort.MaxValue - 1) + 1);
            return new Identity(Id, generationWrappedStartingAtOne);
        }
    }

    public override string ToString()
    {
        if (this == None)
            return $"None";

        if (this == Any)
            return $"Any";

        return IsType ? $"{Type}" : $"\u2756{Id:x8}:{Generation:D5}";
    }
}