// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    #region World State & Storage

    private readonly EntityPool _entityPool;

    private readonly Guid _guid = Guid.NewGuid();

    /// <summary>
    /// The default Aspect of this World. All component data lives here unless routed to another Aspect.
    /// Every living Entity is a member of the Main Aspect.
    /// </summary>
    public Aspect Main { get; }

    #endregion


    #region Locking & Deferred Operations

    //private readonly LockType _spawnLock = new();
    //private readonly LockType _modeChangeLock = new();
    private int _locks;

    internal WorldMode Mode { get; private set; } = WorldMode.Immediate;


    // Guards against a nested drain: a Lock taken *while* the queue is being drained (Signal
    // dispatch does exactly that) must not start a second CatchUp when it is released.
    private bool _catchingUp;


    private void Unlock()
    {
        if (Interlocked.Decrement(ref _locks) > 0) return;

        // The enclosing drain loop picks up whatever this scope queued. Without this, a bulk
        // deferred operation would recurse one stack frame per Entity — and overflow the stack.
        if (_catchingUp)
        {
            Mode = WorldMode.CatchUp;
            return;
        }

        _catchingUp = true;
        Mode = WorldMode.CatchUp;
        try
        {
            CatchUp(_deferredOperations);
        }
        finally
        {
            _catchingUp = false;
            Mode = WorldMode.Immediate;
        }
    }


    internal enum WorldMode
    {
        Immediate = 0,
        CatchUp,
        Deferred,
    }

    #endregion


    #region CRUD

    private Entity NewEntity()
    {
        var entity = _entityPool.Spawn();

        Main.EnsureCapacity(_entityPool.Created + 1);
        Main.Join(entity);

        return entity;
    }

    internal void SpawnBare(Span<Entity> destination)
    {
        _entityPool.Spawn(destination);
        Main.EnsureCapacity(_entityPool.Created + 1);
    }


    internal bool HasComponent(Entity entity, TypeExpression typeExpression)
    {
        return IsAlive(entity) && AspectOf(typeExpression).HasComponent(entity, typeExpression);
    }


    /// <summary>
    /// Mints the current (live) Entity handle for a relation Key.
    /// </summary>
    internal Entity EntityFor(Key key)
    {
        System.Diagnostics.Debug.Assert(key.IsEntity, "Key does not target an Entity.");
        System.Diagnostics.Debug.Assert(key.WorldTag == Tag, "Key belongs to another World.");
        return _entityPool.EntityFor(key.Index);
    }


    /// <summary>
    /// Mints the current (live) Entity handle for an entity column entry.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Entity EntityFor(EntityIndex index) => _entityPool.EntityFor(index.Raw);


    private void DespawnImpl(Entity entity)
    {
        AssertAlive(entity);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation { Opcode = Opcode.Despawn, Entity = entity, FromSignal = InSignalDispatch });
            return;
        }

        // Scanning what the Entity actually holds is cheaper than the Lock, and a despawn of
        // Components nobody watches must stay as fast as it was before Signals existed.
        if (WatchingAnyOf(entity))
        {
            // Aspect.Despawn emits Removed for everything it stores for the Entity; the lock keeps
            // whatever the handlers do from interleaving with the despawn itself.
            using var worldLock = Lock();
            DespawnRows(entity);
            return;
        }

        DespawnRows(entity);
    }


    /// <summary>
    /// Removes the Entity from every Aspect that contains it, cleans up Relations targeting it
    /// wherever they live, and returns its Identity to the pool. (Main always contains it)
    /// </summary>
    private void DespawnRows(Entity entity)
    {
        if (_aspects.Count == 1)
        {
            Main.Despawn(entity);
        }
        else
        {
            foreach (var aspect in _aspects)
            {
                aspect.Despawn(entity);
            }
        }

        _entityPool.Recycle(entity);
    }

    #endregion


    #region Queries

    internal Query CompileQuery(Mask mask) => ResolveAspect(mask).CompileQuery(mask);


    internal ref Meta GetEntityMeta(Entity entity) => ref Main.GetEntityMeta(entity);


    internal IReadOnlyList<Component> GetComponents(Entity entity)
    {
        if (!IsAlive(entity)) return [];

        if (_aspects.Count == 1) return Main.GetComponents(entity);

        var components = new List<Component>(Main.GetComponents(entity));
        for (var i = 1; i < _aspects.Count; i++)
        {
            components.AddRange(_aspects[i].GetComponents(entity));
        }
        return components;
    }

    /// <inheritdoc />
    public override int GetHashCode() => _guid.GetHashCode();

    #endregion


    #region Assert Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertAlive(Entity entity)
    {
        if (IsAlive(entity)) return;

        ThrowDead(entity);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowDead(Entity entity) => throw new ObjectDisposedException(null, DescribeDead(entity));


    /// <summary>
    /// Diagnoses why an Entity handle is not alive in this World, using the handle's
    /// exact-generation snapshot against the current generation of its index.
    /// </summary>
    internal string DescribeDead(Entity entity)
    {
        if (entity == default) return "default(Entity) is not a spawned Entity.";

        if (entity.WorldTag != Tag) return $"Entity {entity} belongs to another World (world tag {entity.WorldTag}; this is World \"{Name}\" with tag {Tag}).";

        if (entity.Index == 0 || entity.Index > (uint)_entityPool.Created) return $"Entity {entity} was never spawned in this World.";

        var current = _entityPool.GenerationOf(entity.Index);

        if (entity.Generation < current)
        {
            // The handle's generation snapshot predates a despawn of its index.
            return Main.Contains(entity)
                ? $"Entity {entity} was already despawned — its index now exists as a new generation: {_entityPool.EntityFor(entity.Index)}."
                : $"Entity {entity} was already despawned.";
        }

        if (entity.Generation > current) return $"Entity {entity} has a generation that does not exist yet (forged or corrupted handle? index {entity.Index} is at generation {current}).";

        return $"Entity {entity} is not alive.";
    }

    #endregion

}
