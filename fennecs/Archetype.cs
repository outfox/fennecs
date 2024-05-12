// SPDX-License-Identifier: MIT

using System.Collections;
using System.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using fennecs.pools;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace fennecs;

/// <summary>
/// A storage of a class of Entities with a fixed set of Components, its <see cref="Signature"/>.
/// </summary>
public sealed class Archetype : IEnumerable<Entity>
{
    /// <summary>
    /// The TypeExpressions that define this Archetype.
    /// </summary>
    public readonly Signature<TypeExpression> Signature;

    /// <summary>
    /// Get a Span of all Identities contained in this Archetype.
    /// </summary>
    public ReadOnlySpan<Identity> Identities => _identities.AsSpan(0, Count);

    internal Array[] Storages => _storages;

    /// <summary>
    /// Number of Entities contained in this Archetype.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Does this Archetype currently contain no Entities?
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Current Capacity of this Archetype. This will grow as Entities are added and the Archetype resizes.
    /// </summary>
    public int Capacity => _identities.Length;

    private const int StartCapacity = 4;


    /// <summary>
    /// The World this Archetype is a part of.
    /// </summary>
    private readonly World _world;

    /// <summary>
    /// The Entities in this Archetype (filled contiguously from the bottom, as are the storages).
    /// </summary>
    private Identity[] _identities;

    /// <summary>
    /// Actual Component data storages. It' is a fixed size array because an Archetype doesn't change.
    /// </summary>
    private readonly Array[] _storages;

    private readonly Dictionary<TypeExpression, int> _storageIndices = new();

    /// <summary>
    /// TODO: Buckets for Wildcard Joins (optional optimization for CrossJoin when complex archetypes get hit repeatedly in tight loops).
    /// </summary>
    private readonly ImmutableDictionary<TypeID, Array[]> _buckets;

    // Used by Queries to check if the table has been modified while enumerating.
    private int _version;


