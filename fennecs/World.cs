// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace fennecs;

public partial class World : IEnumerable<Table>, IDisposable
{
    public EntityBuilder Spawn()
    {
        return new EntityBuilder(this, SpawnInternal());
    }

    public EntityBuilder On(Entity entity)
    {
        return new EntityBuilder(this, entity);
    }
    
    public void DespawnAllWith<T>()
    {
        var query = Query<Entity>().Has<T>().Build();
        query.Run(delegate(Span<Entity> entities)
        {
            foreach (var entity in entities) Despawn(entity);
        });
    }

    public bool HasComponent<T>(Entity entity)
    {
        var type = TypeExpression.Create<T>(Identity.None);
        return HasComponent(entity, type);
    }


    public void AddComponent<T>(Entity entity) where T : new()
    {
        var type = TypeExpression.Create<T>(Identity.None);
        AddComponent(type, entity.Identity, new T());
    }


    public void AddComponent<T>(Entity entity, T data)
    {
        var type = TypeExpression.Create<T>(Identity.None);
        AddComponent(type, entity.Identity, data);
    }


    public void RemoveComponent<T>(Entity entity)
    {
        var type = TypeExpression.Create<T>(Identity.None);
        RemoveComponent(entity.Identity, type);
    }


    public void RemoveComponent<T>(Entity entity, Type target)
    {
        var type = TypeExpression.Create<T>(new Identity(target));
        RemoveComponent(entity, type);
    }


    public IEnumerable<(TypeExpression, object)> GetComponents(Entity entity)
    {
        return GetComponents(entity.Identity);
    }


    public bool TryGetComponent<T>(Entity entity, out Ref<T> component)
    {
        if (!HasComponent<T>(entity))
        {
            component = default;
            return false;
        }

        component = new Ref<T>(ref GetComponent<T>(entity.Identity, Identity.None));
        return true;
    }


    public bool TryGetComponent<T>(Entity entity, Identity target, out Ref<T> component)
    {
        if (!HasComponent<T>(entity, target))
        {
            component = default;
            return false;
        }

        component = new Ref<T>(ref GetComponent<T>(entity.Identity, target));
        return true;
    }


    public bool HasComponent<T>(Entity entity, Entity target)
    {
        var type = TypeExpression.Create<T>(target.Identity);
        return HasComponent(entity.Identity, type);
    }


    public bool HasComponent<T>(Entity entity, Type target)
    {
        var type = TypeExpression.Create<T>(new Identity(target));
        return HasComponent(entity.Identity, type);
    }

    public bool HasComponent<T, Target>(Entity entity)
    {
        var type = TypeExpression.Create<T>(new Identity(LanguageType<Target>.Id));
        return HasComponent(entity.Identity, type);
    }


    public void AddComponent<T>(Entity entity, Entity target) where T : new()
    {
        var type = TypeExpression.Create<T>(target.Identity);
        AddComponent(type, entity.Identity, new T());
    }


    /* Todo: probably not worth it
    public void AddComponent<T, Target>(Entity entity)
    {
        var type = TypeExpression.Create<T, Target>();
        _AddComponent(type, entity.Identity, new T());
    }
    */


    public void AddComponent<T>(Entity entity, T component, Entity target)
    {
        var type = TypeExpression.Create<T>(target.Identity);
        AddComponent(type, entity.Identity, component);
    }

    public void RemoveComponent(Entity entity, Type type, Entity target)
    {
        var typeExpression = TypeExpression.Create(type, target.Identity);
        RemoveComponent(entity.Identity, typeExpression);
    }

    public void RemoveComponent<T>(Entity entity, Entity target)
    {
        var type = TypeExpression.Create<T>(target.Identity);
        RemoveComponent(entity.Identity, type);
    }


    public IEnumerable<Entity> GetTargets<T>(Entity entity)
    {
        var targets = new List<Entity>();
        CollectTargets<T>(targets, entity);
        return targets;
    }


    public void Dispose()
    {
    }

    #region QueryBuilders

    
    public QueryBuilder<Entity> Query()
    {
        return new QueryBuilder<Entity>(this);
    }

    public QueryBuilder<C> Query<C>()
    {
        return new QueryBuilder<C>(this);
    }

    public QueryBuilder<C1, C2> Query<C1, C2>() where C2 : struct
    {
        return new QueryBuilder<C1, C2>(this);
    }

    public QueryBuilder<C1, C2, C3> Query<C1, C2, C3>() where C1 : struct where C2 : struct where C3 : struct
    {
        return new QueryBuilder<C1, C2, C3>(this);
    }

