namespace fennecs.storage;

/// <summary>
/// Read-Only memory region, contiguously contains component data.
/// </summary>
/// <remarks>
/// You can get to the underlying <see cref="ReadOnlyMemory{T}"/> via property <see cref="memory"/>
/// </remarks>
public readonly struct MemoryR<T>(ReadOnlyMemory<T> mem) where T : notnull
{
    /// <summary>
    /// The underlying <see cref="ReadOnlyMemory{T}"/>
    /// </summary>
    public readonly ReadOnlyMemory<T> memory = mem;
    
    /// <summary>
    /// Read-Only access to the component data at the given index.
    /// </summary>
    public ref readonly T this[int index] => ref memory.Span[index];
    
    /// <summary>
    /// Access the the memory as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    public ReadOnlySpan<T> read => memory.Span;
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    internal ReadOnlySpan<T> Span => memory.Span;
}
