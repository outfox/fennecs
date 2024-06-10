// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World : Query
{
    #region World State & Storage
    private readonly IdentityPool _identityPool;

    private Meta[] _meta;

    // "Identity" Archetype; all living Entities. (TODO: maybe change into publicly accessible "all" Query)
    private readonly Archetype _root;

    private readonly HashSet<Query> _queries = [];
    private readonly Dictionary<int, Query> _queryCache = new();

    // The new Type Graph that replaces the old Table Edge system.
    private readonly Dictionary<Signature, Archetype> _typeGraph = new();

    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Relate, HashSet<TypeExpression>> _typesByRelationTarget = new();
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

            // FIXME: Cleanup / Unify! (not pretty to directly interact with the internals here)
            Array.Resize(ref _meta, (int) BitOperations.RoundUpToPowerOf2((uint)(_identityPool.Created + 1)));

            _meta[identity.Index] = new(_root, _root.Count, identity);
            _root.IdentityStorage.Append(identity);
            _root.Invalidate();   
            
            return identity;
        }
    }


    private bool HasComponent(Identity identity, TypeExpression typeExpression)
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
                _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Despawn, Identity = entity});
                return;
            }

            DespawnDependencies(entity);

            ref var meta = ref _meta[entity.Id.Index];

            var table = meta.Archetype;
            table.Delete(meta.Row);

            _identityPool.Recycle(entity);

            // Patch Meta
            _meta[entity.Id.Index] = default;
    }


    private void DespawnDependencies(Entity entity)
    {
        // Find identity-identity relation reverse lookup (if applicable)
        if (!_typesByRelationTarget.TryGetValue(Relate.To(entity), out var types) 
            || types.Count == 0) return;

        // Collect Archetypes that have any of these relations
        var toMigrate = Archetypes.Where(a => a.Signature.Matches(types)).ToList();

        // And migrate them to a new Archetype without the relation
        foreach (var archetype in toMigrate)
        {
            if (archetype.Count > 0)
            {
                var signatureWithoutTarget = archetype.Signature.Except(types);
                var destination = GetArchetype(signatureWithoutTarget);
                archetype.Migrate(destination);
            }
            DisposeArchetype(archetype);
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

        var matchingTables = PooledList<Archetype>.Rent();
        matchingTables.AddRange(Archetypes.Where(table => table.Matches(mask)));

        query = new(this, mask.Clone(), matchingTables);
        if (!_queries.Add(query) || !_queryCache.TryAdd(query.Mask.GetHashCode(), query))
        {
            //TODO: Remove this check :)
            throw new InvalidOperationException("Query was already added to World. File a bug report!");
        }
        return query;
    }


    internal void RemoveQuery(Query query)
    {
        if (!_queries.Remove(query))
        {
            throw new InvalidOperationException("Query was not found in World.");
        }
        
        _queryCache.Remove(query.Mask.GetHashCode());
    }


    internal ref Meta GetEntityMeta(Identity identity) => ref _meta[identity.Index];


    private Archetype GetArchetype(Signature types)
    {
        if (_typeGraph.TryGetValue(types, out var table)) return table;

        table = new(this, types);

        //This could be given to us by the next query update?
        Archetypes.Add(table);

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


    internal void CollectTargets<T>(List<Relate> entities)
    {
        var type = TypeExpression.Of<T>(Match.Entity);

        // Modern LINQ version.
        entities.AddRange(
            from candidate in _tablesByType.Keys 
            where type.Matches(candidate) 
            select candidate.Relation);
        
        // Iterate through tables and get all concrete Entities from their Archetype TypeExpressions
        /*
        foreach (var candidate in _tablesByType.Keys)
        {
            if (type.Matches(candidate))
            {
                entities.Add(candidate.Relation);
            }
        }
        */
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