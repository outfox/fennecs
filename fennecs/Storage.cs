using System.Numerics;
using fennecs.pools;

namespace fennecs;

/// <summary>
/// Generic Storage Interface (with boxing).
/// </summary>
internal interface IStorage
{
    int Count { get; }

    /// <summary>
    /// Stores a boxed value at the given index.
    /// (use <c>Append</c> to add a new one)
    /// </summary>
    void Store(int index, object value);

    /// <summary>
    /// Adds a boxed value (or number of identical values) to the storage.
    /// </summary>
    void Append(object value, int additions = 1);

    /// <summary>
    /// Removes a range of elements. 
    /// </summary>
    void Delete(int index, int removals = 1);

    /// <summary>
    /// Writes the given boxed value over all elements of the storage.
    /// </summary>
    /// <param name="value"></param>
    void Blit(object value);

    /// <summary>
    /// Clears the entire storage.
    /// </summary>
    void Clear();

    /// <summary>
    /// Ensures the storage has the capacity to store at least the given number of elements.
    /// </summary>
    /// <param name="capacity">the desired minimum capacity</param>
    void EnsureCapacity(int capacity);

    /// <summary>
    /// Tries to downsize the storage to the smallest power of 2 that can contain all elements.
    /// </summary>
    void Compact();

    /// <summary>
    /// Moves all elements from this storage into destination.
    /// The destination must be the same or a derived type.
    /// </summary>
    /// <param name="destination">a storage of the type of this storage</param>
    void Migrate(IStorage destination);

    /// <summary>
    /// Moves one element from this storage to the destination storage. 
    /// </summary>
    /// <param name="index">element index to move</param>
    /// <param name="destination">a storage of the same type</param>
    void Move(int index, IStorage destination);

    /// <summary>
    /// Instantiates the appropriate Storage for a <see cref="TypeExpression"/>.
    /// </summary>
    /// <param name="expression">a typeexpression</param>
    /// <returns>generic IStorage reference backed by the specialized instance of the <see cref="Storage{T}"/></returns>
    public static IStorage Instantiate(TypeExpression expression)
    {
        var storageType = typeof(Storage<>).MakeGenericType(expression.Type);
        var instance = (IStorage) Activator.CreateInstance(storageType)!;
        return instance;
    }
}

/// <summary>
/// A front-end to System.Array for fast storage write and blit operations.
/// </summary>
/// <typeparam name="T">the type of the array elements</typeparam>
internal class Storage<T> : IStorage
{
    private const int InitialCapacity = 2;
        
    private T[] _data = new T[InitialCapacity];

    /// <summary>
    /// Replaces the value at the given index.
    /// (use <c>Append</c> to add a new one)
    /// </summary>
    public void Store(int index, T value)
    {
        Span[index] = value;
    }


    /// <inheritdoc />
    public void Store(int index, object value) => Store(index, (T)value);


    /// <summary>
    /// Number of Elements actually stored.
    /// </summary>
    public int Count { get; private set; }


    /// <summary>
    /// Number of Elements actually stored.
    /// </summary>
    public int Capacity => _data.Length;


    /// <summary>
    /// Adds a value (or number of identical values) to the storage.
    /// </summary>
    public void Append(T value, int additions = 1)
    {
        if (additions <= 0) return;
        EnsureCapacity(Count + additions);
        FullSpan.Slice(Count, additions).Fill(value);
        Count += additions;
    }


    /// <summary>
    /// Adds a boxed value (or number of identical values) to the storage.
    /// </summary>
    public void Append(object value, int additions = 1) => Append((T)value, additions);



    /// <summary>
    /// Removes a range of items from the storage.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="removals"></param>
    public void Delete(int index, int removals = 1)
    {
        if (removals <= 0) return;

        // Are there enough elements after the removal site to fill the gap created?
        if (Count - removals > index + removals)
        {
            // Then copy just these elements to the site of removal!
            FullSpan[(Count - removals)..Count].CopyTo(FullSpan[index..]);
        }
        else if (Count > index + removals)
        {
            // Else shift back all remaining elements (if any).
            FullSpan[(index + removals)..Count].CopyTo(FullSpan[index..]);
        }

        // Clear the space at the end.
        FullSpan[(Count - removals)..Count].Clear();
        
        Count -= removals;

        /*
        // Naive Wasteful: Shift EVERYTHING backwards.
        FullSpan[(index + removals)..].CopyTo(FullSpan[index..]);
        if (Count < _data.Length) FullSpan[Count..].Clear();
        */        
    }


