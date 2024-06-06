// SPDX-License-Identifier: MIT

using System.Collections;
using System.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using fennecs.pools;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace fennecs;

/// <summary>
/// A storage of a class of Entities with a fixed set of Components, its <see cref="Signature"/>.
/// </summary>
public sealed class Archetype : IEnumerable<Entity>, IComparable<Archetype>
{
    /// <summary>
    /// The TypeExpressions that define this Archetype.
    /// </summary>
    internal readonly Signature<TypeExpression> Signature;

    /// <summary>
    /// Get a Span of all Identities contained in this Archetype.
    /// </summary>
    internal ReadOnlySpan<Identity> Identities => IdentityStorage.Span;

    /// <summary>
    /// Actual Component data storages. It' is a fixed size array because an Archetype doesn't change.
    /// </summary>
    private IStorage[] Storages { get; }

    /// <summary>
    /// Number of Entities contained in this Archetype.
    /// </summary>
    public int Count => IdentityStorage.Count;

    /// <summary>
    /// Does this Archetype currently contain no Entities?
    /// </summary>
    public bool IsEmpty => Count == 0;
    
    
    /// <summary>
    /// The World this Archetype is a part of.
    /// </summary>
    private readonly World _world;

    /// <summary>
    /// The Entities in this Archetype (filled contiguously from the bottom, as are the storages).
    /// </summary>
    internal readonly Storage<Identity> IdentityStorage;

    private readonly Dictionary<TypeExpression, int> _storageIndices = new();

    /// <summary>
    /// TODO: Buckets for Wildcard Joins (optional optimization for CrossJoin when complex archetypes get hit repeatedly in tight loops).
    /// </summary>
    // private readonly ImmutableDictionary<TypeID, IStorage[]> _buckets;

    // Used by Queries to check if the table has been modified while enumerating.
    internal int Version;


    internal Archetype(World world, Signature<TypeExpression> signature)
    {
        _world = world;
        Storages = new IStorage[signature.Count];
        
        Signature = signature;
        
        // Build the relation between storages and types, as well as type Wildcards in buckets.
        var finishedTypes = PooledList<TypeID>.Rent();
        var finishedBuckets = PooledList<IStorage[]>.Rent();
        var currentBucket = PooledList<IStorage>.Rent();
        TypeID currentTypeId = 0;

        // Types are sorted by TypeID first, so we can iterate them in order to add them to Wildcard buckets.
        for (var index = 0; index < signature.Count; index++)
        {
            var type = signature[index];
            _storageIndices.Add(type, index);
            Storages[index] = IStorage.Instantiate(type);

            // Time for a new bucket?
            if (currentTypeId != type.TypeId)
            {
                //Finish bucket (exclude null type)
                if (currentTypeId != 0)
                {
                    finishedTypes.Add(currentTypeId);
                    finishedBuckets.Add(currentBucket.ToArray());
                    currentBucket.Dispose();
                    currentBucket = PooledList<IStorage>.Rent();
                }

                currentTypeId = type.TypeId;
            }

            //TODO: Harmless assert, but...  is it pretty? We could disallow TypeExpression 0, or skip null types.
            Debug.Assert(currentTypeId != 0, "Trying to create bucket for a null type.");
            currentBucket.Add(Storages[index]);
        }

        // Get quick lookup for Identity component (non-relational)
        // CAVEAT: This isn't necessarily at index 0 because another
        // TypeExpression may have been created before the first TE of Identity.
        IdentityStorage = GetStorage<Identity>(fennecs.Identity.Plain);

        // TODO: Bake buckets dictionary
        // _buckets = Zip(finishedTypes, finishedBuckets);

        currentBucket.Dispose();
        finishedBuckets.Dispose();
        finishedTypes.Dispose();
    }


    private void Match<T>(TypeExpression expression, IList<Storage<T>> result)
    {
        //TODO: Use TypeBuckets as optimization (much faster!).
        foreach (var (type, index) in _storageIndices)
        {
            if (expression.Matches(type))
            {
                result.Add((Storage<T>) Storages[index]);
            }
        }
    }


    internal PooledList<Storage<T>> Match<T>(TypeExpression expression)
    {
        var result = PooledList<Storage<T>>.Rent();
        Match(expression, result);
        return result;
    }

    
    // TODO: This is a surprise tool that will help us later :)
    // ReSharper disable once UnusedMember.Local
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
        var matches = true;
        for (var i = 0; i < matchTypes.Count; i++)
        {
            matches &= matchTypes[i].Matches(Signature);
        }

