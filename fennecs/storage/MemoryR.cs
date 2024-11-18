namespace fennecs.storage;

/// <summary>
/// Read-Only memory region, contiguously contains component data.
/// </summary>
/// <remarks>
/// You can get to the underlying <see cref="ReadOnlyMemory{T}"/> via property <see cref="ReadOnlyMemory"/>
/// </remarks>
/// <params name="ReadOnlyMemory">The underlying <see cref="ReadOnlyMemory{T}"/></params>
public readonly record struct MemoryR<T>(ReadOnlyMemory<T> ReadOnlyMemory) where T : notnull
{
    /// <summary>
    /// Read-Only access to the component data at the given index.
    /// </summary>
    /// <remarks>
    /// When making a large numbers of reads, consider caching span <see cref="read"/> instead.
    /// </remarks>
    public ref readonly T this[int index] => ref ReadOnlyMemory.Span[index];
    
    /// <summary>
    /// Access the the memory as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    public ReadOnlySpan<T> read => ReadOnlyMemory.Span;
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public ReadOnlySpan<T> Span => ReadOnlyMemory.Span;
    
    /// <inheritdoc cref="ReadOnlyMemory{T}.Length"/>
    public int Length => ReadOnlyMemory.Length;
}
