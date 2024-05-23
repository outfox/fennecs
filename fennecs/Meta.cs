// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, and Identity of an "Entity"
/// (the semantic concept, not the <see cref="Entity"/> builder struct).
/// </summary>
internal struct Meta(Identity identity = default, Archetype archetype = null!, int row = -1)
{
    /// <summary>
    /// Archetype the Entity lives in.
    /// </summary>
    internal Archetype Archetype = archetype;

    /// <summary>
    /// Position within the Archetype Table
    /// </summary>
    internal int Row = row;

    /// <summary>
    /// Entity Identity
    /// </summary>
    internal Identity Identity = identity;


    internal static Meta Empty() => new();
}