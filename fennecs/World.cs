// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    #region State & Storage
    private readonly IdentityPool _identityPool;

    private Meta[] _meta;
    private readonly List<Archetype> _archetypes = [];

    // "Identity" Archetype; all living Entities. (TODO: maybe change into publicly accessible "all" Query)
    private readonly Archetype _root;

    private readonly Dictionary<int, Query> _queries = new();

    // The new Type Graph that replaces the old Table Edge system.
    private readonly Dictionary<Signature<TypeExpression>, Archetype> _typeGraph = new();

    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Identity, HashSet<TypeExpression>> _typesByRelationTarget = new();
    #endregion


    #region Locking & Deferred Operations
    private readonly object _spawnLock = new();

    private readonly object _modeChangeLock = new();
    private int _locks;

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
            
            // Fixme: Cleanup!
            _root.IdentityStorage.Append(identity);
            var row = _root.Count - 1;

            while (_meta.Length <= _identityPool.Created) Array.Resize(ref _meta, _meta.Length * 2);

            _meta[identity.Index] = new Meta(identity, _root, row);

            var entityStorage = (Storage<Identity>) _root.Storages.First();
            entityStorage.Append(identity);

            return identity;
        }
    }


    private bool HasComponent(Identity identity, TypeExpression typeExpression)
    {
        var meta = _meta[identity.Index];
        return meta.Identity != Match.Plain
               && meta.Identity == identity
               && typeExpression.Matches(meta.Archetype.Signature);
    }


    private void DespawnImpl(Identity identity)
    {
        lock (_spawnLock)
        {
            AssertAlive(identity);

            if (Mode == WorldMode.Deferred)
            {
                _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Despawn, Identity = identity});
                return;
            }

            ref var meta = ref _meta[identity.Index];

            var table = meta.Archetype;
            table.Remove(meta.Row);
            meta.Clear();

            _identityPool.Recycle(identity);

            // Find identity-identity relation reverse lookup (if applicable)
            if (!_typesByRelationTarget.TryGetValue(identity, out var list)) return;

            //Remove Components from all Entities that had a relation
            foreach (var type in list)
            {
                var tablesWithType = _tablesByType[type];

                //TODO: There should be a bulk remove method instead.
                //TODO: Operation of this more efficient method could be:
                // 1. find each table that matches the type, and the table without the removed component
                // 2. determine signature of target type (with the removed component)
                // 3. migrate to the new archetype
                // 4. dispose or compact the old archetype (it is practically impossible that it will re-emerge) 
                foreach (var tableWithType in tablesWithType)
                    for (var i = tableWithType.Count - 1; i >= 0; i--)
                        RemoveComponent(tableWithType.Identities[i], type);
            }
        }
    }
    #endregion


    #region Queries
    internal Query GetQuery(List<TypeExpression> streamTypes, Mask mask, Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> createQuery)
    {
        if (_queries.TryGetValue(mask.GetHashCode(), out var query))
        {
            MaskPool.Return(mask);
            return query;
        }

        var type = mask.HasTypes[index: 0];
        if (!_tablesByType.TryGetValue(type, out var typeTables))
        {
            typeTables = new List<Archetype>(capacity: 16);
            _tablesByType[type] = typeTables;
        }

        var matchingTables = PooledList<Archetype>.Rent();
        foreach (var table in _archetypes)
            if (table.Matches(mask))
                matchingTables.Add(table);

        query = createQuery(this, streamTypes, mask, matchingTables);

        _queries.Add(query.GetHashCode(), query);
        return query;
    }


    internal void RemoveQuery(Query query)
    {
        _queries.Remove(query.GetHashCode());
    }


    internal ref Meta GetEntityMeta(Identity identity) => ref _meta[identity.Index];


    private Archetype GetArchetype(Signature<TypeExpression> types)
    {
        if (_typeGraph.TryGetValue(types, out var table)) return table;

        table = new Archetype(this, types);
        _archetypes.Add(table);
        _typeGraph.Add(types, table);

        // TODO: This is a suboptimal lookup (enumerate dictionary)
        // IDEA: Maybe we can keep Queries in a Tree which
        // identifies them just by their Signature root. (?) 
        foreach (var query in _queries.Values)
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
                tableList = [];
                _tablesByType[type] = tableList;
            }

            tableList.Add(table);

            if (!type.isRelation) continue;

            if (!_typesByRelationTarget.TryGetValue(type.Target, out var typeList))
            {
                typeList = [];
                _typesByRelationTarget[type.Target] = typeList;
            }

            typeList.Add(type);
        }

        return table;
    }


    internal void CollectTargets<T>(List<Identity> entities)
    {
        var type = TypeExpression.Of<T>(Match.Any);

        // Iterate through tables and get all concrete Entities from their Archetype TypeExpressions
        foreach (var candidate in _tablesByType.Keys)
            if (type.Matches(candidate))
                entities.Add(candidate.Target);
    }
    #endregion


    #region Assert Helpers
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AssertAlive(Identity identity)
    {
        if (IsAlive(identity)) return;

        throw new ObjectDisposedException($"Identity {identity} is no longer alive.");
    }
    #endregion
}