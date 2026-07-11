// SPDX-License-Identifier: MIT

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


    private void Unlock()
    {
        if (Interlocked.Decrement(ref _locks) > 0) return;

        Mode = WorldMode.CatchUp;
        CatchUp(_deferredOperations);
        Mode = WorldMode.Immediate;
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

    internal PooledList<Entity> SpawnBare(int count)
    {
        var entities = _entityPool.Spawn(count);
        Main.EnsureCapacity(_entityPool.Created + 1);
        return entities;
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
            _deferredOperations.Enqueue(new DeferredOperation { Opcode = Opcode.Despawn, Entity = entity });
            return;
        }

        // Remove the Entity from every Aspect that contains it, and clean up
        // Relations targeting it wherever they live. (Main always contains it)
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

        throw new ObjectDisposedException($"Entity {entity} is no longer alive.");
    }

    #endregion

}
