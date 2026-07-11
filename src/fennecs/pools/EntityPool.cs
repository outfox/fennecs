// SPDX-License-Identifier: MIT

using System.Numerics;

namespace fennecs.pools;

/// <summary>
/// Mints, recycles, and validates the Entities of a World.
/// </summary>
/// <remarks>
/// Entity indices are recycled after despawn; each index carries an out-of-band generation
/// (starting at 1, incremented on despawn) that stored 64-bit <see cref="Entity"/> handles are
/// validated against. An index whose generation would wrap is retired instead of recycled.
/// Index 0 is reserved so that <c>default(Entity)</c> is never alive.
/// </remarks>
internal class EntityPool
{
    internal int Created => _created;
    internal int Count => _created - _recycled.Count - _retired;

    private readonly Queue<uint> _recycled;
    private ushort[] _generations;
    private int _created;
    private int _retired;

    // The constant bits 32..47 of every Entity of this World: [kind:4][flags:4][world:8]
    private readonly ulong _midBits;


    public EntityPool(byte worldTag, int initialCapacity = 4096)
    {
        _midBits = Entity.KindBits | ((ulong) worldTag << Key.WorldShift);

        _generations = new ushort[BitOperations.RoundUpToPowerOf2((uint) Math.Max(initialCapacity + 1, 16))];
        _generations.AsSpan(1).Fill(1); // index 0 reserved; generations start at 1

        _recycled = new(initialCapacity * 2);
        for (var i = 0; i < initialCapacity; i++)
        {
            _recycled.Enqueue((uint) ++_created);
        }
    }


    private Entity MakeEntity(uint index) => new(((ulong) _generations[index] << Key.GenShift) | _midBits | index);


    /// <summary>
    /// Mints the current (live) Entity handle for an index.
    /// </summary>
    internal Entity EntityFor(uint index) => MakeEntity(index);


    /// <summary>
    /// The current generation of an index. (caller guarantees the index was spawned at least once)
    /// </summary>
    internal ushort GenerationOf(uint index) => _generations[index];


    /// <summary>
    /// Is this handle's generation current for its index (and does it belong to this World)?
    /// </summary>
    internal bool IsAlive(Entity entity)
    {
        var index = entity.Index;
        return index != 0
               && index <= (uint) _created
               && (ushort) (entity.Value >> 32) == (ushort) (_midBits >> 32)
               && entity.Generation == _generations[index];
    }


    internal Entity Spawn()
    {
        if (_recycled.TryDequeue(out var index)) return MakeEntity(index);

        var newIndex = (uint) Interlocked.Increment(ref _created);
        EnsureGenerations(newIndex);
        return MakeEntity(newIndex);
    }


    internal PooledList<Entity> Spawn(int requested)
    {
        var entities = PooledList<Entity>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= requested)
        {
            // Reuse all Entities in the recycler.
            while (_recycled.TryDequeue(out var index)) entities.Add(MakeEntity(index));

            // If we don't have enough recycled indices, create more.
            for (var i = 0; i < requested - recycled; i++)
            {
                var newIndex = (uint) ++_created;
                EnsureGenerations(newIndex);
                entities.Add(MakeEntity(newIndex));
            }
        }
        else
        {
            // Otherwise, take the requested amount from the recycled pool.
            for (var i = 0; i < requested; i++)
            {
                entities.Add(MakeEntity(_recycled.Dequeue()));
            }
        }

        return entities;
    }


    internal void Recycle(Entity entity)
    {
        var index = entity.Index;

        // Despawns must present the exact-generation snapshot of the Entity they kill; this is
        // what lets the World diagnose stale handles ("already despawned" vs "respawned since"),
        // and is the hook point for future despawn journaling (entity.Value + call site).
        System.Diagnostics.Debug.Assert(entity.Generation == _generations[index],
            $"Recycling a stale Entity snapshot: {entity} has generation {entity.Generation}, but its index is at generation {_generations[index]}.");

        // Bump the generation, invalidating all stored handles to this index.
        var next = ++_generations[index];

        // Retire the index if its generation space is exhausted (wrapped to 0).
        if (next == 0) _retired++;
        else _recycled.Enqueue(index);
    }


    private void EnsureGenerations(uint maxIndex)
    {
        if (maxIndex < (uint) _generations.Length) return;

        var last = _generations.Length;
        Array.Resize(ref _generations, (int) BitOperations.RoundUpToPowerOf2(maxIndex + 1));
        _generations.AsSpan(last).Fill(1);
    }
}
