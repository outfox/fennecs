// SPDX-License-Identifier: MIT

namespace fennecs;

internal struct EntityMeta(Entity entity, Archetype archetype, int row)
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
    internal Entity Entity = entity;

    internal void Clear()
    {
        Entity = Entity.None;
        Archetype = null!;
        Row = 0;
    }
}