// SPDX-License-Identifier: MIT

namespace fennecs;

public partial class World
{
    #region Static World Registry

    // Entities carry an 8-bit world tag (see Entity.WorldTag) that resolves against this registry,
    // so stored 64-bit handles can route their fluent API calls without holding a World reference.
    private static readonly World?[] Worlds = new World?[256];

    // Tag 0 is permanently reserved so default(Entity) can never resolve to a World.
    private static readonly Queue<byte> FreeTags = new(Enumerable.Range(1, 255).Select(i => (byte) i));

    private static readonly Lock TagLock = new();


    /// <summary>
    /// The tag identifying this World in the process-wide registry. (bits 32..39 of its Entities)
    /// </summary>
    internal byte Tag { get; private set; }


    /// <summary>
    /// Resolves a World by its tag.
    /// </summary>
    /// <exception cref="InvalidOperationException">if no World with that tag exists (default Entity, or its World was Disposed)</exception>
    internal static World Get(byte tag)
    {
        var world = Worlds[tag];
        if (world is null) throw new InvalidOperationException($"No World with tag {tag} — the Entity is default, or its World has been Disposed.");
        return world;
    }


    /// <summary>
    /// Resolves a World by its tag, without throwing.
    /// </summary>
    internal static bool TryGet(byte tag, out World? world)
    {
        world = Worlds[tag];
        return world is not null;
    }


    private void ClaimTag()
    {
        lock (TagLock)
        {
            if (!FreeTags.TryDequeue(out var tag))
            {
                throw new InvalidOperationException("Too many concurrent Worlds: a process supports up to 255. Dispose() Worlds that are no longer used.");
            }

            Tag = tag;
            Worlds[tag] = this;
        }
    }


    private void ReleaseTag()
    {
        lock (TagLock)
        {
            if (Tag == 0) return; // already released (double Dispose)

            Worlds[Tag] = null;
            FreeTags.Enqueue(Tag);
            Tag = 0;
        }
    }

    #endregion
}
