// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Collections.Immutable;
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

    private readonly List<Archetype> _archetypes = [];
    
    private readonly Dictionary<int, Query> _queries = new();

    // The "Identity" Archetype, which is the root of the Archetype Graph.
    private readonly Archetype _root;

    private readonly ConcurrentQueue<DeferredOperation> _deferredOperations = new();
    
    private readonly Dictionary<TypeExpression, List<Archetype>> _tablesByType = new();
    private readonly Dictionary<Identity, HashSet<TypeExpression>> _typesByRelationTarget = new();

    private readonly object _modeChangeLock = new();

    private Mode _mode = Mode.Immediate;

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
    
    public void CollectTargets<T>(List<Identity> entities)
    {
        var type = TypeExpression.Create<T>(Match.Any);

        // Iterate through tables and get all concrete Entities from their Archetype TypeExpressions
        foreach (var candidate in _tablesByType.Keys)
        {
            if (type.Matches(candidate)) entities.Add(candidate.Target);
        }
    }

    private readonly object _spawnLock = new();

    #region CRUD
    private Identity NewEntity()
    {
        lock (_spawnLock)
        {
            var identity = _identityPool.Spawn();

            var row = _root.Add(identity);

            while (_meta.Length <= _identityPool.Living) Array.Resize(ref _meta, _meta.Length * 2);

            _meta[identity.Index] = new EntityMeta(identity, _root, row);

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

    private void RemoveComponent(Identity identity, TypeExpression typeExpression)
    {
        if (_mode == Mode.Deferred)
        {
            _deferredOperations.Enqueue(new DeferredOperation {Code = OpCode.Remove, Identity = identity, TypeExpression = typeExpression});
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
        //meta.ArchId = newTable.Id;
        meta.Archetype = newTable;
    }
    #endregion

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


    internal ref EntityMeta GetEntityMeta(Identity identity)
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
            AssertAlive(op.Identity);

            switch (op.Code)
            {
                case OpCode.Add:
                    AddComponent(op.Identity, op.TypeExpression, op.Data);
                    break;
                case OpCode.Remove:
                    RemoveComponent(op.Identity, op.TypeExpression);
                    break;
                case OpCode.Despawn:
                    Despawn(op.Identity);
                    break;
            }
        }
    }


    public struct DeferredOperation
    {
        public required OpCode Code;
        public TypeExpression TypeExpression;
        public Identity Identity;
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
    private void AssertAlive(Identity identity)
    {
        if (IsAlive(identity)) return;

        throw new ObjectDisposedException($"Identity {identity} is no longer alive.");
    }

    #endregion
}