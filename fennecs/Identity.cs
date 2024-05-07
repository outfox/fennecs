// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Refers to an identity:
/// real Entity, tracked object, or virtual concept (e.g. any/none Match Expression).
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct Identity : IEquatable<Identity>, IComparable<Identity>
{
    [FieldOffset(0)] internal readonly ulong Value;

    //Identity Components
    [FieldOffset(0)] public readonly int Index;
    [FieldOffset(4)] public readonly ushort Generation;
    [FieldOffset(4)] public readonly TypeID Decoration;

    //Type header (only used in TypeExpression, so must be 0 here) 
    [FieldOffset(6)] internal readonly TypeID RESERVED = 0;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(4)] internal readonly uint DWordHigh;


    // Entity Reference.
    public bool IsEntity => Index > 0 && Decoration > 0;

    // Tracked Object Reference.
    public bool IsObject => Decoration < 0;

    // Wildcard Entities, such as Any, Object, Entity, or Relation.
    public bool IsWildcard => Decoration == 0 && Index < 0;


    #region IComparable/IEquatable Implementation
    public static bool operator ==(Identity left, Identity right) => left.Equals(right);
    public static bool operator !=(Identity left, Identity right) => !left.Equals(right);

    public bool Equals(Identity other) => Value == other.Value;

    public int CompareTo(Identity other) => Value.CompareTo(other.Value);

    public static implicit operator Identity(Entity entity) => entity.Id;
    
    public static implicit operator bool(Identity self) => self != default;
    
    public override bool Equals(object? obj)
    {
        throw new InvalidCastException("Entity: Boxing equality comparisons disallowed. Use IEquatable<Entity>.Equals(Entity other) instead.");
        //return obj is Entity other && Equals(other); //<-- second best option   
    }


    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }
    #endregion


    public Type Type => Decoration switch
    {
        // Decoration is Type Id
        <= 0 => LanguageType.Resolve(Math.Abs(Decoration)),
        // Decoration is Generation
        _ => typeof(Identity),
    };


    #region Constructors / Creators
    public static Identity Of<T>(T item) where T : class => new(item.GetHashCode(), LanguageType<T>.TargetId);


    internal Identity(int id, TypeID decoration = 1) : this((uint) id | (ulong) decoration << 32)
    {
    }


    internal Identity(ulong value)
    {
        Value = value;
    }


    internal Identity Successor
    {
        get
        {
            if (!IsEntity) throw new InvalidCastException("Cannot reuse virtual Identities");

            var generationWrappedStartingAtOne = (TypeID) (Generation % (TypeID.MaxValue - 1) + 1);
            return new Identity(Index, generationWrappedStartingAtOne);
        }
    }
    #endregion


    public override string ToString()
    {
        if (Equals(Match.Plain))
            return "[None]";

        if (Equals(Match.Any))
            return "wildcard[Any]";

        if (Equals(Match.Target))
            return "wildcard[Target]";

        if (Equals(Match.Entity))
            return "wildcard[Entity]";

        if (Equals(Match.Object))
            return "wildcard[Object]";

        if (IsObject)
            return $"O-<{Type}>#{Index:X8}";

        if (IsEntity)
            return $"E-{Index:x8}:{Generation:D5}";

        return $"?-{Value:x16}";
    }
}