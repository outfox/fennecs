// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, and Identity of an "Entity"
/// (the semantic concept, not the <see cref="Entity"/> builder struct).
/// </summary>
internal struct Meta(Identity identity, Archetype archetype, int row)
{
    /// <summary>
    /// Archetype the entity lives in.
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


    internal void Clear()
    {
        Identity = default;
        Archetype = default!;
        Row = 0;
    }
}