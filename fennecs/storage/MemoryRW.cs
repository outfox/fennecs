namespace fennecs.storage;

/// <summary>
/// Read-Write memory region, contiguously contains component data.
/// </summary>
/// <remarks>
/// You can get to the underlying <see cref="Memory{T}"/> via property <see cref="Memory"/>
/// </remarks>
/// <params name="Memory">The underlying <see cref="Memory{T}"/></params>

public readonly record struct MemoryRW<T> where T : notnull
{
    /// <summary>
    /// Read-Write memory region, contiguously contains component data.
    /// </summary>
    /// <remarks>
    /// You can get to the underlying <see cref="Memory{T}"/> via property <see cref="Memory"/>
    /// </remarks>
    /// <params name="Memory">The underlying <see cref="Memory{T}"/></params>
    public MemoryRW(Storage<T> Storage, int Start, int Length)
    {
        start = Start;
        length = Length;
        storage = Storage;
    }

    /// <summary>
    /// Read-Write access to the component data at the given index.
    /// </summary>
    /// <remarks>
    ///When making a large number of reads/writes, consider caching spans <see cref="read"/> or <see cref="write"/> instead.
    /// </remarks>
    public ref T this[int index] => ref storage.Span[index];

    /// <summary>
    /// Access the the memory as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    public ReadOnlySpan<T> read => storage.Span.Slice(start, length);
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public Span<T> write => storage.Span.Slice(start, length);

    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public Span<T> Span => storage.Span.Slice(start, length);
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public Memory<T> Memory => storage.ActualMemory();
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public ReadOnlyMemory<T> ReadOnlyMemory => storage.ActualMemory();


    /// <inheritdoc cref="Memory{T}.Length"/>
    public int Length => length;

    public Storage<T> storage { get; init; }
    public int start { get; init; }
    public int length { get; init; }
}