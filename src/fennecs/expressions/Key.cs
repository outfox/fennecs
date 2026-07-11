// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace fennecs;

/// <summary>
/// The category of a <see cref="Key"/>'s target: which kinds of secondary targets it describes or matches.
/// </summary>
/// <remarks>
/// This nibble is shared between <see cref="Entity"/> (bits 44..47 of its raw value) and
/// <see cref="TypeExpression"/> keys, so an Entity's low 48 bits are always a valid Entity key.
/// </remarks>
[Flags]
internal enum SecondaryKind : byte
{
    /// <summary>Plain Components (no secondary target). The <c>default</c>.</summary>
    None = 0,

    /// <summary>Reserved for future keyed/hashed Components.</summary>
    Data = 0x1,

    /// <summary>Entity relation target.</summary>
    Entity = 0x2,

    /// <summary>Object Link target.</summary>
    Object = 0x4,

    /// <summary>Reserved for future Family (hash-keyed) targets.</summary>
    Family = 0x8,

    /// <summary>Any non-plain target (relations and links).</summary>
    Target = Entity | Object | Family,

    /// <summary>Any target, including plain.</summary>
    Any = Data | Target,
}


/// <summary>
/// Secondary Key of a Component type expression: the target of a relation or Object Link,
/// or a Wildcard matching categories of targets.
/// </summary>
/// <remarks>
/// Layout (48 bits, the low 48 bits of <see cref="TypeExpression"/> and <see cref="Entity"/>):
/// <code>
/// [SecondaryKind:4 (47..44)] [sub:12 (43..32)] [value:32 (31..0)]
/// </code>
/// <para>Entity keys: sub = [flags:4][world:8], value = entity index. (never zero — index 0 is reserved)</para>
/// <para>Object keys: sub = TypeId of the linked type, value = the object's hash code. (sub is never zero)</para>
/// <para>Wildcards: a non-<see cref="SecondaryKind.None"/> kind nibble with zero payload.</para>
/// <para>Plain (no target): <c>default</c>.</para>
/// </remarks>
internal readonly record struct Key : IComparable<Key>
{
    internal readonly ulong Value;

    internal Key(ulong value)
    {
        Debug.Assert((value & HeaderMask) == 0, "Key must not have header bits set.");
        Value = value;
    }


    #region Layout

    /// <summary>Header bits (Generation in an Entity; PrimaryKind + TypeId in a TypeExpression).</summary>
    internal const ulong HeaderMask = 0xFFFF_0000_0000_0000ul;

    /// <summary>The 48 bits of a Key.</summary>
    internal const ulong Mask = ~HeaderMask;

    /// <summary>SecondaryKind nibble.</summary>
    internal const ulong KindMask = 0x0000_F000_0000_0000ul;

    /// <summary>12-bit sub field (linked type for Object keys; [flags:4][world:8] for Entity keys).</summary>
    internal const ulong SubMask = 0x0000_0FFF_0000_0000ul;

    /// <summary>8-bit World tag within an Entity key's sub field.</summary>
    internal const ulong WorldMask = 0x0000_00FF_0000_0000ul;

    /// <summary>32-bit value field (entity index, or object hash code).</summary>
    internal const ulong ValueMask = 0x0000_0000_FFFF_FFFFul;

    /// <summary>Everything below the kind nibble; zero ⇔ wildcard or plain.</summary>
    internal const ulong PayloadMask = SubMask | ValueMask;

    internal const int GenShift = 48;
    internal const int KindShift = 44;
    internal const int SubShift = 32;
    internal const int WorldShift = 32;

    #endregion


    internal SecondaryKind Kind => (SecondaryKind) ((Value & KindMask) >> KindShift);

    /// <summary>The sub and value fields; zero ⇔ this Key is a Wildcard (or Plain).</summary>
    internal ulong Payload => Value & PayloadMask;

    /// <summary>Entity index (Entity keys) or object hash code (Object keys).</summary>
    internal uint Index => (uint) Value;

    /// <summary>World tag of an Entity key.</summary>
    internal byte WorldTag => (byte) ((Value & WorldMask) >> WorldShift);

    /// <summary>Sub field: TypeId of the linked type for Object keys.</summary>
    internal TypeID Sub => (TypeID) ((Value & SubMask) >> SubShift);


    /// <summary>Is this Key a Wildcard? (matches a category of targets rather than one specific target)</summary>
    public bool IsWildcard => Payload == 0 && Kind != SecondaryKind.None;

    /// <summary>Does this Key target one specific Entity?</summary>
    public bool IsEntity => Kind == SecondaryKind.Entity && Payload != 0;

    /// <summary>Does this Key target one specific linked Object?</summary>
    public bool IsObject => Kind == SecondaryKind.Object && Payload != 0;

    /// <summary>Does this Key target something specific (an Entity or an Object)?</summary>
    public bool IsRelation => Payload != 0 && Kind is SecondaryKind.Entity or SecondaryKind.Object;


    /// <summary>
    /// Create a Key for a tracked object, backed by the Object Link type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// The Key consists of the linked type and the object's <see cref="object.GetHashCode"/>.
    /// </remarks>
    internal static Key Of<T>(T item) where T : class =>
        new(((ulong) SecondaryKind.Object << KindShift)
            | ((ulong) (ushort) LanguageType<T>.Id << SubShift)
            | (uint) (item?.GetHashCode() ?? 0));


    /// <summary>
    /// Extracts the Key of an Entity. (drops the Generation header)
    /// </summary>
    public static implicit operator Key(Entity entity) => entity.Key;


    #region Wildcards

    /// <summary>Matches only Plain Components (no secondary target). Not a Wildcard; the <c>default</c>.</summary>
    public static Key Plain => default;

    /// <summary>Wildcard: matches any target, INCLUDING Plain.</summary>
    public static Key Any => new((ulong) SecondaryKind.Any << KindShift);

    /// <summary>Wildcard: matches any non-plain target (relations and links), EXCLUDING Plain.</summary>
    public static Key Target => new((ulong) SecondaryKind.Target << KindShift);

    /// <summary>Wildcard: matches only Entity-Entity relation targets.</summary>
    public static Key AnyEntity => new((ulong) SecondaryKind.Entity << KindShift);

    /// <summary>Wildcard: matches only Entity-Object Link targets.</summary>
    public static Key AnyObject => new((ulong) SecondaryKind.Object << KindShift);

    #endregion


    /// <summary>The linked type of an Object key. (only valid for Object keys)</summary>
    internal Type Type => LanguageType.Resolve(Sub);


    /// <inheritdoc />
    public int CompareTo(Key other) => Value.CompareTo(other.Value);


    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();


    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == 0) return "[None]";

        if (IsWildcard)
            return Kind switch
            {
                SecondaryKind.Any => "wildcard[Any]",
                SecondaryKind.Target => "wildcard[Target]",
                SecondaryKind.Entity => "wildcard[Entity]",
                SecondaryKind.Object => "wildcard[Object]",
                _ => $"wildcard[?-{Value:x16}]",
            };

        if (IsObject) return $"O-<{Type}>#{Index:X8}";

        if (IsEntity) return $"E-{Index:x8}@{WorldTag:d3}";

        return $"?-{Value:x16}";
    }
}