        return matches;
    }

    
    internal void Delete(int entry, int count = 1)
    {
        Invalidate();

        foreach (var storage in Storages)
        {
            storage.Delete(entry, count);
        }
    }

    /// <summary>
    ///  Remove Entities from the Archetype that exceed a given count.
    /// </summary>
    /// <param name="maxEntityCount"></param>
    public void Truncate(int maxEntityCount)
    {
        var excess = Math.Clamp(Count - maxEntityCount, 0, Count);
        if (excess <= 0) return;
        
        var toDelete = Identities.Slice(Count - excess, excess);
        foreach (var storage in Storages)
        {
            // HACK... 
            if (storage == IdentityStorage) continue;
            
            //Must call before World removes Dependencies (can have dependencies in same archetype!)
            //TODO: Urgently needs unit test to rule out dangerous conflicts!
            storage.Delete(Count-excess, excess);
        }

        _world.Recycle(toDelete);
        IdentityStorage.Delete(Count - excess, excess);
    }

    internal void PatchMetas(int entry, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var identity = IdentityStorage[entry + i];
            ref var meta = ref _world.GetEntityMeta(identity);
            meta = new() { Identity = identity, Archetype = this, Row = entry + i };
        }
    }

    /// <summary>
    /// Moves all Entities from this Archetype to the destination Archetype back-filling with the provided Components.
    /// </summary>
    /// <param name="destination">the Archetype to move the entities to</param>
    /// <param name="additions">the new components and their TypeExpressions to add to the destination Archetype</param>
    /// <param name="backFills">values for each addition to add</param>
    /// <param name="addMode"></param>
    internal void Migrate(Archetype destination, PooledList<TypeExpression> additions, PooledList<object> backFills, Batch.AddConflict addMode)
    {
        Invalidate();
        destination.Invalidate();

        var addedCount = Count;
        var addedStart = destination.Count;

        // Replacement pre-fill of values ("Replace")
        if (addMode == Batch.AddConflict.Replace)
        {
            var alreadyPresent = Signature.Intersect(additions);
            foreach (var type in alreadyPresent)
            {
                var value = backFills[additions.IndexOf(type)];
                Fill(type, value); //Fill with value to replace before migrating.
            }
        }
        
        // Certain Add-modes permit operating on archetypes that themselves are in the query.
        // No more migrations are needed at this point (they would be semantically idempotent)
        if (destination == this) return;

        // Migration (and subtractive copy)
        foreach (var type in Signature)
        {
            var srcStorage = GetStorage(type);
            if (destination.Signature.Contains(type))
            {
                var destStorage = destination.GetStorage(type);
                srcStorage.Migrate(destStorage);
            }
            else
            {
                // Discard values not in the destination (subtract components)
                srcStorage.Clear();
            }
        }

        // Additive back-fill of values ("Preserve" and "Strict")
        foreach (var type in destination.Signature.Except(Signature))
        {
            var value = backFills[additions.IndexOf(type)];
            destination.BackFill(type, value, addedCount);
        }
        
        // Update all Meta info to mark entities as moved.
        destination.PatchMetas(addedStart, addedCount);
    }


    /// <summary>
    /// Moves all Entities from this Archetype to the destination Archetype,
    /// discarding any components not present in the destination.
    /// </summary>
    /// <param name="destination">the Archetype to move the entities to</param>
    internal void Migrate(Archetype destination)
    {
        if (IsEmpty) return;
        
        // Certain Add-modes permit operating on archetypes that themselves are in the query.
        // No more migrations are needed at this point (they would be semantically idempotent)
        if (destination == this) return;
        
        Invalidate();
        destination.Invalidate();
        
        var addedCount = Count;
        var addedStart = destination.Count;


        // Migration (and subtractive copy)
        foreach (var type in Signature)
        {
            var srcStorage = GetStorage(type);
            if (destination.Signature.Contains(type))
            {
                var destStorage = destination.GetStorage(type);
                srcStorage.Migrate(destStorage);
            }
            else
            {
                // Discard values not in the destination (subtract components)
                srcStorage.Clear();
            }
        }
        
        // Update all Meta info to mark entities as moved.
        destination.PatchMetas(addedStart, addedCount);
    }


    /// <summary>
    /// Fills the appropriate storage of the archetype with the provided value.
    /// </summary>
    internal void Fill<T>(TypeExpression type, T value) where T: notnull
    {
        // DeferredOperation sends data as objects
        if (typeof(T).IsAssignableFrom(typeof(object)))
        {
            var sysArray = GetStorage(type);
            sysArray.Blit(value);
            return;
        }

        var join = CrossJoin<T>([type]);
        if (join.Empty) return;
        do
        {
            var storage = join.Select;
            storage.Blit(value);
        } while (join.Iterate());
    }


    internal Storage<T> GetStorage<T>(Target target)
    {
        var type = TypeExpression.Of<T>(target);
        return (Storage<T>) GetStorage(type);
    }


    internal IStorage GetStorage(TypeExpression typeExpression) => Storages[_storageIndices[typeExpression]];
    
    
    internal void BackFill<T>(TypeExpression typeExpression, T value, int additions) where T: notnull
    {
        // DeferredOperation sends data as objects (decorated with TypeExpressions)
        if (typeof(T).IsAssignableFrom(typeof(object)))
        {
            var iStorage = GetStorage(typeExpression);
            iStorage.Append(value, additions);
            return;
        }
        
        var storage = (Storage<T>) GetStorage(typeExpression);
        storage.Append(value);
    }


    internal static void MoveEntry(int entry, Archetype source, Archetype destination)
    {
        // We do this at the start to flag down any running, possibly async enumerators.
        source.Invalidate();
        destination.Invalidate();

        foreach (var (type, oldIndex) in source._storageIndices)
        {
            if (!destination._storageIndices.TryGetValue(type, out var newIndex))
            {
                // Move is subtractive, discard anything we don't have in the destination
                source.Storages[oldIndex].Delete(entry);
                continue;
            }

            var oldStorage = source.Storages[oldIndex];
            var newStorage = destination.Storages[newIndex];

            oldStorage.Move(entry, newStorage);
        }

        // If we cycled an entity from the end of the storages, need Row update.
        if (source.Count > entry) source.PatchMetas(entry);
        
        // Entity was moved, needs both Archetype and Row update.
        destination.PatchMetas(destination.Count-1);
    }


    /// <inheritdoc />
    public int CompareTo(Archetype? other)
    {
        return other == null ? 1 : Signature.CompareTo(other.Signature);
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
        var snapshot = Volatile.Read(ref Version);
        for (var i = 0; i < Count; i++)
        {
            if (snapshot != Volatile.Read(ref Version)) throw new InvalidOperationException("Collection modified while enumerating.");
            yield return new Entity(_world, IdentityStorage[i]);
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
    public Entity this[int index] => new(_world, IdentityStorage[index]);


    #region Cross Joins
    internal Cross.Join<C0> CrossJoin<C0>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0>(this, streamTypes.AsSpan());
    }


    internal Cross.Join<C0, C1> CrossJoin<C0, C1>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1>(this, streamTypes);
    }


    internal Cross.Join<C0, C1, C2> CrossJoin<C0, C1, C2>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1, C2>(this, streamTypes);
    }


    internal Cross.Join<C0, C1, C2, C3> CrossJoin<C0, C1, C2, C3>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1, C2, C3>(this, streamTypes);
    }


    internal Cross.Join<C0, C1, C2, C3, C4> CrossJoin<C0, C1, C2, C3, C4>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1, C2, C3, C4>(this, streamTypes);
    }
    #endregion


    #region Inner Joins
    internal Cross.Join<C0> InnerJoin<C0>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0>(this, streamTypes.AsSpan());
    }


    internal Cross.Join<C0, C1> InnerJoin<C0, C1>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1>(this, streamTypes);
    }


    internal Cross.Join<C0, C1, C2> InnerJoin<C0, C1, C2>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1, C2>(this, streamTypes);
    }


    internal Cross.Join<C0, C1, C2, C3> InnerJoin<C0, C1, C2, C3>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1, C2, C3>(this, streamTypes);
    }


    internal Cross.Join<C0, C1, C2, C3, C4> InnerJoin<C0, C1, C2, C3, C4>(ImmutableArray<TypeExpression> streamTypes)
    {
        return IsEmpty ? default : new Cross.Join<C0, C1, C2, C3, C4>(this, streamTypes);
    }
    #endregion


    internal void Spawn(int count, object[] components)
    {
        using var worldLock = _world.Lock();
        
        var first = Count;
        
        foreach (var component in components)
        {
            var type = TypeExpression.Of(component.GetType(), fennecs.Identity.Plain);
            var storage = GetStorage(type);
            storage.Append(component, count);
        }
        
        using var identities = _world.SpawnBare(count); 
        IdentityStorage.Append(identities);
        PatchMetas(first, count);
    }

    internal void Spawn(int count, IReadOnlyList<TypeExpression> components, IReadOnlyList<object> values)
    {
        using var worldLock = _world.Lock();
        
        var first = Count;

        for (var i = 0; i < components.Count; i++)
        {
            var storage = GetStorage(components[i]);
            storage.Append(values[i], count);
        }

        using var identities = _world.SpawnBare(count);
        IdentityStorage.Append(identities);
        PatchMetas(first, count);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Invalidate() => Interlocked.Increment(ref Version);
}