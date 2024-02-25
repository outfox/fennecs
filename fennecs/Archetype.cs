// SPDX-License-Identifier: MIT

using System.Collections;
using System.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using fennecs.pools;

namespace fennecs;

internal sealed class Archetype : IEnumerable<Entity>
{
    /// <summary>
    /// An Edge in the Graph that this Archetype is a member of.
    /// </summary>
    internal sealed class Edge
    {
        internal Archetype? Add;
        internal Archetype? Remove;
    }

    private const int StartCapacity = 4;
    
    public readonly ImmutableSortedSet<TypeExpression> Types;

    public Identity[] Identities => _identities;

    internal Array[] Storages => _storages;

    public int Count { get; private set; }
    public bool IsEmpty => Count == 0;

    public int Capacity => _identities.Length;

    private readonly World _world;

    private Identity[] _identities;

    /// <summary>
    /// Actual Component data storages. It' is a fixed size array because an Archetype doesn't change.
    /// </summary>
    private readonly Array[] _storages;

    private readonly Dictionary<TypeExpression, int> _storageIndices = new();

    private readonly Dictionary<TypeExpression, Edge> _edges = new();

    /// <summary>
    /// TODO: Buckets for Wildcard Joins
    /// </summary>
    private readonly ImmutableDictionary<TypeID, Array[]> _buckets;

    // Used by Queries to check if the table has been modified while enumerating.
    private int _version;


    public Archetype(World world, ImmutableSortedSet<TypeExpression> types)
    {
        _world = world;

        Types = types;

        _identities = new Identity[StartCapacity];

        _storages = new Array[types.Count];

        // Build the relation between storages and types, as well as type Wildcards in buckets.
        var finishedTypes = PooledList<TypeID>.Rent();
        var finishedBuckets = PooledList<Array[]>.Rent();
        var currentBucket = PooledList<Array>.Rent();
        TypeID currentTypeId = 0;

        // Types are sorted by TypeID first, so we can iterate them in order to add them to Wildcard buckets.
        for (var index = 0; index < types.Count; index++)
        {
            var type = types[index];
            _storageIndices.Add(type, index);
            _storages[index] = Array.CreateInstance(type.Type, StartCapacity);

            // Time for a new bucket?
            if (currentTypeId != type.TypeId)
            {
                //Finish bucket (exclude null type)
                if (currentTypeId != 0)
                {
                    finishedTypes.Add(currentTypeId);
                    finishedBuckets.Add(currentBucket.ToArray());
                    currentBucket.Dispose();
                    currentBucket = PooledList<Array>.Rent();
                }

                currentTypeId = type.TypeId;
            }

            //TODO: Harmless assert, but...  is it pretty? We could disallow TypeExpression 0, or skip null types.
            Debug.Assert(currentTypeId != 0, "Trying to create bucket for a null type.");
            currentBucket.Add(_storages[index]);
        }

        // Bake buckets dictionary
        _buckets = Zip(finishedTypes, finishedBuckets);

        currentBucket.Dispose();
        finishedBuckets.Dispose();
        finishedTypes.Dispose();
    }


    internal void Match<T>(TypeExpression expression, IList<T[]> result)
    {
        //TODO: Use TypeBuckets as optimization (much faster!).
        foreach (var (type, index) in _storageIndices)
        {
            if (expression.Matches(type))
            {
                result.Add((T[]) _storages[index]);
            }
        }
    }

    
    internal PooledList<T[]> Match<T>(TypeExpression expression)
    {
        var result = PooledList<T[]>.Rent();
        Match(expression, result);
        return result;
    }

    
    private static ImmutableDictionary<T, U> Zip<T, U>(IReadOnlyList<T> finishedTypes, IReadOnlyList<U> finishedBuckets) where T : notnull
    {
        var result = finishedTypes
            .Zip(finishedBuckets, (k, v) => new {Key = k, Value = v})
            .ToImmutableDictionary(item => item.Key, item => item.Value);
        return result;
    }


    internal bool Matches(TypeExpression type)
    {
        return type.Matches(Types);
    }


