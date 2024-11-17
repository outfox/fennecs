namespace fennecs.storage;

/// <summary>
/// Read-Write memory region, contiguously contains component data.
/// </summary>
/// <remarks>
/// You can get to the underlying <see cref="Memory{T}"/> via property <see cref="Memory"/>
/// </remarks>

public readonly record struct MemoryRW<T>(Memory<T> mem) where T : notnull
{
    /// <summary>
    /// The underlying <see cref="Memory{T}"/>
    /// </summary>
    public readonly Memory<T> Memory = mem;
    
    /// <summary>
    /// Read-Write access to the component data at the given index.
    /// </summary>
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
}