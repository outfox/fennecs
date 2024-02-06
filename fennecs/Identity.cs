// SPDX-License-Identifier: MIT

namespace fennecs;

public readonly struct Identity(int id, ushort gen = 1) : IEquatable<Identity>
{
    public readonly int Id = id;
    public readonly ushort Generation = gen;

    public ulong Value => (uint) Id | (ulong) Generation << 32;

    public static readonly Identity None = new(0, 0);
    public static readonly Identity Any = new(int.MaxValue, 0);

    public Identity(ulong value) : this((int) (value & uint.MaxValue), (ushort) (value >> 32))
    {
        //Placeholder constructor until we can merge Identity and TypeExpression
    }

    public bool Equals(Identity other) => Id == other.Id && Generation == other.Generation;

    public override bool Equals(object? obj)
    {
        throw new InvalidCastException("Identity: Boxing equality comparisons disallowed. Use IEquatable<Identity>.Equals(Identity other) instead.");
        //return obj is Identity other && Equals(other); <-- second best option   
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var low = (uint) Id;
            var high = (uint) Generation;
            return (int) (0x811C9DC5u * low + 0x1000193u * high + 0xc4ceb9fe1a85ec53u);
        }
    }

    public bool IsType()
    {
        return Generation < 0;
    }

    public override string ToString()
    {
        if (IsType()) return $"\u2b1b{Type.Name}";
        if (Equals(None)) return $"\u25c7none";
        if (Equals(Any)) return $"\u2bc1any";

        return $"\u2756{Id:x4}:{Generation:D5}";
    }

    public static implicit operator Entity(Identity id) => new(id);
    public static bool operator ==(Identity left, Identity right) => left.Equals(right);
    public static bool operator !=(Identity left, Identity right) => !left.Equals(right);

    public Type Type => Generation switch
    {
        _ => typeof(System.Type),
        /*
        > 0 => typeof(Entity),
        < 0 => TypeRegistry.Resolve(Generation),
        _ => throw new InvalidCastException($"Entity: Cannot resolve type of pseudo-entity {this}")
        */
    };

    public Identity Successor
    {
        get
        {
            if (IsType()) throw new InvalidCastException("Cannot reuse type Identities");
            var generationWrappedStartingAtOne = (ushort) (Generation % (ushort.MaxValue - 1) + 1);
            return new Identity(Id, generationWrappedStartingAtOne);
        }
    }
}