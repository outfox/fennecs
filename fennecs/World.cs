// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    
    #region World State & Storage

    private readonly EntityPool _entityPool;

    private Meta[] _meta;

    private readonly Guid _guid = Guid.NewGuid();

    // "Entity" Archetype; all living Entities. (TODO: maybe change into publicly accessible "all" Query)
    private readonly Archetype _root;
    private readonly List<Archetype> _archetypes = [];
    
    private readonly HashSet<Query> _queries = [];
    private readonly Dictionary<int, Query> _queryCache = new();
    

    // The new Type Graph that replaces the old Table Edge system.
    private readonly Dictionary<Signature, Archetype> _typeGraph = new();

    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Key, HashSet<TypeExpression>> _typesByRelationTarget = new();

    #endregion


    #region Locking & Deferred Operations

    private readonly Lock _spawnLock = new();

    private readonly Lock _modeChangeLock = new();
    private int _locks;

    internal static int Concurrency => Math.Max(1, Environment.ProcessorCount-2);

    internal uint Discriminator(Entity entity) => _entityPool.Discriminator(entity);
    
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

    private Entity NewEntity()
    {
        lock (_spawnLock)
        {
            var entity = _entityPool.Spawn();

            // FIXME: Cleanup / Unify! (not pretty to directly interact with the internals here)
            Array.Resize(ref _meta, (int)BitOperations.RoundUpToPowerOf2((uint)(_entityPool.Created + 1)));

            _meta[entity.Index] = new(_root, _root.Count, entity);
            _root.EntityStorage.Append(entity);
            _root.Invalidate();

            return entity;
        }
    }

    internal PooledList<Entity> SpawnBare(int count)
    {
        lock (_spawnLock)
        {
            var identities = _entityPool.Spawn(count);
            Array.Resize(ref _meta, (int)BitOperations.RoundUpToPowerOf2((uint)_entityPool.Created + 1));
            return identities;
        }
    }


    internal bool HasComponent(Entity entity, TypeExpression typeExpression)
    {
        var meta = _meta[entity.Index];
        return meta.Entity != default
               && meta.Entity == entity
               && meta.Archetype.Has(typeExpression);
    }


    internal bool HasComponent(Entity entity, MatchExpression expression)
    {
        var meta = _meta[entity.Index];
        return meta.Entity != default
               && meta.Entity == entity
               && meta.Archetype.Matches(expression);
    }
    

    private void DespawnImpl(Entity entity)
    {
        AssertAlive(entity);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation { Opcode = Opcode.Despawn, Entity = entity });
            return;
        }

        ref var meta = ref _meta[entity.Index];

        var table = meta.Archetype;
        table.Delete(meta.Row);

        DespawnDependencies(entity);

        _entityPool.Recycle(entity);

        // Patch Meta
        _meta[entity.Index] = default;
    }


    private void DespawnDependencies(Entity entity)
    {
        // Find entity-entity relation reverse lookup (if applicable)
        if (!_typesByRelationTarget.TryGetValue(entity.Key, out var types)) return;

        // Collect Archetypes that have any of these relations
        var toMigrate = _archetypes.Where(a => a.Signature.Overlaps(types)).ToList();

        // Do not change the home archetype of the entity (relating to entities having a relation with themselves)
        var homeArchetype = _meta[entity.Index].Archetype;

        // And migrate them to a new Archetype without the relation
        foreach (var archetype in toMigrate)
        {
            if (archetype == homeArchetype) continue;

            if (archetype.Count > 0)
            {
                var signatureWithoutTarget = archetype.Signature.Except(types);
                var destination = GetOrCreateArchetype(signatureWithoutTarget);
                archetype.Migrate(destination);
            }
        }

        // No longer tracking this Entity
        _typesByRelationTarget.Remove(entity.Key);
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


    internal ref Meta GetEntityMeta(Entity entity)
    {
        AssertAlive(entity);
        return ref _meta[entity.Index];
    }


    private Archetype GetOrCreateArchetype(Signature types)
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

            if (!type.IsRelation) continue;

            if (!_typesByRelationTarget.TryGetValue(type.Key, out var typeSet))
            {
                typeSet = [];
                _typesByRelationTarget[type.Key] = typeSet;
            }

            typeSet.Add(type);
        }

        _typeGraph.Add(types, table);
        return table;
    }

    /// <inheritdoc />
    public override int GetHashCode() => _guid.GetHashCode();

    #endregion


    #region Assert Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertAlive(Entity entity)
    {
        if (IsAlive(entity)) return;
        throw new ObjectDisposedException($"Entity {entity} is not alive in World {Name}.");
    }

    #endregion

    /// <summary>
    /// A unique ID for a World.
    /// </summary>
    public readonly record struct Id
    {
        private readonly byte _id;

        internal Id(byte id)
        {
            //ArgumentOutOfRangeException.ThrowIfEqual(id, 0, $"{typeof(Id).FullName} must be between 1 and {byte.MaxValue}");
            Bits = (ulong) id << 32;
            _id = id;
        }

        internal Id(int id)
        {
            //Debug.Assert(id is > 0 and <= byte.MaxValue, $"{typeof(Id).FullName} must be between 1 and {byte.MaxValue}");
            Bits = (ulong) id << 32;
            _id = (byte) id;
        }

        /// <summary>
        /// Casts a byte to an Id.
        /// </summary>
        public static implicit operator Id(byte id) => new(id);
        
        internal int Index => _id;
        
        internal readonly ulong Bits;
        
        /// <inheritdoc />
        public override string ToString() => $"{_id:d3}";
    }
}
