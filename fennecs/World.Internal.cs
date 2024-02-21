// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs;

public partial class World : IDisposable
{
    public void Dispose()
    {
    }

    #region Archetypes

    private readonly IdentityPool _identityPool;

    private EntityMeta[] _meta;

    internal int Count
    {
        get
        {
            lock (_spawnLock)
            {
                return _identityPool.Count;
            }
        }
    }

    private readonly List<Table> _tables = [];
    private readonly Dictionary<int, Query> _queries = new();

    // The "Identity" Archetype, which is the root of the Archetype Graph.
    private readonly Table _root;


    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();
    
    private readonly Dictionary<TypeExpression, List<Table>> _tablesByType = new();
    private readonly Dictionary<Entity, HashSet<TypeExpression>> _typesByRelationTarget = new();

    private readonly object _modeChangeLock = new();

    private Mode _mode = Mode.Immediate;

    public void CollectTargets<T>(List<Entity> entities)
    {
        var type = TypeExpression.Create<T>(Entity.Any);

        // Iterate through tables and get all concrete entities from their Archetype TypeExpressions
        foreach (var candidate in _tablesByType.Keys)
        {
            if (type.Matches(candidate)) entities.Add(candidate.Target);
        }
    }

    private readonly object _spawnLock = new();

    #region CRUD
    private Entity NewEntity()
    {
        lock (_spawnLock)
        {
            var identity = _identityPool.Spawn();

            var row = _root.Add(identity);

            while (_meta.Length <= _identityPool.Living) Array.Resize(ref _meta, _meta.Length * 2);

            _meta[identity.Id] = new EntityMeta(identity, _root.Id, row);

            var entityStorage = (Entity[]) _root.Storages.First();
            entityStorage[row] = identity;

            return identity;
        }
    }

    private bool HasComponent(Entity entity, TypeExpression typeExpression)
    {
        var meta = _meta[entity.Id];
        return meta.Entity != Entity.None
               && meta.Entity == entity
               && typeExpression.Matches(_tables[meta.TableId].Types);
    }

    private void RemoveComponent(Entity entity, TypeExpression typeExpression)
    {
        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Code = OpCode.Remove, Entity = entity, TypeExpression = typeExpression});
            return;
        }

        ref var meta = ref _meta[entity.Id];
        var oldTable = _tables[meta.TableId];

        if (!oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"cannot remove non-existent component {typeExpression} from identity {entity}");
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
        }

        var newRow = Table.MoveEntry(entity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.TableId = newTable.Id;
    }
    #endregion

    internal Query GetQuery(Mask mask, Func<World, Mask, List<Table>, Query> createQuery)
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

        var matchingTables = PooledList<Table>.Rent();
        foreach (var table in _tables)
        {
            if (table.Matches(mask)) matchingTables.Add(table);
        }

        query = createQuery(this, mask, matchingTables);

        _queries.Add(mask, query);
        return query;
    }

    internal void RemoveQuery(Query query)
    {
        _queries.Remove(query.Mask);
    }


    internal ref EntityMeta GetEntityMeta(Entity entity)
    {
        return ref _meta[entity.Id];
    }

    internal Table GetTable(int tableId)
    {
        return _tables[tableId];
    }

    internal (TypeExpression, object)[] GetComponents(Entity entity)
    {
        AssertAlive(entity);

        using var list = PooledList<(TypeExpression, object)>.Rent();

        var meta = _meta[entity.Id];
        var table = _tables[meta.TableId];


        foreach (var type in table.Types)
        {
            var storage = table.GetStorage(type);
            list.Add((type, storage.GetValue(meta.Row)!));
        }

        var array = list.ToArray();
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
        }

        foreach (var query in _queries.Values.Where(query => table.Matches(query.Mask)))
        {
            query.AddTable(table);
        }

        return table;
    }

    #endregion

    public void Lock()
    {
        lock (_modeChangeLock)
        {
            if (_mode != Mode.Immediate) throw new InvalidOperationException("this: Lock called while not in immediate (default) mode");

            _mode = Mode.Deferred;
        }
    }

    public void Unlock()
    {
        lock (_modeChangeLock)
        {
            if (_mode != Mode.Deferred) throw new InvalidOperationException("this: Unlock called while not in deferred mode");

            _mode = Mode.Immediate;
            Apply(_deferredOperations);
        }
    }


    private void Apply(ConcurrentQueue<DeferredOperation> operations)
    {
        while (operations.TryDequeue(out var op))
        {
            AssertAlive(op.Entity);

            switch (op.Code)
            {
                case OpCode.Add:
                    AddComponent(op.Entity, op.TypeExpression, op.Data);
                    break;
                case OpCode.Remove:
                    RemoveComponent(op.Entity, op.TypeExpression);
                    break;
                case OpCode.Despawn:
                    Despawn(op.Entity);
                    break;
            }
        }
    }


    public struct DeferredOperation
    {
        public required OpCode Code;
        public TypeExpression TypeExpression;
        public Entity Entity;
        public object Data;
    }

    public enum OpCode
    {
        Add,
        Remove,
        Despawn,
    }

    private enum Mode
    {
        Immediate = default,
        Deferred,
        //Bulk
    }

    #region Assert Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertAlive(Entity entity)
    {
        if (IsAlive(entity)) return;

        throw new ObjectDisposedException($"Identity {entity} is no longer alive.");
    }

    #endregion
}