    public QueryBuilder<C1, C2, C3, C4> Query<C1, C2, C3, C4>() where C1 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4>(this);
    }

    public QueryBuilder<C1, C2, C3, C4, C5> Query<C1, C2, C3, C4, C5>() where C1 : struct
    {
        return new QueryBuilder<C1, C2, C3, C4, C5>(this);
    }
    
    #endregion
    
    #region Archetypes
        private EntityMeta[] _meta = new EntityMeta[65536];
    private readonly List<Table> _tables = [];
    private readonly Dictionary<int, Query> _queries = new();


    private readonly Stack<Identity> _unusedIds = [];
    private Table entityRoot => _tables[0];

    internal int Count { get; private set; }

    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();
    private readonly Dictionary<TypeExpression, List<Table>> _tablesByType = new();
    
    private readonly Dictionary<Identity, HashSet<TypeExpression>> _typesByRelationTarget = new();

    private readonly object _modeChangeLock = new();

    private Mode _mode = Mode.Immediate;

    public World()
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

    public void CollectTargets<T>(List<Entity> entities)
    {
        var type = TypeExpression.Create<T>(Identity.Any);

        // Iterate through tables and get all concrete entities from their Archetype TypeExpressions
        foreach (var candidate in _tablesByType.Keys)
        {
            if (type.Matches(candidate)) entities.Add(new Entity(candidate.Target));
        }
    }

    public void CollectTargets<T>(List<Entity> entities, Entity origin)
    {
        var type = TypeExpression.Create<T>(origin.Identity);

        // Iterate through tables and get all concrete entities from their Archetype TypeExpressions
        foreach (var candidate in _tablesByType.Keys)
        {
            if (type.Matches(candidate)) entities.Add(new Entity(candidate.Target));
        }
    }

    private readonly object _spawnLock = new();

    internal Entity SpawnInternal(Type? type = default)
    {
        lock (_spawnLock)
        {
            if (!_unusedIds.TryPop(out var identity))
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
        lock (_spawnLock)
        {
            AssertAlive(identity);

            if (_mode == Mode.Deferred)
            {
                _deferredOperations.Enqueue(new DeferredOperation {Operation = Deferred.Despawn, Identity = identity});
                return;
            }

            ref var meta = ref _meta[identity.Id];

            var table = _tables[meta.TableId];
            table.Remove(meta.Row);
            meta.Clear();

            _unusedIds.Push(identity.Successor);

            // Find entity-entity relation reverse lookup (if applicable)
            if (!_typesByRelationTarget.TryGetValue(identity, out var list)) return;

            //Remove components from all entities that had a relation
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

    internal void AddComponent<T>(TypeExpression typeExpression, Identity identity, T data)
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
        var table = _tables[meta.TableId];
        var storage = (T[]) table.GetStorage(type);
        return ref storage[meta.Row];
    }


    internal bool HasComponent(Identity identity, TypeExpression typeExpression)
    {
        var meta = _meta[identity.Id];
        return meta.Identity != Identity.None
               && meta.Identity == identity
               && _tables[meta.TableId].Types.Contains(typeExpression);
    }


    internal void RemoveComponent(Identity identity, TypeExpression typeExpression)
    {
        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Operation = Deferred.Remove, Identity = identity, TypeExpression = typeExpression});
            return;
        }

        ref var meta = ref _meta[identity.Id];
        var oldTable = _tables[meta.TableId];

        if (!oldTable.Types.Contains(typeExpression))
        {
            throw new ArgumentException($"cannot remove non-existent component {typeExpression} from entity {identity}");
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

        var newRow = Table.MoveEntry(identity, meta.Row, oldTable, newTable);

        meta.Row = newRow;
        meta.TableId = newTable.Id;
    }

    public void DiscardQuery(Mask mask)
    {
        _queries.Remove(mask);
        MaskPool.Return(mask);
    }

    public Query GetQuery(Mask mask, Func<World, Mask, List<Table>, Query> createQuery)
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


    internal bool IsAlive(Identity identity)
    {
        return identity.IsEntity && _meta[identity.Id].Identity == identity;
    }


    internal ref EntityMeta GetEntityMeta(Identity identity)
    {
        return ref _meta[identity.Id];
    }
    
    internal Table GetTable(int tableId)
    {
        return _tables[tableId];
    }
    
    internal (TypeExpression, object)[] GetComponents(Identity identity)
    {
        AssertAlive(identity);

        using var list = PooledList<(TypeExpression, object)>.Rent();

        var meta = _meta[identity.Id];
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


    [Obsolete("Use new Identity(type)")]
    internal static Entity GetTypeEntity(Type type)
    {
        return new Identity(type);
    }


    private void ApplyDeferredOperations()
    {
        if (_deferredOperations.IsEmpty) return;
        
        foreach (var op in _deferredOperations)
        {
            AssertAlive(op.Identity);

            switch (op.Operation)
            {
                case Deferred.Add:
                    AddComponent(op.TypeExpression, op.Identity, op.Data);
                    break;
                case Deferred.Remove:
                    RemoveComponent(op.Identity, op.TypeExpression);
                    break;
                case Deferred.Despawn:
                    Despawn(op.Identity);
                    break;
            }
        }

        _deferredOperations.Clear();
    }


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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertAlive(Identity identity)
    {
        if (IsAlive(identity)) return;
        
        throw new ObjectDisposedException($"Entity {identity} is no longer alive.");
    }
    
    #endregion

    #region Enumerators
    public IEnumerator<Table> GetEnumerator() => _tables.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _tables).GetEnumerator();

    #endregion

    #endregion
}