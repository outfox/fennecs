// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// Meta Table that holds the Archetype, Row, and Entity of an "Entity"
/// (the semantic concept, not the <see cref="Entity"/> builder struct).
/// </summary>
/// <remarks>
/// Archetype is the Archetype the Entity lives in.
/// Row is the position within the Archetype Table.
/// Entity is the Entity itself (including Generation).
/// </remarks>
internal readonly record struct Meta(Archetype Archetype, int Row, Entity Entity);
