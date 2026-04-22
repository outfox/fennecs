// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace fennecs;

/// <summary>
/// Refers to an identity:
/// real Entity, tracked object, or virtual concept (e.g. any/none Match Expression).
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal readonly record struct Identity : IComparable<Identity>
{
    [FieldOffset(0)] internal readonly ulong Value;

    //Identity Components
    [FieldOffset(0)] internal readonly int Index;
    [FieldOffset(4)] internal readonly ushort Generation;
    [FieldOffset(4)] internal readonly TypeID Decoration;

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
    /// Truthy if the Identity represents a virtual concept (see <see cref="Cross"/>).
    /// Falsy if it is an actual Entity or a tracked object.
    /// Falsy if it is the <c>default</c> Identity.
    /// </summary>
    public bool IsWildcard => Decoration == 0 && Index < 0;


    #region IComparable/IEquatable Implementation

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Identity other) => Value == other.Value;

    /// <inheritdoc cref="IComparable{T}"/>
    public int CompareTo(Identity other) => Value.CompareTo(other.Value);

    /// <summary>
    /// Casts an Entity to its Identity. (extracting the appropriatefield)
    /// </summary>
    /// <param name="Entity">an Entity</param>
    /// <returns>the Identity</returns>
    public static implicit operator Identity(Entity entity) => entity.Id;
    

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
    internal static Identity Of<T>(T item) where T : class => new(item != null! ? item.GetHashCode() : 0, LanguageType<T>.TargetId);
    
    
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
            if (!IsEntity) throw new InvalidOperationException("Cannot reuse virtual Identities");

            var generationWrappedStartingAtOne = (TypeID) (Generation % (TypeID.MaxValue - 1) + 1);
            return new Identity(Index, generationWrappedStartingAtOne);
        }
    }
    #endregion


    /// <inheritdoc />
    public override string ToString()
    {
        if (Equals(default))
            return "[None]";

        if (Equals(new(-1, 0)))
            return "wildcard[Any]";

        if (Equals(new(-2, 0)))
            return "wildcard[Target]";

        if (Equals(new(-3, 0)))
            return "wildcard[Entity]";

        if (Equals(new(-4, 0)))
            return "wildcard[Object]";

        if (IsObject)
            return $"O-<{Type}>#{Index:X8}";

        if (IsEntity)
            return $"E-{Index:x8}:{Generation:D5}";

        return $"?-{Value:x16}";
    }
    
    #region Wildcards

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches all types of relations on the given Stream Type: <b>Plain, Entity, and Object</b>.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over Entities if they match multiple Component types. This is due to the Wildcard's nature of matching all Components.</para>
    /// </summary>
    /// <remarks>
    /// <para>⚠️ Using Wildcards can lead to a CROSS JOIN effect, iterating over Entities multiple times for
    /// each matching Component. While querying is efficient, this increases the number of operations per entity.</para>
    /// <para>⚠️ Note: <c>Match.Plain</c> is the default, not <c>Match.Any</c>. This protects users from accidentally
    /// triggering cross-join behavior. Use Wildcards deliberately when you need to match multiple relation targets.</para>
    /// <para>This effect is more pronounced in large archetypes with many matching Components, potentially
    /// multiplying the workload significantly. However, for smaller archetypes or simpler tasks, impacts are minimal.</para>
    /// <para>Risks and considerations include:</para>
    /// <ul>
    /// <li>Repeated enumeration: Entities matching a Wildcard are processed multiple times, for each matching
    /// Component type combination.</li>
    /// <li>Complex queries: Especially in Archetypes where Entities match multiple Components, multiple Wildcards
    /// can create a cartesian product effect, significantly increasing complexity and workload.</li>
    /// <li>Use Wildcards deliberately and sparingly.</li>
    /// </ul>
    /// </remarks>
    public static Identity Any => new(-1, 0); // or prefer default ?

    /// <summary>
    /// <b>Wildcard match expression for Entity iteration.</b><br/>Matches any non-plain Components of the given Stream Type, i.e. any with a <see cref="TypeExpression.Match"/>.
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over Entities if they match multiple Component types. This is due to the Wildcard's nature of matching all Components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Identity Target => new(-2, 0);

    /// <summary>
    /// <para>Wildcard match expression for Entity iteration. <br/>This matches all <b>Entity-Object</b> Links of the given Stream Type.
    /// </para>
    /// <para>Use it freely in filter expressions to match any Component type. See <see cref="QueryBuilder"/> for how to apply it in queries.</para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over Entities if they match multiple Component types. This is due to the Wildcard's nature of matching all Components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Identity Object => new(-4, 0);

    /// <summary>
    /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches only <b>Entity-Entity</b> Relations of the given Stream Type.
    /// </para>
    /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
    /// </para>
    /// <para>Applying this to a Query's Stream Type can result in multiple iterations over Entities if they match multiple Component types. This is due to the Wildcard's nature of matching all Components.</para>
    /// </summary>
    /// <inheritdoc cref="Any"/>
    public static Identity Entity => new(-3, 0);

    /// <summary>
    /// <para>
    /// <c>default</c><br/>In Query Matching; matches ONLY Plain Components, i.e. those without a Relation Target.
    /// </para>
    /// <para>
    /// Since it's specific, this Match Expression is always free and has no enumeration cost.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Not a Wildcard. Formerly known as "None", as plain Components without a target
    /// can only exist once per Entity (same as Components with a particular target).
    /// </remarks>
    public static Identity Plain => default;
    
    #endregion
}
