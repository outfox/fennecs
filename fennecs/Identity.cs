// SPDX-License-Identifier: MIT

using System.Diagnostics;
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
    [FieldOffset(0)] internal readonly int Index;
    [FieldOffset(4)] internal readonly ushort Generation;
    [FieldOffset(4)] internal readonly TypeID Decoration;

    //Type header (only used in TypeExpression, so must be 0 here) 
    [FieldOffset(6)] internal readonly TypeID RESERVED = 0;

    //Constituents for GetHashCode()
    [FieldOffset(0)] internal readonly uint DWordLow;
    [FieldOffset(4)] internal readonly uint DWordHigh;


    // Entity Reference.
    /// <summary>
    /// Truthy if the Identity represents an actual Entity.
    /// Falsy if it is a virtual concept or a tracked object.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsEntity => Index > 0 && Decoration > 0;

    // Tracked Object Reference.
    /// <summary>
    /// Truthy if the Identity represents a tracked object.
    /// Falsy if it is a virtual concept or an actual Entity.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsObject => Decoration < 0;

    // Wildcard Entities, such as Any, Object, Entity, or Relation.
    /// <summary>
    /// Truthy if the Identity represents a virtual concept (see <see cref="Match"/>).
    /// Falsy if it is an actual Entity or a tracked object.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsWildcard => Decoration == 0 && Index < 0;


    #region IComparable/IEquatable Implementation
    /// <inheritdoc cref="Equals(fennecs.Identity)"/>
    public static bool operator ==(Identity left, Identity right) => left.Equals(right);
    /// <inheritdoc cref="Equals(fennecs.Identity)"/>
    public static bool operator !=(Identity left, Identity right) => !left.Equals(right);

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Identity other) => Value == other.Value;

    /// <inheritdoc cref="IComparable{T}"/>
    public int CompareTo(Identity other) => Value.CompareTo(other.Value);

    /// <summary>
    /// Casts an Entity to its Identity. (extracting the appropriatefield)
    /// </summary>
    /// <param name="entity">an Entity</param>
    /// <returns>the Identity</returns>
    public static implicit operator Identity(Entity entity) => entity.Id;
    
    /// <summary>
    /// Truthy if the Identity is not default.
    /// </summary>
    /// <param name="self">an Identity</param>
    /// <returns>truthiness value</returns>
    public static implicit operator bool(Identity self) => self != default;
    
    
    ///<summary>
    /// Implements <see cref="System.IEquatable{T}"/>.Equals(object? obj)
    /// </summary>
    /// <remarks>
    /// ⚠️This method ALWAYS throws InvalidCastException, as boxing of this type is disallowed.
    /// </remarks>
    public override bool Equals(object? obj)
    {
        throw new InvalidCastException("fennecs.Identity: Boxing equality comparisons disallowed. Use IEquatable<Identity>.Equals(Identity other) instead.");
    }


    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (int) (0x811C9DC5u * DWordLow + 0x1000193u * DWordHigh + 0xc4ceb9fe1a85ec53u);
        }
    }
    #endregion


    internal Type Type => Decoration switch
    {
        // Decoration is Type Id
        <= 0 => LanguageType.Resolve(Math.Abs(Decoration)),
        // Decoration is Generation
        _ => typeof(Identity),
    };


    #region Constructors / Creators
    /// <summary>
    /// Create an Identity for a tracked object and the backing Object Link type.
    /// Used to set targets of Object Links. 
    /// </summary>
    /// <param name="item">target item (an instance of object)</param>
    /// <typeparam name="T">type of the item (becomes the backing type of the object link)</typeparam>
    /// <returns></returns>
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


    /// <inheritdoc />
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