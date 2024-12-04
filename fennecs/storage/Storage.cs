using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using fennecs.pools;

namespace fennecs.storage;

/// <summary>
/// Generic Storage Interface (with boxing).
/// </summary>
public interface IStorage
{
    /// <summary>
    /// The TypeExpression of the components stored. (i.e. their backing type and secondary key)
    /// </summary>
    TypeExpression Expression { get; }
    
    /// <summary>
    /// The number of elements currently stored.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The backing type of the elements stored.
    /// </summary>
    Type Type { get; }
    
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
        var instance = (IStorage) Activator.CreateInstance(storageType, expression)!;
        return instance;
    }

    /// <summary>
    /// Returns the element at position Row as a boxed object.
    /// </summary>
    IStrongBox Box(int row);
    
    /// <summary>
    /// Gets the value at index as a boxed object.
    /// </summary>
    /// <remarks>
    /// Value Types are copied, then boxed.
    /// </remarks>
    /// <throws><see cref="IndexOutOfRangeException"/>if the row index is out of range</throws>
    object Get(int row);
}

/// <summary>
/// A front-end to System.Array for fast storage write and blit operations.
/// </summary>
/// <typeparam name="T">the type of the array elements</typeparam>
public class Storage<T> : IStorage where T : notnull
{
    internal Storage() : this(TypeExpression.Of<T>(default)) { }

    /// <summary>
    /// The backing type and secondary key of stored elements.
    /// </summary>
    internal readonly TypeExpression _expression;
    
    public TypeExpression Expression => _expression;
    
    private const int InitialCapacity = 32;
        
    private static readonly ArrayPool<T> Pool = ArrayPool<T>.Create();
    
    private T[] _data;
    private T[] _read;

    
    /// <summary>
    /// A front-end to System.Array for fast storage write and blit operations.
    /// </summary>
    /// <typeparam name="T">the type of the array elements</typeparam>
    public Storage(TypeExpression expression)
    {
        using var _ = PooledList<Storage<T>>.Rent();        
        _expression = expression;
        
        _data = Pool.Rent(InitialCapacity);
        _read = typeof(events.Commit).IsAssignableFrom(typeof(T)) ? Pool.Rent(InitialCapacity) : _data;
    }

    /// <summary>
    /// Replaces the value at the given index.
    /// (use <c>Append</c> to add a new one)
    /// </summary>
    public void Store(int index, T value) => Span[index] = value;
    
    /// <inheritdoc />
    public Type Type => typeof(T);
    
    /// <inheritdoc />
    public void Store(int index, object value) => Store(index, (T)value);


    /// <summary>
    /// Number of Elements actually stored.
    /// </summary>
    public int Count { get; private set; }


    /// <summary>
    /// Capacity of the underlying array (NOT the number of elements stored).
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

        var previous = _data;
        _data = Pool.Rent(newSize);
        previous.AsSpan(0, Count).CopyTo(_data);
        Pool.Return(previous);
    }

    /// <summary>
    /// Tries to downsize the storage to the smallest power of 2 that can contain all elements.
    /// </summary>
    public void Compact()
    {
        var newSize = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(InitialCapacity, Count));
        if (newSize == _data.Length) return; // nothing to do

        var previous = _data;
        _data = Pool.Rent(newSize);
        previous.AsSpan(0, Count).CopyTo(_data);
        Pool.Return(previous);
    }


    /// <summary>
    /// Migrates all the entries in this storage to the destination storage.
    /// </summary>
    /// <param name="destination">a storage of the same type</param>
    public void Migrate(Storage<T> destination)
    {
        destination.Append(Span);
        Clear();

        // TODO: This will get changed with Chunks, so chunks can be moved instead of copied. 
        // TODO: This is a older, potentially huge optimization, but it struggles with backfill logic. 
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
    /// Gets the value at index as a <see cref="IStrongBox"/>.
    /// </summary>
    /// <remarks>
    /// Value Types are copied, then boxed.
    /// </remarks>
    public IStrongBox Box(int row) => new StrongBox<T>(Span[row]);
    
    
    /// <summary>
    /// Gets the value at index as a boxed object.
    /// </summary>
    /// <remarks>
    /// Value Types are copied, then boxed.
    /// </remarks>
    public object Get(int row) => Span[row];
    
    
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

    /// <summary>
    /// Returns a Memory handle to a section of the contained data.
    /// </summary>
    public Memory<T> ActualMemory() => _data.AsMemory(0, Count);
    
    //public ReadOnlyMemory<T> ActualMemory() => _data.AsMemory(0, Count);
    
    /// <summary>
    /// Returns a ReadOnlyMemory handle to a section of the contained data.
    /// </summary>
    public MemoryR<T> AsReadOnlyMemory(int start, int length) => new(this, start, length);

    /// <summary>
    /// Returns a ReadOnlyMemory handle to the entire contained data.
    /// </summary>
    public MemoryR<T> AsReadOnlyMemory() => new(this, 0, Count);

    /// <summary>
    /// Returns a Memory handle to the entire contained data.
    /// </summary>
    public MemoryRW<T> AsMemory() => new(this, 0, Count);

    /// <summary>
    /// Returns a Memory handle to the entire contained data.
    /// </summary>
    public MemoryRW<T> AsMemory(int start, int count) => new(this, start, count);

    /// <summary>
    /// Returns a span representation of the actually contained data.
    /// </summary>
    public Span<T> Span => _data.AsSpan(0, Count);

    /// <summary>
    /// Returns a span representation of the actually contained data.
    /// </summary>
    public ReadOnlySpan<T> ReadOnlySpan => _data.AsSpan()[..Count];

    private Span<T> FullSpan => _data.AsSpan();

    /// <summary>
    /// Indexer (for debug purposes!)
    /// </summary>
    /// <remarks>
    /// Allows inspection of the entire array, not just the used elements.
    /// </remarks>
    internal ref T this[int index] => ref _data[index];

    /// <summary>
    /// Cast to <see cref="Span{T}"/> implicitly.
    /// </summary>
    public static implicit operator Span<T>(Storage<T> self) => self.Span;
}