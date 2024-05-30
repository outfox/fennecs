// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, and Identity of an "Entity"
/// (the semantic concept, not the <see cref="Entity"/> builder struct).
/// </summary>
internal readonly record struct Meta(Archetype Archetype, int Row, Identity Identity)
{
    /// <summary>
    /// Archetype the Entity lives in.
    /// </summary>
    public Archetype Archetype { get; init; } = Archetype;

    /// <summary>
    /// Position within the Archetype Table
    /// </summary>
    public int Row { get; init; } = Row;

    /// <summary>
    /// Entity Identity
    /// </summary>
    public Identity Identity { get; init; } = Identity;
}
