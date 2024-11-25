// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    #region World State & Storage

    private readonly IdentityPool _identityPool;

    private Meta[] _meta;

    private readonly Guid _guid = Guid.NewGuid();

    // "Identity" Archetype; all living Entities. (TODO: maybe change into publicly accessible "all" Query)
    private readonly Archetype _root;
    private readonly List<Archetype> _archetypes = [];
    
    private readonly HashSet<Query> _queries = [];
    private readonly Dictionary<int, Query> _queryCache = new();
    

    // The new Type Graph that replaces the old Table Edge system.
    private readonly Dictionary<Signature, Archetype> _typeGraph = new();

    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Relate, HashSet<TypeExpression>> _typesByRelationTarget = new();

    #endregion


    #region Locking & Deferred Operations

    private readonly Lock _spawnLock = new();

    private readonly Lock _modeChangeLock = new();
    private int _locks;

    internal static int Concurrency => Math.Max(1, Environment.ProcessorCount-2);
    
    internal WorldMode Mode { get; private set; } = WorldMode.Immediate;


    private void Unlock()
    {
        lock (_modeChangeLock)
        {
            if (--_locks != 0) return;

            Mode = WorldMode.CatchUp;
            CatchUp(_deferredOperations);
            Mode = WorldMode.Immediate;
        }
    }


    internal enum WorldMode
    {
        Immediate = default,
        CatchUp,
        Deferred,
    }

    #endregion


    #region CRUD

    private Identity NewEntity()
    {
        lock (_spawnLock)
        {
            var identity = _identityPool.Spawn();

            // FIXME: Cleanup / Unify! (not pretty to directly interact with the internals here)
            Array.Resize(ref _meta, (int)BitOperations.RoundUpToPowerOf2((uint)(_identityPool.Created + 1)));

            _meta[identity.Index] = new(_root, _root.Count, identity);
            _root.IdentityStorage.Append(identity);
            _root.Invalidate();

            return identity;
        }
    }

    internal PooledList<Identity> SpawnBare(int count)
    {
        lock (_spawnLock)
        {
            var identities = _identityPool.Spawn(count);
            Array.Resize(ref _meta, (int)BitOperations.RoundUpToPowerOf2((uint)_identityPool.Created + 1));
            return identities;
        }
    }


    internal bool HasComponent(Identity identity, TypeExpression typeExpression)
    {
        var meta = _meta[identity.Index];
        return meta.Identity != default
               && meta.Identity == identity
               && typeExpression.Matches(meta.Archetype.MatchSignature);
    }


    private void DespawnImpl(Entity entity)
    {
        AssertAlive(entity);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation { Opcode = Opcode.Despawn, Identity = entity });
            return;
        }

        ref var meta = ref _meta[entity.Id.Index];

        var table = meta.Archetype;
        table.Delete(meta.Row);

        DespawnDependencies(entity);

        _identityPool.Recycle(entity);

        // Patch Meta
        _meta[entity.Id.Index] = default;
    }


    private void DespawnDependencies(Entity entity)
    {
        // Find identity-identity relation reverse lookup (if applicable)
        if (!_typesByRelationTarget.TryGetValue(Relate.To(entity), out var types)) return;

        // Collect Archetypes that have any of these relations
        var toMigrate = _archetypes.Where(a => a.Signature.Matches(types)).ToList();

        // Do not change the home archetype of the entity (relating to entities having a relation with themselves)
        var homeArchetype = _meta[entity.Id.Index].Archetype;

        // And migrate them to a new Archetype without the relation
        foreach (var archetype in toMigrate)
        {
            if (archetype == homeArchetype) continue;

            if (archetype.Count > 0)
            {
                var signatureWithoutTarget = archetype.Signature.Except(types);
                var destination = GetArchetype(signatureWithoutTarget);
                archetype.Migrate(destination);
            }
        }

        // No longer tracking this Entity
        _typesByRelationTarget.Remove(Relate.To(entity));
    }

    #endregion


    #region Queries

    internal Query CompileQuery(Mask mask)
    {
        // Return cached query if available.
        if (_queryCache.TryGetValue(mask.GetHashCode(), out var query)) return query;

        // Create a new query and cache it.
        var matchingTables = new HashSet<Archetype>(_archetypes.Where(table => table.Matches(mask)));
        query = new(this, mask.Clone(), matchingTables);
        _queries.Add(query);
        _queryCache.Add(query.Mask.GetHashCode(), query);
        return query;
    }


    internal void RemoveQuery(Query query)
    {
        _queries.Remove(query);
        _queryCache.Remove(query.Mask.GetHashCode());
    }


    internal ref Meta GetEntityMeta(Identity identity) => ref _meta[identity.Index];


    private Archetype GetArchetype(Signature types)
    {
        if (_typeGraph.TryGetValue(types, out var table)) return table;

        table = new(this, types);

        //This could be given to us by the next query update?
        _archetypes.Add(table);

        // TODO: This is a suboptimal lookup (enumerate dictionary)
        // IDEA: Maybe we can keep Queries in a Tree which
        // identifies them just by their Signature root. (?) 
        foreach (var query in _queries)
        {
            if (table.Matches(query.Mask))
            {
                query.TrackArchetype(table);
            }
        }

        foreach (var type in types)
        {
            if (!_tablesByType.TryGetValue(type, out var tableList))
            {
                tableList = new(capacity: 16);
                _tablesByType[type] = tableList;
            }

            tableList.Add(table);

            if (!type.isRelation) continue;

            if (!_typesByRelationTarget.TryGetValue(type.Relation, out var typeSet))
            {
                typeSet = [];
                _typesByRelationTarget[type.Relation] = typeSet;
            }

            typeSet.Add(type);
        }

        _typeGraph.Add(types, table);
        return table;
    }

    internal IReadOnlyList<Component> GetComponents(Identity id)
    {
        var archetype = _meta[id.Index].Archetype;
        return archetype.GetRow(_meta[id.Index].Row);
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