    /// <summary>
    /// Writes the given value over all elements of the storage.
    /// </summary>
    /// <param name="value"></param>
    public void Blit(T value)
    {
        Span[..Count].Fill(value);
    }


    /// <summary>
    /// Writes the given boxed value over all elements of the storage.
    /// </summary>
    /// <param name="value"></param>
    public void Blit(object value) => Blit((T)value);


    /// <summary>
    /// Clears the entire storage.
    /// </summary>
    public void Clear()
    {
        if (Count <= 0) return;
        
        Span.Clear();
        Count = 0;
    }

    /// <summary>
    /// Ensures the storage has the capacity to store at least the given number of elements.
    /// </summary>
    /// <param name="capacity">the desired minimum capacity</param>
    public void EnsureCapacity(int capacity)
    {
        var newSize = (int)BitOperations.RoundUpToPowerOf2((uint)capacity);
        if (newSize <= _data.Length) return;
        Array.Resize(ref _data, newSize);
    }

    /// <summary>
    /// Tries to downsize the storage to the smallest power of 2 that can contain all elements.
    /// </summary>
    public void Compact()
    {
        var newSize = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(InitialCapacity, Count));
        Array.Resize(ref _data, newSize);
    }


    /// <summary>
    /// Migrates all the entries in this storage to the destination storage.
    /// </summary>
    /// <param name="destination">a storage of the same type</param>
    public void Migrate(Storage<T> destination)
    {
        destination.Append(Span);
        Clear();

        // TODO: This is a potentially huge optimization, but it struggles with backfill logic. 
        // (i.e. what if there's nothing to migrate yet, but we are going to need to backfill?)
        // (and what's the case for swap vs. copy?)
        // (and despite saving CPU on the copy, Meta updates will be much more expensive)
        // (meaning there's not a direct correlation between Storage size and efficiency)
        /*
        if (destination.Count >= Count)
        {
            destination.Append(Span);
        }
        else
        {
            // In many cases, we're migrating a much larger Archetype/Storage into a smaller
            // or empty one. We then just perform the copy operation the other way around...
            Append(destination.Span);

            // ... and we just switch counts and data pointers for this storage.
            (_data, destination._data) = (destination._data, _data);
            (Count, destination.Count) = (destination.Count, Count);
        }

        // We are still the "source" archetype, so we are expected to be empty (and we do the emptying)
        Clear();
        */
    }

    /// <summary>
    /// Moves one element from this storage to the destination storage.
    /// </summary>
    /// <param name="index">element index to move</param>
    /// <param name="destination">a storage of the same type</param>
    public void Move(int index, Storage<T> destination)
    {
        destination.Append(Span[index]);
        Delete(index);
    }

    /// <inheritdoc/>
    public void Move(int index, IStorage destination) => Move(index, (Storage<T>)destination);

    /// <summary>
    /// Boxed / General migration method.
    /// </summary>
    /// <param name="destination">a storage, must be of the same type</param>
    public void Migrate(IStorage destination) => Migrate((Storage<T>)destination);


    /// <summary>
    /// Used for memory-efficient spawning
    /// </summary>
    /// <param name="values">PooledList as gotten from <see cref="World.SpawnBare"/></param>
    internal void Append(PooledList<T> values)
    {
        EnsureCapacity(Count + values.Count);
        values.CopyTo(FullSpan[Count..]);
        Count += values.Count;    
    }
    
    private void Append(Span<T> appendage)
    {
        EnsureCapacity(Count + appendage.Length);
        appendage.CopyTo(FullSpan[Count..]);
        Count += appendage.Length;
    }

    public Memory<T> AsMemory(int start, int length)
    {
        return _data.AsMemory(start, length);
    }

    /// <summary>
    /// Returns a span representation of the actually contained data.
    /// </summary>
    public Span<T> Span => _data.AsSpan(0, Count);

    private Span<T> FullSpan => _data.AsSpan();

    /// <summary>
    /// Indexer (for debug purposes!)
    /// </summary>
    /// <remarks>
    /// Allows inspection of the entire array, not just the used elements.
    /// </remarks>
    internal T this[int index] => _data[index];

    /// <summary>
    /// Cast to <see cref="Span{T}"/> implicitly.
    /// </summary>
    public static implicit operator Span<T>(Storage<T> self) => self.Span;
}