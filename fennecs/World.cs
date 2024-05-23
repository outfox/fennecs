// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
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

            // FIXME: Cleanup / Unify! (not pretty to directly interact with the internals here)
            while (_meta.Length <= _identityPool.Created) Array.Resize(ref _meta, _meta.Length * 2);

            _meta[identity.Index] = Meta.Empty();
            
            _root.IdentityStorage.Append(identity);
            
            var row = _root.Count - 1;
            _root.PatchMetas(row);
            _root.Invalidate();   
            
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
            AssertAlive(identity);

            if (Mode == WorldMode.Deferred)
            {
                _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Despawn, Identity = identity});
                return;
            }

            ref var meta = ref _meta[identity.Index];

            var table = meta.Archetype;
            table.Delete(meta.Row);

            _identityPool.Recycle(identity);

            DespawnDependencies(identity);
    }

    
    private void DespawnDependencies(Identity identity)
    {
            // Patch Meta
            _meta[identity.Index] = Meta.Empty();

            // Find identity-identity relation reverse lookup (if applicable)
            if (!_typesByRelationTarget.Remove(identity, out var list)) return;

            //Remove Components from all Entities that had a relation
            foreach (var type in list) //TODO: Benchmark sorted and reversed hashsets here.
            {
                //Cloen the list.
                var tablesWithType = new List<Archetype>(_tablesByType[type]);

                foreach (var source in tablesWithType)
                {
                    var signatureWithoutTarget = new Signature<TypeExpression>(source.Signature.Where(t => t.Target != identity).ToImmutableSortedSet());
                    var destination = GetArchetype(signatureWithoutTarget);
                    source.Migrate(destination);
                    
                    //Because the dependency is now gone, we close down the whole archetype.
                    ForgetArchetype(source);
                }
            }
    }
    #endregion


    #region Queries

    internal Query CompileQuery(List<TypeExpression> streamTypes, Mask mask, Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> createQuery)
    {
        var type = mask.HasTypes[index: 0];
        if (!_tablesByType.TryGetValue(type, out var typeTables))
        {
            typeTables = new List<Archetype>(capacity: 16);
            _tablesByType[type] = typeTables;
        }

        var matchingTables = PooledList<Archetype>.Rent();
        foreach (var table in _archetypes)
        {
            if (table.Matches(mask)) matchingTables.Add(table);
        }

        var query = createQuery(this, streamTypes, mask, matchingTables);
        return query;
    }
    
    internal Query CacheQuery(List<TypeExpression> streamTypes, Mask mask, Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> createQuery)
    {
        // Compile if not cached.
        if (!_queries.TryGetValue(mask.GetHashCode(), out var query))
        {
            query = CompileQuery(streamTypes, mask, createQuery);
            _queries.Add(query.GetHashCode(), query);
            return query;
        }
        
        MaskPool.Return(mask);
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