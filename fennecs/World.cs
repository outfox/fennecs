// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    #region World Bits
    internal static uint Mask;  
    internal static int Shift;

    private static readonly Lock Init = new();
    
    private static int _bits;
    
    /// <summary>
    /// How many Bits to reserve in Entity IDs for the World Index.
    /// <ul>The more bits you reserve ...
    /// <li>... the more Worlds can be created</li>
    /// <li>... the fewer Entities may exist in each of them</li>
    /// </ul>
    /// </summary>
    /// <example>
    /// <c>World.Bits = 1</c> (min) up to 2 Worlds and 2^30 (1 Gi) Entities per World<br/>
    /// <c>World.Bits = 7</c> (default) up to 128 Worlds and 2^24 (16 Mi) Entities per World<br/>
    /// <c>World.Bits = 15</c> (max) up to 32768 Worlds but only 2^16 (64 Ki) Entities per World<br/>
    /// </example>
    /// <remarks>
    /// Out of the 32 bits that form an Entity's Identity, the most significant bit is reserved to discern valid
    /// Identities from default values. Since the maximum (fast) collection size in .NET is capped at
    /// 1Gi = 1,073,741,824, this bit does not go to waste. 🦊
    /// </remarks>

    /// <exception cref="ArgumentOutOfRangeException">if the value is not between 1 and 15, inclusive </exception>
    /// <exception cref="InvalidOperationException">if any worlds already exist</exception>
    public static int Bits
    {
        get => _bits;
        set
        {
            lock (Init)
            {
                if (value is < 1 or > 15)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "WorldBits should be less than 16");
                }

                if (_worlds.Any(w => w != null!))
                    throw new InvalidOperationException("Cannot change WorldBits after Worlds have been created.");
            
                _bits = value;
            
                Shift = 32 - value;
                Mask = 0xFFFF_FFFF << Shift;

                _worlds = new World[MaxWorlds];
                
                WorldIds.Clear();
                for (var i = 0; i < MaxWorlds; i++)
                {
                    WorldIds.Enqueue(new(i));
                }
            }
        }
    }
    
    /// <summary>
    /// The maximum supported number of worlds (derived from the value set in <see cref="Bits"/>)
    /// </summary>
    public static int MaxWorlds => 1 << Bits;
    
    #endregion
    
    #region World State & Storage

    private readonly IdentityPool _identityPool;

    private Meta[] _meta;
    private uint[] _gen;

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

    private Entity.Snapshot NewEntity()
    {
        lock (_spawnLock)
        {
            var identity = _identityPool.Spawn();

            // FIXME: Cleanup / Unify! (not pretty to directly interact with the internals here)
            var capacity = (int) BitOperations.RoundUpToPowerOf2(_identityPool.Created + 1);
            Array.Resize(ref _meta, capacity);
            Array.Resize(ref _gen, capacity);

            var index = identity.Index;
            _meta[index] = new(_root, _root.Count);
            _root.EntityStorage.Append(identity);
            _root.Invalidate();

            return new(identity, _gen[index]);
        }
    }

    internal PooledList<fennecs.Id> SpawnBare(int count)
    {
        lock (_spawnLock)
        {
            var identities = _identityPool.Spawn(count);
            var capacity = (int) BitOperations.RoundUpToPowerOf2(_identityPool.Created + 1);
            Array.Resize(ref _meta, capacity);
            Array.Resize(ref _gen, capacity);
            return identities;
        }
    }


    internal bool HasComponent(Entity entity, TypeExpression typeExpression)
    {
        return entity.Alive && entity.Archetype.Has(typeExpression);
    }


    internal bool HasComponent(Entity entity, MatchExpression expression)
    {
        return entity.Alive && entity.Archetype.Matches(expression);
    }
    

    private void DespawnImpl(Entity entity)
    {
        AssertAlive(entity);

        if (Mode == WorldMode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation { Opcode = Opcode.Despawn, Entity = entity });
            return;
        }

        lock (_spawnLock)
        {
            var index = entity.Index;
            ref var meta = ref _meta[index];

            var table = meta.Archetype;
            table.Delete(meta.Row);

            DespawnDependencies(entity);

            // Clear Meta
            _meta[index] = default;

            _gen[index]++;
            _identityPool.Recycle(entity.Id);
        }
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


    internal ref Meta GetEntityMeta(Entity.Id id)
    {
        return ref _meta[id.Index];
    }
    
    internal ref Meta GetMeta(Entity.Id id) => ref _meta[id.Index];

    internal ref Meta GetEntityMeta(Entity entity) => ref _meta[entity.Index];

    internal ref uint GetGeneration(Entity.Id entity) => ref _gen[entity.Index];


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
    //TODO: We should do this in Entity instead, and clean up World code a lot?
    private void AssertAlive(Entity entity)
    {
        if (entity.Alive) return;
        throw new ObjectDisposedException($"Entity {entity} is not alive in World {Name}.");
    }

    #endregion

    
    /// <summary>
    /// A unique ID for a World.
    /// </summary>
    internal readonly record struct Id(int Index)
    {
        // The bit pattern representing this world; the top bit is used to represent this being a valid world.
        internal readonly uint Tag = (uint) (Index << Shift) | ValidFlag;

        /// <inheritdoc />
        public override string ToString() => $"w{Index:d3}"; 

        private const uint ValidFlag = 0b_1000_0000_0000_0000_0000_0000_0000_0000u;
    }
}
