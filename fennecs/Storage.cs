using System.Numerics;

namespace fennecs;

/// <summary>
/// Generic Storage Interface (with boxing).
/// </summary>
internal interface IStorage
{
    int Count { get; }

    /// <summary>
    /// Adds a boxed value (or number of identical values) to the storage.
    /// </summary>
    void Append(object value, int additions = 1);

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
    /// Instantiates the appropriate Storage for a <see cref="TypeExpression"/>.
    /// </summary>
    /// <param name="expression">a typeexpression</param>
    /// <returns>generic IStorage reference backed by the specialized instance of the <see cref="Storage{T}"/></returns>
    public static IStorage Instantiate(TypeExpression expression)
    {
        var storageType = typeof(Storage<>).MakeGenericType(expression.Type);
        var instance = (IStorage) Activator.CreateInstance(storageType)!;
        if (instance == null) throw new InvalidOperationException($"Could not instantiate Storage for {expression}");
        return instance;
    }
}

/// <summary>
/// A front-end to System.Array for fast storage write and blit operations.
/// </summary>
/// <typeparam name="T">the type of the array elements</typeparam>
internal class Storage<T>(int initialCapacity = 16) : IStorage
{
    private T[] _data = new T[initialCapacity];

    /// <summary>
    /// Number of Elements actually stored.
    /// </summary>
    public int Count { get; private set; }

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
        Span[(index + removals)..].CopyTo(Span[index..]);

        //Only clear subsection (this could be very large free space!)
        Span[(Count - removals)..Count].Clear();
        Count -= removals;
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
        var newSize = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(initialCapacity, Count));
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
    }

    /// <summary>
    /// Boxed / General migration method.
    /// </summary>
    /// <param name="destination">a storage, must be of the same type</param>
    public void Migrate(IStorage destination) => Migrate((Storage<T>)destination);


    private void Append(Span<T> appendage)
    {
        EnsureCapacity(Count + appendage.Length);
        appendage.CopyTo(FullSpan[Count..]);
        Count += appendage.Length;
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
}