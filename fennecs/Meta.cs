// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, Entity, and Generation.
/// </summary>
/// <remarks>
/// Archetype is the Archetype the Entity lives in.
/// Row is the position within the Archetype Table.
/// Entity is the Entity itself, Generation is a discriminator for the Entity's Generation.
/// This disciminator can be used to annotate an Entity by casting it to <see cref="EntityWithGeneration"/>.
/// </remarks>
internal readonly record struct Meta(Archetype Archetype, int Row, Entity Entity, int Generation = 0)
{
}
