// SPDX-License-Identifier: MIT

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
    //   | Entity (Identity)            | TypeNumber |
    //   | 48 bits                      |  16 bits   |
    
    //   PLANNED:
    //   TypeNumber
    //   | Type    | Flags |
    //   | 14 bits | 2 bits |
    
    //   Flags
    //   00 - Component Type
    //   01 - Component Type Targeting Entity
    //   10 - Component Type Targeting WeakReference
    //   11 - Reserved (for potential hash-bucket storage features)
    
    
    //Union Backing Store
    [FieldOffset(0)] public readonly ulong Value;

    //Identity Components
    [FieldOffset(0)] public readonly int Id;
    [FieldOffset(4)] public readonly ushort Generation;
    [FieldOffset(4)] public readonly TypeID Decoration;
    
    // Type Header
    [FieldOffset(6)] public readonly TypeID TypeId;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(4)] internal readonly uint DWordHigh;

    
    public Entity Target => new(Id, Decoration);
    
    public bool isRelation => TypeId != 0 && Target != Entity.None;

    public Type Type => LanguageType.Resolve(TypeId);

    public bool Matches(IEnumerable<TypeExpression> other)
    {
        var self = this;
        return other.Any(type => self.Matches(type));
    }
    
    /// <summary>
    /// Match against another TypeExpression; used for Query Matching, and is Non-Commutative.
    /// Examines the Target field and decides whether the other TypeExpression is a match.
    /// </summary>
    /// <param name="other">another type expression</param>
    /// <returns>true if matched</returns>
    public bool Matches(TypeExpression other)
    {
        // Reject if Types are incompatible. 
        if (TypeId != other.TypeId) return false;

        // Entity.None matches only None. (plain components)
        if (Target == Entity.None) return other.Target == Entity.None;

        // Entity.Any matches everything; relations and pure components (target == none).
        if (Target == Entity.Any) return true;
        
        // Entity.Target matches all Entity-Target Relations.
        if (Target == Entity.Target) return other.Target != Entity.None;
        
        // Entity.Relation matches only Entity-Entity relations.
        if (Target == Entity.Relation) return other.Target.IsEntity;
        
        // Entity.Object matches only Entity-Object relations.
        if (Target == Entity.Object) return other.Target.IsObject;

        // Direct match?
        return Target == other.Target;
    } 

    public bool Equals(TypeExpression other) => Value == other.Value;

    public int CompareTo(TypeExpression other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => throw new InvalidCastException("Boxing Disallowed; use TypeId.Equals(TypeId) instead.");

    public static TypeExpression Create<T>(Entity target = default)
    {
        return new TypeExpression(target, LanguageType<T>.Id);
    }

    public static TypeExpression Create(Type type, Entity target = default)
    {
        return new TypeExpression(target, LanguageType.Identify(type));
    }
    

    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
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

    internal TypeExpression(Entity target, TypeID typeId)
    {
        Value = target.Value;
        TypeId = typeId;
    }

    public override string ToString()
    {
        return isRelation ? $"<{LanguageType.Resolve(TypeId)}\u2192{Target}>" : $"<{LanguageType.Resolve(TypeId)}>";
    }
}