    internal bool Matches(Mask mask)
    {
        //Not overrides both Any and Has.
        var matchesNot = !mask.NotTypes.Any(t => t.Matches(Types));
        if (!matchesNot) return false;

        //If already matching, no need to check any further. 
        var matchesHas = mask.HasTypes.All(t => t.Matches(Types));
        if (!matchesHas) return false;

        //Short circuit to avoid enumerating all AnyTypes if already matching; or if none present.
        var matchesAny = mask.AnyTypes.Count == 0;
        matchesAny |= mask.AnyTypes.Any(t => t.Matches(Types));

        return matchesHas && matchesNot && matchesAny;
    }


    public int Add(Identity identity)
    {
        Interlocked.Increment(ref _version);

        EnsureCapacity(Count + 1);
        _identities[Count] = identity;
        return Count++;
    }


    public void Remove(int row)
    {
        Interlocked.Increment(ref _version);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(row, Count, nameof(row));

        Count--;

        // If removing not the last row, move the last row to the removed row
        if (row < Count)
        {
            _identities[row] = _identities[Count];
            foreach (var storage in _storages)
            {
                Array.Copy(storage, Count, storage, row, 1);
            }

            _world.GetEntityMeta(_identities[row]).Row = row;
        }

        // Free the last row
        _identities[Count] = default;

        foreach (var storage in _storages) Array.Clear(storage, Count, 1);
    }


    public Edge GetTableEdge(TypeExpression typeExpression)
    {
        if (_edges.TryGetValue(typeExpression, out var edge)) return edge;

        edge = new Edge();
        _edges[typeExpression] = edge;

        return edge;
    }


    public T[] GetStorage<T>(Identity target)
    {
        var type = TypeExpression.Create<T>(target);
        return (T[]) GetStorage(type);
    }
    
    
    internal Array GetStorage(TypeExpression typeExpression) => _storages[_storageIndices[typeExpression]];

    
    private void EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));

        if (capacity <= _identities.Length) return;

        Resize(Math.Max(capacity, StartCapacity) * 2);
    }


    internal void Resize(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length, nameof(length));
        ArgumentOutOfRangeException.ThrowIfLessThan(length, Count, nameof(length));

        Array.Resize(ref _identities, length);

        for (var i = 0; i < _storages.Length; i++)
        {
            var elementType = _storages[i].GetType().GetElementType()!;
            var newStorage = Array.CreateInstance(elementType, length);
            Array.Copy(_storages[i], newStorage, Math.Min(_storages[i].Length, length));
            _storages[i] = newStorage;
        }
    }


    
    internal void Set<T>(TypeExpression typeExpression, T data, int newRow)
    {
        // DeferredOperation sends data as objects
        if (typeof(T).IsAssignableFrom(typeof(object)))
        {
            var sysArray = GetStorage(typeExpression);
            sysArray.SetValue(data, newRow);
            return;
        }
        
        var storage = (T[]) GetStorage(typeExpression);
        storage[newRow] = data;
    }
    

    internal static int MoveEntry(Identity identity, int oldRow, Archetype oldArchetype, Archetype newArchetype)
    {
        var newRow = newArchetype.Add(identity);

        foreach (var (type, oldIndex) in oldArchetype._storageIndices)
        {
            if (!newArchetype._storageIndices.TryGetValue(type, out var newIndex)) continue;

            var oldStorage = oldArchetype._storages[oldIndex];
            var newStorage = newArchetype._storages[newIndex];

            Array.Copy(oldStorage, oldRow, newStorage, newRow, 1);
        }

        oldArchetype.Remove(oldRow);

        return newRow;
    }



    public override string ToString()
    {
        var sb = new StringBuilder($"Archetype ");
        sb.AppendJoin("\n", Types);
        return sb.ToString();
    }
    

    public IEnumerator<Entity> GetEnumerator()
    {
        var snapshot = _version;
        for (var i = 0; i < Count; i++)
        {
            if (snapshot != _version) throw new InvalidOperationException("Collection modified while enumerating.");
            yield return new Entity(_world, _identities[i]);
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Entity this[int index] => new(_world, _identities[index]);
}