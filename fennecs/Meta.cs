// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, and Entity of an "Entity"
/// (the semantic concept, not the <see cref="Entity"/> builder struct).
/// </summary>
internal readonly record struct Meta(Archetype Archetype, int Row, Entity Entity)
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
    /// Entity Entity
    /// </summary>
    public Entity Entity { get; init; } = Entity;
}
