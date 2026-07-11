// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Primary Kind of a <see cref="TypeExpression"/>: the storage class of the Component it expresses.
/// Occupies the top nibble (bits 60..63) of a packed TypeExpression.
/// </summary>
/// <remarks>
/// Values with the <see cref="Wildcard"/> bit (0x8) set match any of their non-wild counterparts;
/// <see cref="Any"/> matches every storage kind. Only <see cref="Data"/> is in use today — the
/// remaining values reserve headroom for future storage kinds (tags, singletons, sparse/spatial storages).
/// </remarks>
internal enum PrimaryKind : byte
{
    /// <summary>No type. (only valid in <c>default</c> TypeExpressions)</summary>
    None = 0x0,

    /// <summary>Future: zero-size Components (tags), saving storage and migration costs.</summary>
    Void = 0x1,

    /// <summary>Data Components. (all Components today)</summary>
    Data = 0x2,

    /// <summary>Future: singleton Components.</summary>
    Unique = 0x3,

    /// <summary>The Wildcard bit; not a valid kind by itself.</summary>
    Wildcard = 0x8,

    /// <summary>Future Wildcard: matches Void Components.</summary>
    WildVoid = Wildcard | Void,

    /// <summary>Future Wildcard: matches Data Components.</summary>
    WildData = Wildcard | Data,

    /// <summary>Future Wildcard: matches Unique Components.</summary>
    WildUnique = Wildcard | Unique,

    /// <summary>Future Wildcard: matches every storage kind.</summary>
    Any = 0xF,
}
