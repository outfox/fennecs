// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace fennecs;

public sealed class Archetypes
{
    private EntityMeta[] _meta = new EntityMeta[65536];
    private readonly List<Table> _tables = [];
    private readonly Dictionary<int, Query> _queries = new();


    private readonly ConcurrentBag<Identity> _unusedIds = [];
    private Table entityRoot => _tables[0];

    internal int Count { get; private set; }

    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();
    private readonly Dictionary<TypeExpression, List<Table>> _tablesByType = new();
    
    private readonly Dictionary<Identity, HashSet<TypeExpression>> _typesByRelationTarget = new();
    private readonly Dictionary<ushort, HashSet<Entity>> _targetsByRelationType = new();
    private readonly Dictionary<int, HashSet<TypeExpression>> _relationsByTypes = new();

    private readonly object _modeChangeLock = new();
    private int _lockCount;

    private Mode _mode = Mode.Immediate;

    public Archetypes()
    {
        AddTable([TypeExpression.Create<Entity>(Identity.None)]);
    }

    public Entity GetTarget(TypeExpression type)
    {
        if (type is {isRelation: true, Target.IsEntity: true})
        {
            return type.Target;
        }

        throw new InvalidCastException($"TypeExpression {type} is not a Entity-Component-Entity relation type");
    }

    public void GetTargets<T>(HashSet<Entity> result)
    {
        var type = TypeExpression.Create<T>(Identity.Any);

        // Iterate through tables and get all concrete entities from all Archetype TypeExpressions
        foreach (var table in _tables) table.FindTargets(type, result);
    }

    private readonly object _spawnLock = new();

    public Entity Spawn(Type? type = default)
    {
        lock (_spawnLock)
        {
            if (!_unusedIds.TryTake(out var identity))
            {
                identity = new Identity(++Count);
            }
            
            var row = entityRoot.Add(identity);

            if (_meta.Length == Count) Array.Resize(ref _meta, Count * 2);

            _meta[identity.Id] = new EntityMeta(identity, entityRoot.Id, row);

            var entity = new Entity(identity);

            var entityStorage = (Entity[]) entityRoot.Storages[0];
            entityStorage[row] = entity;

            return entity;
        }
    }


