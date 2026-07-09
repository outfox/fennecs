// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    #region World State & Storage

    private readonly IdentityPool _identityPool;

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

    private Identity NewEntity()
    {
        var identity = _identityPool.Spawn();

        Main.EnsureCapacity(_identityPool.Created + 1);
        Main.Join(identity);

        return identity;
    }

    internal PooledList<Identity> SpawnBare(int count)
    {
        var identities = _identityPool.Spawn(count);
        Main.EnsureCapacity(_identityPool.Created + 1);
        return identities;
    }


    internal bool HasComponent(Identity identity, TypeExpression typeExpression)
    {
        return AspectOf(typeExpression).HasComponent(identity, typeExpression);
    }


    private void DespawnImpl(Entity entity)
    {
        AssertAlive(entity);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation { Opcode = Opcode.Despawn, Identity = entity });
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

        _identityPool.Recycle(entity);
    }

    #endregion


    #region Queries

    internal Query CompileQuery(Mask mask) => ResolveAspect(mask).CompileQuery(mask);


    internal ref Meta GetEntityMeta(Identity identity) => ref Main.GetEntityMeta(identity);


    internal IReadOnlyList<Component> GetComponents(Identity id)
    {
        if (_aspects.Count == 1) return Main.GetComponents(id);

        var components = new List<Component>(Main.GetComponents(id));
        for (var i = 1; i < _aspects.Count; i++)
        {
            components.AddRange(_aspects[i].GetComponents(id));
        }
        return components;
    }

    /// <inheritdoc />
    public override int GetHashCode() => _guid.GetHashCode();

    #endregion


    #region Assert Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertAlive(Identity identity)
    {
        if (IsAlive(identity)) return;

        throw new ObjectDisposedException($"Identity {identity} is no longer alive.");
    }

    #endregion

}
