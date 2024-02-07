// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace fennecs;

[StructLayout(LayoutKind.Explicit)]
public readonly struct TypeExpression : IEquatable<TypeExpression>, IComparable<TypeExpression>
{
    //             This is a 64 bit union struct.
    //                 Layout: (little endian)
    //   | LSB                                   MSB |
    //   |-------------------------------------------|
    //   | Value                                     |
    //   | 64 bits                                   |
    //   |-------------------------------------------|
    //   | Id              | Generation | TypeNumber |
    //   | 32 bits         |  16 bits   |  16 bits   |
    //   |-------------------------------------------|
    //   | Identity                     | TypeNumber |
    //   | 48 bits                      |  16 bits   |
    
    //Union Backing Store
    [FieldOffset(0)] public readonly ulong Value;

    //Identity Components
    [FieldOffset(0)] public readonly int Id;
    [FieldOffset(4)] public readonly ushort Generation;
    [FieldOffset(4)] public readonly ushort Decoration;
    [FieldOffset(6)] public readonly ushort TypeId;

    public Identity Target => new(Value);

    private struct Any;
    private struct None;
    
    public bool isRelation => TypeId != 0 && Target != Identity.None;
    public bool isBacklink => typeof(IRelationBacklink).IsAssignableFrom(Type);
    public bool isEntity => TypeId == 0;

    public Type Type
    {
        get
        {
            return (TypeId, Id) switch
            {
                (0, int.MaxValue) => typeof(Any),
                (0, 0) => typeof(None),
                (0, _) => typeof(Entity),
                _ => TypeRegistry.Resolve(TypeId),
            };
        }
    }
    
    
    public bool Matches(IEnumerable<TypeExpression> other)
    {
        var self = this;
        return other.Any(type => self.Matches(type));
    }
    
    public bool Matches(TypeExpression other)
    {
        // Reject if Type completely incompatible 
        if (TypeId != other.TypeId) return false;

        // Most common case.
        if (Target == Identity.None) return other.Target == Identity.None;
        
        // Any only matches other Relations, not None.
        if (Target == Identity.Any) return other.Target != Identity.None;

        // Direct match.
        if (Target == other.Target) return true;
        
        // For commutative matching only. (usually a TypeId from a Query is matched against one from a Table)
        return other.Target == Identity.Any;
    } 

    public bool Equals(TypeExpression other) => Value == other.Value;

    public int CompareTo(TypeExpression other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => throw new InvalidCastException("Boxing Disallowed; use TypeId.Equals(TypeId) instead.");

    public static TypeExpression Create<T>(Identity target = default) 
    {
        return new TypeExpression(target, LanguageType<T>.Id);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var low = (uint) (Value & 0xFFFFFFFFu);
            var high = (uint) (Value >> 32);
            return (int) (0x811C9DC5u * low + 0x1000193u * high + 0xc4ceb9fe1a85ec53u);
        }
    }


    public static bool operator ==(TypeExpression left, TypeExpression right)
    {
        return left.Equals(right);
    }


    public static bool operator !=(TypeExpression left, TypeExpression right)
    {
        return !(left == right);
    }

    public static implicit operator ulong(TypeExpression self) => self.Value;


    [SetsRequiredMembers]
    private TypeExpression(Identity target, ushort typeId)
    {
        Value = target.Value;
        TypeId = typeId;
    }

    public override string ToString()
    {
        return $"{TypeId:x4}/{Target} {TypeRegistry.Resolve(TypeId).Name}";
    }
}