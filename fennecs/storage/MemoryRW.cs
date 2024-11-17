namespace fennecs.storage;

/// <summary>
/// Read-Write memory region, contiguously contains component data.
/// </summary>
/// <remarks>
/// You can get to the underlying <see cref="Memory{T}"/> via property <see cref="Memory"/>
/// </remarks>
/// <params name="Memory">The underlying <see cref="Memory{T}"/></params>

public readonly record struct MemoryRW<T>(Memory<T> Memory) where T : notnull
{
    /// <summary>
    /// Read-Write access to the component data at the given index.
    /// </summary>
    /// <remarks>
    ///When making a large number of reads/writes, consider caching spans <see cref="read"/> or <see cref="write"/> instead.
    /// </remarks>
    public ref T this[int index] => ref Memory.Span[index];
    
    /// <summary>
    /// Access the the memory as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    public ReadOnlySpan<T> read => Memory.Span;
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public Span<T> write => Memory.Span;

    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    internal Span<T> Span => Memory.Span;
    
    /// <inheritdoc cref="Memory{T}.Length"/>
    public int Length => Memory.Length;
}