    public void Despawn(Identity identity)
    {
        if (!IsAlive(identity)) return;

        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Operation = Deferred.Despawn, Identity = identity});
            return;
        }

        ref var meta = ref _meta[identity.Id];

        var table = _tables[meta.TableId];
        table.Remove(meta.Row);
        meta.Clear();

        _unusedIds.Add(identity.Successor);
        
        // Find entity-entity relation reverse lookup (if applicable)
        if (!_typesByRelationTarget.TryGetValue(identity, out var list)) return;

        //Remove components from all entities that had a relation
        foreach (var type in list)
        {
            _targetsByRelationType[type.TypeId].Remove(identity);

            var tablesWithType = _tablesByType[type];

            foreach (var tableWithType in tablesWithType)
            {
                //TODO: There should be a bulk remove method instead.
                for (var i = 0; i < tableWithType.Count; i++)
                {
                    RemoveComponent(type, tableWithType.Identities[i]);
                }
            }
        }
    }


    internal void AddComponent<T>(TypeExpression typeExpression, Identity identity, T data, Entity target = default)
    {
        AssertAlive(identity);

        ref var meta = ref _meta[identity.Id];
        var oldTable = _tables[meta.TableId];

        if (oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"Entity {identity} already has component of type {typeExpression}");
        }

        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Operation = Deferred.Add, Identity = identity, TypeExpression = typeExpression, Data = data!});
            return;
        }

        _targetsByRelationType.TryAdd(typeExpression.TypeId, []);
        _targetsByRelationType[typeExpression.TypeId].Add(identity);

        var oldEdge = oldTable.GetTableEdge(typeExpression);

        var newTable = oldEdge.Add;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.ToList();
            newTypes.Add(typeExpression);
            newTable = AddTable(new SortedSet<TypeExpression>(newTypes));
            oldEdge.Add = newTable;

            var newEdge = newTable.GetTableEdge(typeExpression);
            newEdge.Remove = oldTable;
        }

        var newRow = Table.MoveEntry(identity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.TableId = newTable.Id;

        var storage = newTable.GetStorage(typeExpression);
        storage.SetValue(data, newRow);
    }


    public ref T GetComponent<T>(Identity identity, Identity target = default)
    {
        AssertAlive(identity);

        var type = TypeExpression.Create<T>(target);
        var meta = _meta[identity.Id];
        AssertEqual(meta.Identity, identity);
        var table = _tables[meta.TableId];
        var storage = (T[]) table.GetStorage(type);
        return ref storage[meta.Row];
    }


    internal bool HasComponent(TypeExpression typeExpression, Identity identity)
    {
        var meta = _meta[identity.Id];
        return meta.Identity != Identity.None
               && meta.Identity == identity
               && _tables[meta.TableId].Types.Contains(typeExpression);
    }


    internal void RemoveComponent(TypeExpression typeExpression, Identity identity)
    {
        ref var meta = ref _meta[identity.Id];
        var oldTable = _tables[meta.TableId];

        if (!oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"cannot remove non-existent component {typeExpression} from entity {identity}");
        }

        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Operation = Deferred.Remove, Identity = identity, TypeExpression = typeExpression});
            return;
        }


        // could be _targetsByRelationType[type.Wildcard()].Remove(identity);
        //(with enough unit test coverage)
        if (_targetsByRelationType.TryGetValue(typeExpression.TypeId, out var targetSet))
        {
            targetSet.Remove(identity);
        }

        var oldEdge = oldTable.GetTableEdge(typeExpression);

        var newTable = oldEdge.Remove;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.ToList();
            newTypes.Remove(typeExpression);
            newTable = AddTable(new SortedSet<TypeExpression>(newTypes));
            oldEdge.Remove = newTable;

            var newEdge = newTable.GetTableEdge(typeExpression);
            newEdge.Add = oldTable;

            //Tables.Add(newTable); <-- already added in AddTable
        }

        var newRow = Table.MoveEntry(identity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.TableId = newTable.Id;
    }

    public void DiscardQuery(Mask mask)
    {
        _queries.Remove(mask);
        MaskPool.Return(mask);
    }

    public Query GetQuery(Mask mask, Func<Archetypes, Mask, List<Table>, Query> createQuery)
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

        var matchingTables = typeTables
            .Where(table => table.Matches(mask))
            .ToList();

        query = createQuery(this, mask, matchingTables);

        _queries.Add(mask, query);
        return query;
    }


    internal bool IsAlive(Identity identity)
    {
        return identity != Identity.None && _meta[identity.Id].Identity == identity;
    }


    internal ref EntityMeta GetEntityMeta(Identity identity)
    {
        return ref _meta[identity.Id];
    }


    internal Table GetTable(int tableId)
    {
        return _tables[tableId];
    }


    internal Entity GetTarget(TypeExpression typeExpression, Identity identity)
    {
        var meta = _meta[identity.Id];
        var table = _tables[meta.TableId];

        foreach (var storageType in table.Types)
        {
            if (!storageType.isRelation || storageType.TypeId != typeExpression.TypeId) continue;
            return new Entity(storageType.Target);
        }

        return Entity.None;
    }


    internal Entity[] GetTargets(TypeExpression typeExpression, Identity identity)
    {
        if (identity == Identity.Any)
        {
            return _targetsByRelationType.TryGetValue(typeExpression.TypeId, out var entitySet)
                ? entitySet.ToArray()
                : Array.Empty<Entity>();
        }

        AssertAlive(identity);

        var list = ListPool<Entity>.Rent();
        var meta = _meta[identity.Id];
        var table = _tables[meta.TableId];
        foreach (var storageType in table.Types)
        {
            if (!storageType.isRelation || storageType.TypeId != typeExpression.TypeId) continue;
            list.Add(new Entity(storageType.Target));
        }

        var targetEntities = list.ToArray();
        ListPool<Entity>.Return(list);

        return targetEntities;
    }


    internal (TypeExpression, object)[] GetComponents(Identity identity)
    {
        AssertAlive(identity);

        var list = ListPool<(TypeExpression, object)>.Rent();

        var meta = _meta[identity.Id];
        var table = _tables[meta.TableId];


        foreach (var type in table.Types)
        {
            var storage = table.GetStorage(type);
            list.Add((type, storage.GetValue(meta.Row)!));
        }

        var array = list.ToArray();
        ListPool<(TypeExpression, object)>.Return(list);
        return array;
    }


    private Table AddTable(SortedSet<TypeExpression> types)
    {
        var table = new Table(_tables.Count, this, types);
        _tables.Add(table);

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

            if (!_relationsByTypes.TryGetValue(type.TypeId, out var relationTypeSet))
            {
                relationTypeSet = [];
                _relationsByTypes[type.TypeId] = relationTypeSet;
            }

            relationTypeSet.Add(type);
        }

        foreach (var query in _queries.Values.Where(query => table.Matches(query.Mask)))
        {
            query.AddTable(table);
        }

        return table;
    }


    [Obsolete("Use new Identity(type)")]
    internal static Entity GetTypeEntity(Type type)
    {
        return new Identity(type);
    }


    private void ApplyDeferredOperations()
    {
        foreach (var op in _deferredOperations)
        {
            AssertAlive(op.Identity);

            switch (op.Operation)
            {
                case Deferred.Add:
                    AddComponent(op.TypeExpression, op.Identity, op.Data);
                    break;
                case Deferred.Remove:
                    RemoveComponent(op.TypeExpression, op.Identity);
                    break;
                case Deferred.Despawn:
                    Despawn(op.Identity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _deferredOperations.Clear();
    }


    public void Lock()
    {
        lock (_modeChangeLock)
        {
            if (_mode != Mode.Immediate) throw new InvalidOperationException("Archetypes: Lock called while not in immediate (default) mode");

            _lockCount++;
            _mode = Mode.Deferred;
        }
    }

    public void Unlock()
    {
        lock (_modeChangeLock)
        {
            if (_mode != Mode.Deferred) throw new InvalidOperationException("Archetypes: Unlock called while not in deferred mode");

            _lockCount--;

            if (_lockCount != 0) return;

            _mode = Mode.Immediate;
            ApplyDeferredOperations();
        }
    }

    private enum Mode
    {
        Immediate = default,
        Deferred,
        //Bulk
    }

    private struct DeferredOperation
    {
        public required Deferred Operation;
        public TypeExpression TypeExpression;
        public Identity Identity;
        public object Data;
    }

    private enum Deferred
    {
        Add,
        Remove,
        Despawn,
    }


    #region Assert Helpers

    private void AssertAlive(Identity identity)
    {
        if (!IsAlive(identity))
        {
            throw new Exception($"Entity {identity} is not alive.");
        }
    }


    private static void AssertEqual(Identity metaIdentity, Identity identity)
    {
        if (metaIdentity != identity)
        {
            throw new Exception($"Entity {identity} meta/generation mismatch.");
        }
    }

    #endregion
}