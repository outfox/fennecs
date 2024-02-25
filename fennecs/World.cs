// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World
{
    #region State & Storage

    private readonly IdentityPool _identityPool;

    private Meta[] _meta;
    private readonly List<Archetype> _archetypes = [];
    private readonly Archetype _root; // "Identity" Archetype; all living Entities.

    private readonly Dictionary<int, Query> _queries = new();

    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();

    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Identity, HashSet<TypeExpression>> _typesByRelationTarget = new();

    #endregion


    #region Locking & Deferred Operations

    private readonly object _spawnLock = new();

    private readonly object _modeChangeLock = new();
    private Mode _mode = Mode.Immediate;
    private int _locks;


    public struct WorldLock : IDisposable
    {
        private World _world;


        public WorldLock(World world)
        {
            lock (world._modeChangeLock)
            {
                _world = world;
                _world._mode = Mode.Deferred;
                _world._locks++;
            }
        }


        public void Dispose()
        {
            _world.Unlock();
            _world = null!;
        }
    }


    private void Unlock()
    {
        lock (_modeChangeLock)
        {
            if (--_locks == 0)
            {
                _mode = Mode.CatchUp;
                Apply(_deferredOperations);
                _mode = Mode.Immediate;
            }
        }
    }


    private void Apply(ConcurrentQueue<DeferredOperation> operations)
    {
        while (operations.TryDequeue(out var op))
        {
            AssertAlive(op.Identity);

            switch (op.Opcode)
            {
                case Opcode.Add:
                    AddComponent(op.Identity, op.TypeExpression, op.Data);
                    break;
                case Opcode.Remove:
                    RemoveComponent(op.Identity, op.TypeExpression);
                    break;
                case Opcode.Despawn:
                    Despawn(op.Identity);
                    break;
            }
        }
    }


    internal struct DeferredOperation
    {
        internal required Opcode Opcode;
        internal TypeExpression TypeExpression;
        internal Identity Identity;
        internal object Data;
    }


    internal enum Opcode
    {
        Add,
        Remove,
        Despawn,
    }


    private enum Mode
    {
        Immediate = default,
        CatchUp,
        Deferred,
        //Bulk
    }

    #endregion


    #region CRUD

    private Identity NewEntity()
    {
        lock (_spawnLock)
        {
            var identity = _identityPool.Spawn();

            var row = _root.Add(identity);

            while (_meta.Length <= _identityPool.Living) Array.Resize(ref _meta, _meta.Length * 2);

            _meta[identity.Index] = new Meta(identity, _root, row);

            var entityStorage = (Identity[]) _root.Storages.First();
            entityStorage[row] = identity;

            return identity;
        }
    }


    private bool HasComponent(Identity identity, TypeExpression typeExpression)
    {
        var meta = _meta[identity.Index];
        return meta.Identity != Match.Plain
               && meta.Identity == identity
               && typeExpression.Matches(meta.Archetype.Types);
    }


    private void Despawn(Identity identity)
    {
        lock (_spawnLock)
        {
            AssertAlive(identity);

            if (_mode == Mode.Deferred)
            {
                _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Despawn, Identity = identity});
                return;
            }

            ref var meta = ref _meta[identity.Index];

            var table = meta.Archetype;
            table.Remove(meta.Row);
            meta.Clear();

            _identityPool.Despawn(identity);

            // Find identity-identity relation reverse lookup (if applicable)
            if (!_typesByRelationTarget.TryGetValue(identity, out var list)) return;

            //Remove Components from all Entities that had a relation
            foreach (var type in list)
            {
                var tablesWithType = _tablesByType[type];

                //TODO: There should be a bulk remove method instead.
                foreach (var tableWithType in tablesWithType)
                {
                    for (var i = tableWithType.Count - 1; i >= 0; i--)
                    {
                        RemoveComponent(tableWithType.Identities[i], type);
                    }
                }
            }
        }
    }


    private void RemoveComponent(Identity identity, TypeExpression typeExpression)
    {
        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Remove, Identity = identity, TypeExpression = typeExpression});
            return;
        }

        ref var meta = ref _meta[identity.Index];
        var oldTable = meta.Archetype;

        if (!oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"cannot remove non-existent component {typeExpression} from identity {identity}");
        }

        var oldEdge = oldTable.GetTableEdge(typeExpression);

        var newTable = oldEdge.Remove;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.Remove(typeExpression);
            newTable = AddTable(newTypes);
            oldEdge.Remove = newTable;

            var newEdge = newTable.GetTableEdge(typeExpression);
            newEdge.Add = oldTable;
        }

        var newRow = Archetype.MoveEntry(identity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.Archetype = newTable;
    }

    #endregion


    #region Queries

    internal Query GetQuery(List<TypeExpression> streamTypes, Mask mask, Func<World, List<TypeExpression>, Mask, List<Archetype>, Query> createQuery)
    {
        if (_queries.TryGetValue(mask, out var query))
        {
            MaskPool.Return(mask);
            return query;
        }

        var type = mask.HasTypes[0];
        if (!_tablesByType.TryGetValue(type, out var typeTables))
        {
            typeTables = new(16);
            _tablesByType[type] = typeTables;
        }

        var matchingTables = PooledList<Archetype>.Rent();
        foreach (var table in _archetypes)
        {
            if (table.Matches(mask)) matchingTables.Add(table);
        }

        query = createQuery(this, streamTypes, mask, matchingTables);

        _queries.Add(mask, query);
        return query;
    }


    internal void RemoveQuery(Query query)
    {
        _queries.Remove(query.Mask);
    }


    internal ref Meta GetEntityMeta(Identity identity)
    {
        return ref _meta[identity.Index];
    }


    internal IEnumerable<TypeExpression> GetComponents(Identity identity)
    {
        AssertAlive(identity);
        var meta = _meta[identity.Index];
        var array = meta.Archetype.Types;
        return array;
    }


    private Archetype AddTable(ImmutableSortedSet<TypeExpression> types)
    {
        var table = new Archetype(this, types);
        _archetypes.Add(table);

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

        foreach (var query in _queries.Values.Where(query => table.Matches(query.Mask)))
        {
            query.AddTable(table);
        }

        return table;
    }


    internal void CollectTargets<T>(List<Identity> entities)
    {
        var type = TypeExpression.Create<T>(Match.Any);

        // Iterate through tables and get all concrete Entities from their Archetype TypeExpressions
        foreach (var candidate in _tablesByType.Keys)
        {
            if (type.Matches(candidate)) entities.Add(candidate.Target);
        }
    }

    #endregion


    #region Assert Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertAlive(Identity identity)
    {
        if (IsAlive(identity)) return;

        throw new ObjectDisposedException($"Identity {identity} is no longer alive.");
    }

    #endregion


    #region Component Interaction

    /// <summary>
    /// Creates an Archetype relation between this identity and an object (instance of a class).
    /// The relation is backed by the object itself, which will be enumerated by queries if desired.
    /// Whenever the identity is enumerated by a Query, it will be batched only with other Entities
    /// that share the exact relation, in addition to conforming with the other clauses of the Query.
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between Entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few Entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    internal void AddLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Identity.Of(target));
        AddComponent(identity, typeExpression, target);
    }


    /// <summary>
    /// Checks if this identity has an object-backed relation (instance of a class).
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal bool HasLink<T>(Identity identity, [NotNull] T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Identity.Of(target));
        return HasComponent(identity, typeExpression);
    }


    /// <summary>
    /// Removes the object-backed relation between this identity and the object (instance of a class).
    /// The object is internally reference-counted, and the reference will be discarded once no other
    /// identity is linked to it.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    internal void RemoveLink<T>(Identity identity, T target) where T : class
    {
        var typeExpression = TypeExpression.Create<T>(Identity.Of(target));
        RemoveComponent(identity, typeExpression);
    }


    /// <summary>
    /// Creates an Archetype relation between this identity and another identity.
    /// The relation is backed by an arbitrary type to provide additional data.
    /// Whenever the identity is enumerated by a Query, it will be batched only with other Entities
    /// that share the exact relation, in addition to conforming with the other clauses of the Query.
    /// </summary>
    ///<remarks>
    /// Beware of Archetype Fragmentation!
    /// This feature is great to express relations between Entities, but it will lead to
    /// fragmentation of the Archetype Graph, i.e. Archetypes with very few Entities that
    /// are difficult to iterate over efficiently.
    /// </remarks>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <typeparam name="T">any Component type</typeparam>
    internal void AddRelation<T>(Identity identity, Identity target, T data)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        AddComponent(identity, typeExpression, data);
    }


    /// <summary>
    /// Checks if this identity has a relation Component with another identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any Component type</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal bool HasRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        return HasComponent(identity, typeExpression);
    }


    /// <summary>
    /// Removes the relation Component between this identity and another identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="target"></param>
    /// <typeparam name="T">any Component type</typeparam>
    internal void RemoveRelation<T>(Identity identity, Identity target)
    {
        var typeExpression = TypeExpression.Create<T>(target);
        RemoveComponent(identity, typeExpression);
    }


    internal void AddComponent<T>(Identity identity) where T : new()
    {
        var type = TypeExpression.Create<T>(Match.Plain);
        AddComponent(identity, type, new T());
    }


    internal void AddComponent<T>(Identity identity, T data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var type = TypeExpression.Create<T>();
        AddComponent(identity, type, data);
    }


    internal bool HasComponent<T>(Identity identity, Identity target = default)
    {
        var type = TypeExpression.Create<T>(target);
        return HasComponent(identity, type);
    }


    internal void RemoveComponent<T>(Identity identity)
    {
        var type = TypeExpression.Create<T>(Match.Plain);
        RemoveComponent(identity, type);
    }


    private void AddComponent<T>(Identity identity, TypeExpression typeExpression, T data)
    {
        AssertAlive(identity);

        ref var meta = ref _meta[identity.Index];
        var oldTable = meta.Archetype;

        if (oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"Identity {identity} already has component of type {typeExpression}");
        }

        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Opcode = Opcode.Add, Identity = identity, TypeExpression = typeExpression, Data = data!});
            return;
        }

        var oldEdge = oldTable.GetTableEdge(typeExpression);

        var newTable = oldEdge.Add;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.Add(typeExpression);
            newTable = AddTable(newTypes);
            oldEdge.Add = newTable;

            var newEdge = newTable.GetTableEdge(typeExpression);
            newEdge.Remove = oldTable;
        }

        var newRow = Archetype.MoveEntry(identity, meta.Row, oldTable, newTable);
        newTable.Set(typeExpression, data, newRow);

        meta.Row = newRow;
        meta.Archetype = newTable;
    }


    internal ref T GetComponent<T>(Identity identity, Identity target = default)
    {
        AssertAlive(identity);

        if (typeof(T) == typeof(Identity))
        {
            throw new TypeAccessException("Not allowed get mutable reference in root table (TypeExpression<Identity>, system integrity).");
        }

        var meta = _meta[identity.Index];
        var table = meta.Archetype;
        var storage = table.GetStorage<T>(target);
        return ref storage[meta.Row];
    }

    #endregion
}