using System.Diagnostics;
using System.Numerics;

namespace fennecs.pools;

/// <summary>
/// Generation and Entity management for Entities
/// </summary>
/// <remarks>
/// Not thread-safe (yet?). This is the responsibility of the World that uses the pool.
/// </remarks>
internal sealed class EntityPool
{
    internal uint Created { get; private set; }

    internal int Alive => (int) (Created - (uint) _recycled.Count);

    private readonly Queue<Entity> _recycled;

    private readonly uint _worldTag;
    
    private readonly World.Id _worldId;

    private uint NewIndex => ++Created;

    // TODO: Make this configurable
    private const int MaxEntities = 0x00FF_FFFF;
    
    internal uint Discriminator(Entity entity) => _discriminators[entity.Index];
    
    private uint[] _discriminators = [];

    private const int MinimumRecycledCapacity = 1024;
    

    public EntityPool(World.Id worldId, int initialCapacity)
    {
        _worldId = worldId;
        _worldTag = (uint) worldId.Index << World.Shift;
        
        _recycled = new(initialCapacity * 2);

        for (var i = 0; i < initialCapacity; i++) _recycled.Enqueue(new(_worldTag, NewIndex));
        
        EnsureMinimumRecycledCapacity();
        EnsureDiscriminators();
    }
    
    private void EnsureDiscriminators()
    {
        if (_discriminators.Length >= Created) return;
        
        var last = _discriminators.Length;

        var newSize = Math.Min((int) BitOperations.RoundUpToPowerOf2(Created), MaxEntities);
        Array.Resize(ref _discriminators, newSize);
        
        _discriminators.AsSpan(last).Fill(1u);
    }

    private void EnsureMinimumRecycledCapacity()
    {
        if (_recycled.Count >= MinimumRecycledCapacity) return;
        for (var i = _recycled.Count; i < Math.Min(MinimumRecycledCapacity * 2, MaxEntities); i++) _recycled.Enqueue(new(_worldTag, NewIndex));
    }


    internal Entity Spawn()
    {
        if (_recycled.TryDequeue(out var entity)) return entity;
        
        entity = new(_worldTag, NewIndex);
        
        if (Created > MaxEntities) throw new InvalidOperationException($"Reached maximum number of Entities {MaxEntities} in World {_worldId}");
        EnsureDiscriminators();
        
        return entity;
    }


    internal PooledList<Entity> Spawn(int count)
    {
        var identities = PooledList<Entity>.Rent();
        var recycled = _recycled.Count;

        if (recycled <= count)
        {
            // Reuse all entities in the recycler.
            identities.AddRange(_recycled);
            _recycled.Clear();

            // If we don't have enough recycled Identities, create more.
            for (var i = 0; i < count - recycled; i++)
            {
                identities.Add(new(_worldTag, NewIndex));
            }
        }
        else
        {
            // Otherwise, take the requested amount from the recycled pool.
            for (var i = 0; i < count; i++)
            {
                //TODO: Optimize this!
                identities.Add(_recycled.Dequeue());
            }
        }

        EnsureMinimumRecycledCapacity();
        EnsureDiscriminators();
        
        return identities;
    }


    internal void Recycle(Entity entity)
    {
        // Increment Generation discriminator, and discard Entities whose discriminator is exhausted (wraps around).
        if (++_discriminators[entity.Index] > 0) _recycled.Enqueue(entity);
    }

    internal void Recycle(ReadOnlySpan<Entity> toDelete)
    {
        foreach (var entity in toDelete) Recycle(entity);
    }
}