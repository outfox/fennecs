// SPDX-License-Identifier: MIT

namespace fennecs;

internal struct EntityMeta(Identity identity, Archetype archetype, int row)
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
        Identity = Match.Plain;
        Archetype = null!;
        Row = 0;
    }
}