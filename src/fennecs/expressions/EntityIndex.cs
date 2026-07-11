// SPDX-License-Identifier: MIT

namespace fennecs;

/// <summary>
/// The 32-bit entity column type present in every Archetype: just the Entity's index within
/// its World. World and generation are implicit — the Archetype knows its World, and only
/// live Entities occupy rows — so enumerating entities costs 4 bytes of bandwidth per row.
/// </summary>
/// <remarks>
/// Promote to a full 64-bit <see cref="Entity"/> via <see cref="fennecs.World.EntityFor(EntityIndex)"/>,
/// which injects the world tag and looks up the current generation.
/// </remarks>
internal readonly record struct EntityIndex(uint Raw)
{
    /// <inheritdoc />
    public override string ToString() => $"#{Raw:x8}";
}
