using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HypEcs;

public sealed class Archetypes
{
    internal EntityMeta[] Meta = new EntityMeta[512];

    internal readonly Queue<Identity> UnusedIds = new();

    internal readonly List<Table> Tables = new();

    internal readonly Dictionary<int, Query> Queries = new();

    internal int EntityCount;

    readonly List<TableOperation> _tableOperations = new();
    readonly Dictionary<Type, Entity> _typeEntities = new();
    internal readonly Dictionary<StorageType, List<Table>> TablesByType = new();
    readonly Dictionary<Identity, HashSet<StorageType>> _typesByRelationTarget = new();
    readonly Dictionary<int, HashSet<StorageType>> _relationsByTypes = new();

    int _lockCount;
    bool _isLocked;

    public Archetypes()
    {
        AddTable(new SortedSet<StorageType> { StorageType.Create<Entity>(Identity.None) });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity Spawn()
    {
        var identity = UnusedIds.Count > 0 ? UnusedIds.Dequeue() : new Identity(++EntityCount);

        var table = Tables[0];

        var row = table.Add(identity);

        if (Meta.Length == EntityCount) Array.Resize(ref Meta, EntityCount << 1);

        Meta[identity.Id] = new EntityMeta(identity, table.Id, row);

        var entity = new Entity(identity);
        
        var entityStorage = (Entity[])table.Storages[0];
        entityStorage[row] = entity;

        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Despawn(Identity identity)
    {
        if (!IsAlive(identity)) return;

        if (_isLocked)
        {
            _tableOperations.Add(new TableOperation { Despawn = true, Identity = identity });
            return;
        }

        ref var meta = ref Meta[identity.Id];

        var table = Tables[meta.TableId];

        table.Remove(meta.Row);

        meta.Row = 0;
        meta.Identity = Identity.None;

        UnusedIds.Enqueue(identity);

        if (!_typesByRelationTarget.TryGetValue(identity, out var list))
        {
            return;
        }

        foreach (var type in list)
        {
            var tablesWithType = TablesByType[type];

            foreach (var tableWithType in tablesWithType)
            {
                for (var i = 0; i < tableWithType.Count; i++)
                {
                    RemoveComponent(type, tableWithType.Identities[i]);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent(StorageType type, Identity identity, object data)
    {
        ref var meta = ref Meta[identity.Id];
        var oldTable = Tables[meta.TableId];

        if (oldTable.Types.Contains(type))
        {
            throw new Exception($"Entity {identity} already has component of type {type}");
        }

        if (_isLocked)
        {
            _tableOperations.Add(new TableOperation { Add = true, Identity = identity, Type = type, Data = data });
            return;
        }

        var oldEdge = oldTable.GetTableEdge(type);

        var newTable = oldEdge.Add;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.ToList();
            newTypes.Add(type);
            newTable = AddTable(new SortedSet<StorageType>(newTypes));
            oldEdge.Add = newTable;

            var newEdge = newTable.GetTableEdge(type);
            newEdge.Remove = oldTable;
        }

        var newRow = Table.MoveEntry(identity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.TableId = newTable.Id;

        var storage = newTable.GetStorage(type);
        storage.SetValue(data, newRow);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>(Identity identity, Identity target)
    {
        var type = StorageType.Create<T>(target);
        var meta = Meta[identity.Id];
        var table = Tables[meta.TableId];
        var storage = (T[])table.GetStorage(type);
        return ref storage[meta.Row];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent(StorageType type, Identity identity)
    {
        var meta = Meta[identity.Id];
        return meta.Identity != Identity.None && Tables[meta.TableId].Types.Contains(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveComponent(StorageType type, Identity identity)
    {
        ref var meta = ref Meta[identity.Id];
        var oldTable = Tables[meta.TableId];

        if (!oldTable.Types.Contains(type))
        {
            throw new Exception($"cannot remove non-existent component {type.Type.Name} from entity {identity}");
        }

        if (_isLocked)
        {
            _tableOperations.Add(new TableOperation { Add = false, Identity = identity, Type = type });
            return;
        }

        var oldEdge = oldTable.GetTableEdge(type);

        var newTable = oldEdge.Remove;

        if (newTable == null)
        {
            var newTypes = oldTable.Types.ToList();
            newTypes.Remove(type);
            newTable = AddTable(new SortedSet<StorageType>(newTypes));
            oldEdge.Remove = newTable;

            var newEdge = newTable.GetTableEdge(type);
            newEdge.Add = oldTable;

            Tables.Add(newTable);
        }

        var newRow = Table.MoveEntry(identity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.TableId = newTable.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query GetQuery(Mask mask, Func<Archetypes, Mask, List<Table>, Query> createQuery)
    {
        var hash = mask.GetHashCode();

        if (Queries.TryGetValue(hash, out var query))
        {
            MaskPool.Add(mask);
            return query;
        }

        var matchingTables = new List<Table>();

        var type = mask.HasTypes[0];
        if (!TablesByType.TryGetValue(type, out var typeTables))
        {
            typeTables = new List<Table>();
            TablesByType[type] = typeTables;
        }

        foreach (var table in typeTables)
        {
            if (!IsMaskCompatibleWith(mask, table)) continue;

            matchingTables.Add(table);
        }

        query = createQuery(this, mask, matchingTables);
        Queries.Add(hash, query);

        return query;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsMaskCompatibleWith(Mask mask, Table table)
    {
        var has = ListPool<StorageType>.Get();
        var not = ListPool<StorageType>.Get();
        var any = ListPool<StorageType>.Get();

        var hasAnyTarget = ListPool<StorageType>.Get();
        var notAnyTarget = ListPool<StorageType>.Get();
        var anyAnyTarget = ListPool<StorageType>.Get();

        foreach (var type in mask.HasTypes)
        {
            if (type.Identity == Identity.Any) hasAnyTarget.Add(type);
            else has.Add(type);
        }

        foreach (var type in mask.NotTypes)
        {
            if (type.Identity == Identity.Any) notAnyTarget.Add(type);
            else not.Add(type);
        }

        foreach (var type in mask.AnyTypes)
        {
            if (type.Identity == Identity.Any) anyAnyTarget.Add(type);
            else any.Add(type);
        }

        var matchesComponents = table.Types.IsSupersetOf(has);
        matchesComponents &= !table.Types.Overlaps(not);
        matchesComponents &= mask.AnyTypes.Count == 0 || table.Types.Overlaps(any);

        var matchesRelation = true;

        foreach (var type in hasAnyTarget)
        {
            if (!_relationsByTypes.TryGetValue(type.TypeId, out var list))
            {
                matchesRelation = false;
                continue;
            }

            matchesRelation &= table.Types.Overlaps(list);
        }

        ListPool<StorageType>.Add(has);
        ListPool<StorageType>.Add(not);
        ListPool<StorageType>.Add(any);
        ListPool<StorageType>.Add(hasAnyTarget);
        ListPool<StorageType>.Add(notAnyTarget);
        ListPool<StorageType>.Add(anyAnyTarget);

        return matchesComponents && matchesRelation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAlive(Identity identity)
    {
        return Meta[identity.Id].Identity != Identity.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref EntityMeta GetEntityMeta(Identity identity)
    {
        return ref Meta[identity.Id];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Table GetTable(int tableId)
    {
        return Tables[tableId];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Entity GetTarget(StorageType type, Identity identity)
    {
        var meta = Meta[identity.Id];
        var table = Tables[meta.TableId];

        foreach (var storageType in table.Types)
        {
            if (!storageType.IsRelation || storageType.TypeId != type.TypeId) continue;
            return new Entity(storageType.Identity);
        }

        return Entity.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Entity[] GetTargets(StorageType type, Identity identity)
    {
        var meta = Meta[identity.Id];
        var table = Tables[meta.TableId];

        var list = ListPool<Entity>.Get();

        foreach (var storageType in table.Types)
        {
            if (!storageType.IsRelation || storageType.TypeId != type.TypeId) continue;
            list.Add(new Entity(storageType.Identity));
        }

        var targetEntities = list.ToArray();
        ListPool<Entity>.Add(list);

        return targetEntities;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal (StorageType, object)[] GetComponents(Identity identity)
    {
        var meta = Meta[identity.Id];
        var table = Tables[meta.TableId];

        var list = ListPool<(StorageType, object)>.Get();

        foreach (var type in table.Types)
        {
            var storage = table.GetStorage(type);
            list.Add((type, storage.GetValue(meta.Row)!));
        }

        var array = list.ToArray();
        ListPool<(StorageType, object)>.Add(list);
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Table AddTable(SortedSet<StorageType> types)
    {
        var table = new Table(Tables.Count, this, types);
        Tables.Add(table);

        foreach (var type in types)
        {
            if (!TablesByType.TryGetValue(type, out var tableList))
            {
                tableList = new List<Table>();
                TablesByType[type] = tableList;
            }

            tableList.Add(table);

            if (!type.IsRelation) continue;

            if (!_typesByRelationTarget.TryGetValue(type.Identity, out var typeList))
            {
                typeList = new HashSet<StorageType>();
                _typesByRelationTarget[type.Identity] = typeList;
            }

            typeList.Add(type);

            if (!_relationsByTypes.TryGetValue(type.TypeId, out var relationTypeSet))
            {
                relationTypeSet = new HashSet<StorageType>();
                _relationsByTypes[type.TypeId] = relationTypeSet;
            }

            relationTypeSet.Add(type);
        }

        foreach (var query in Queries.Values.Where(query => IsMaskCompatibleWith(query.Mask, table)))
        {
            query.AddTable(table);
        }

        return table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Entity GetTypeEntity(Type type)
    {
        if (!_typeEntities.TryGetValue(type, out var entity))
        {
            entity = Spawn();
            _typeEntities.Add(type, entity);
        }

        ;

        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ApplyTableOperations()
    {
        foreach (var op in _tableOperations)
        {
            if (!IsAlive(op.Identity)) continue;

            if (op.Despawn) Despawn(op.Identity);
            else if (op.Add) AddComponent(op.Type, op.Identity, op.Data);
            else RemoveComponent(op.Type, op.Identity);
        }

        _tableOperations.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Lock()
    {
        _lockCount++;
        _isLocked = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Unlock()
    {
        _lockCount--;
        if (_lockCount != 0) return;
        _isLocked = false;

        ApplyTableOperations();
    }

    struct TableOperation
    {
        public bool Despawn;
        public bool Add;
        public StorageType Type;
        public Identity Identity;
        public object Data;
    }
}