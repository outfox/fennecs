// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, and Identity of an "Entity"
/// (the semantic concept, not the <see cref="Entity"/> builder struct).
/// </summary>
internal readonly struct Meta(Identity identity = default, Archetype archetype = null!, int row = -1)
{
    /// <summary>
    /// Archetype the Entity lives in.
    /// </summary>
    internal readonly Archetype Archetype = archetype;

    /// <summary>
    /// Position within the Archetype Table
    /// </summary>
    internal readonly int Row = row;

    /// <summary>
    /// Entity Identity
    /// </summary>
    internal readonly Identity Identity = identity;


    /// <summary>
    /// Empty Meta is not exactly "default" (row is -1)
    /// TODO: Might change this, as it makes little difference.
    /// </summary>
    internal static Meta Empty() => new();
}