    internal Archetype(World world, Signature<TypeExpression> signature)
    {
        _world = world;

        Signature = signature;

        _identities = new Identity[StartCapacity];

        _storages = new Array[signature.Count];

        // Build the relation between storages and types, as well as type Wildcards in buckets.
        var finishedTypes = PooledList<TypeID>.Rent();
        var finishedBuckets = PooledList<Array[]>.Rent();
        var currentBucket = PooledList<Array>.Rent();
        TypeID currentTypeId = 0;

        // Types are sorted by TypeID first, so we can iterate them in order to add them to Wildcard buckets.
        for (var index = 0; index < signature.Count; index++)
        {
            var type = signature[index];
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


    private void Match<T>(TypeExpression expression, IList<T[]> result)
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
        return type.Matches(Signature);
    }


    internal bool Matches(IReadOnlyList<TypeExpression> types)
    {
        return types.Any(Matches);
    }


    internal bool Matches(Mask mask)
    {
        //Not overrides both Any and Has.
        var matchesNot = !mask.NotTypes.Any(t => t.Matches(Signature));
        if (!matchesNot) return false;

        //If already matching, no need to check any further. 
        var matchesHas = mask.HasTypes.All(t => t.Matches(Signature));
        if (!matchesHas) return false;

        //Short circuit to avoid enumerating all AnyTypes if already matching; or if none present.
        var matchesAny = mask.AnyTypes.Count == 0;
        matchesAny |= mask.AnyTypes.Any(t => t.Matches(Signature));

        return matchesHas && matchesNot && matchesAny;
    }


    internal bool IsMatchSuperSet(IReadOnlyList<TypeExpression> matchTypes)
    {
        var match = true;
        for (var i = 0; i < matchTypes.Count; i++)
        {
            match &= matchTypes[i].Matches(Signature);
        }

        return match;
    }


    internal int Add(Identity identity)
    {
        Interlocked.Increment(ref _version);

        EnsureCapacity(Count + 1);
        _identities[Count] = identity;
        return Count++;
    }


    internal void Remove(int row)
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


    /// <summary>
    ///  Remove Entities from the Archetype that exceed a given count.
    /// </summary>
    /// <param name="maxEntityCount"></param>
    public void Truncate(int maxEntityCount)
    {
        var excess = Math.Clamp(Count - maxEntityCount, 0, Count);
        if (excess <= 0) return;

        // TODO: Build bulk deletion?
        var toDelete = Identities.Slice(Count - excess, excess);
        for (var i = toDelete.Length - 1; i >= 0; i--)
        {
            _world.Despawn(new Entity(_world, toDelete[i]));
        }
    }


    /// <summary>
    /// Moves all Entities from this Archetype to the destination Archetype back-filling with the provided Components.
    /// </summary>
    /// <param name="destination">the Archetype to move the entities to</param>
    /// <param name="additions">the new components and their TypeExpressions to add to the destination Archetype</param>
    /// <param name="backFills">values for each addition to add</param>
    internal void Migrate(Archetype destination, PooledList<TypeExpression> additions, PooledList<object> backFills)
    {
        if (destination == this)
        {
            destination.Fill(additions, backFills, 0, Count);
            return;
        }

        destination.EnsureCapacity(destination.Count + Count);
        // Subtractive copy
        foreach (var type in Signature)
        {
            if (!destination.Signature.Contains(type)) continue;
            var srcStorage = GetStorage(type);
            var destStorage = destination.GetStorage(type);
            Array.Copy(srcStorage, 0, destStorage, destination.Count, Count);
            Array.Clear(srcStorage);
        }

        // Additive back-fill of values
        destination.Fill(additions, backFills, destination.Count, Count);

        // Move identities
        for (var i = 0; i < Count; i++)
        {
            _world.GetEntityMeta(_identities[i]).Archetype = destination;
        }

        Array.Copy(_identities, 0, destination._identities, destination.Count, Count);


        // Update destination Archetype state
        destination.Count += Count;
        destination._version++;

        // Clear source Archetype state
        Array.Clear(_identities, 0, Count);
        Count = 0;
        _version++;
    }


    /// <summary>
    /// Fills all matching Storages of the archetype with each of the provided values.
    /// </summary>
    /// <param name="types">typeExpressions which storages to fill</param>
    /// <param name="values">values for the types</param>
    /// <param name="start">the index to start filling from</param>
    /// <param name="count">how many elements to fill</param>
    internal void Fill(PooledList<TypeExpression> types, PooledList<object> values, int start, int count)
    {
        for (var i = 0; i < types.Count; i++)
        {
            var type = types[i];
            var value = values[i];
            var storage = GetStorage(type);
            var elementType = storage.GetType().GetElementType()!;
            if (elementType.IsValueType)
            {
                for (var elementIndex = start; elementIndex < start + count; elementIndex++)
                {
                    storage.SetValue(value, elementIndex);
                }
            }
            else
            {
                Array.Fill((object[]) storage, value, start, count);
            }
        }
    }

    /// <summary>
    /// Fills the appropriate storage of the archetype with the provided value.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    internal void Fill<T>(TypeExpression type, T value)
    {
        var storage = (T[]) GetStorage(type);
        Array.Fill(storage, value);
    }

    internal T[] GetStorage<T>(Identity target)
    {
        var type = TypeExpression.Of<T>(target);
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


    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder("Archetype ");
        sb.AppendJoin("\n", Signature);
        return sb.ToString();
    }


    /// <inheritdoc />
    public IEnumerator<Entity> GetEnumerator()
    {
        var snapshot = Volatile.Read(ref _version);
        for (var i = 0; i < Count; i++)
        {
            if (snapshot != Volatile.Read(ref _version)) throw new InvalidOperationException("Collection modified while enumerating.");
            yield return new Entity(_world, _identities[i]);
        }
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns (constructs) the Entity at the given index, associated with the World this Archetype belongs to.
    /// </summary>
    /// <remarks>
    /// There's no bounds checking, so be sure to check against the Count property before using this method.
    /// (This is a performance optimization to avoid the overhead of bounds checking and exceptions in tight loops.)
    /// </remarks>
    public Entity this[int index] => new(_world, _identities[index]);


    #region Cross Joins
    internal Match.Join<C0> CrossJoin<C0>(TypeExpression[] streamTypes)
    {
        return IsEmpty ? default : new Match.Join<C0>(this, streamTypes);
    }


    internal Match.Join<C0, C1> CrossJoin<C0, C1>(TypeExpression[] streamTypes)
    {
        return IsEmpty ? default : new Match.Join<C0, C1>(this, streamTypes);
    }


    internal Match.Join<C0, C1, C2> CrossJoin<C0, C1, C2>(TypeExpression[] streamTypes)
    {
        return IsEmpty ? default : new Match.Join<C0, C1, C2>(this, streamTypes);
    }


    internal Match.Join<C0, C1, C2, C3> CrossJoin<C0, C1, C2, C3>(TypeExpression[] streamTypes)
    {
        return IsEmpty ? default : new Match.Join<C0, C1, C2, C3>(this, streamTypes);
    }


    internal Match.Join<C0, C1, C2, C3, C4> CrossJoin<C0, C1, C2, C3, C4>(TypeExpression[] streamTypes)
    {
        return IsEmpty ? default : new Match.Join<C0, C1, C2, C3, C4>(this, streamTypes);
    }
    #endregion
}