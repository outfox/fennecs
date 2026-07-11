// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table entry that holds the Archetype and Row where an Entity's data lives.
/// (indexed by the Entity's index; liveness is validated out-of-band via the World's generation table)
/// </summary>
internal readonly record struct Meta(Archetype Archetype, int Row)
{
    /// <summary>
    /// Archetype the Entity lives in. (null if the Entity is not a member of the Aspect)
    /// </summary>
    public Archetype Archetype { get; init; } = Archetype;

    /// <summary>
    /// Position within the Archetype Table
    /// </summary>
    public int Row { get; init; } = Row;
}
