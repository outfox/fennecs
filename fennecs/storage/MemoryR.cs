using System.Runtime.CompilerServices;

namespace fennecs.storage;

/// <summary>
/// Read-Only memory region, contiguously contains component data.
/// </summary>
/// <remarks>
/// You can get to the underlying <see cref="ReadOnlyMemory{T}"/> via property <see cref="ReadOnlyMemory"/>
/// </remarks>
/// <params name="ReadOnlyMemory">The underlying <see cref="ReadOnlyMemory{T}"/></params>
public readonly record struct MemoryR<T> where T : notnull
{
    /// <summary>
    /// Read-Only memory region, contiguously contains component data.
    /// </summary>
    /// <remarks>
    /// You can get to the underlying <see cref="ReadOnlyMemory{T}"/> via property <see cref="ReadOnlyMemory"/>
    /// </remarks>
    /// <params name="ReadOnlyMemory">The underlying <see cref="ReadOnlyMemory{T}"/></params>
    public MemoryR(Storage<T> Storage, int Start, int Length)
    {
        storage = Storage;
        start = Start;
        length = Length;
    }

    /// <summary>
    /// Read-Only access to the component data at the given index.
    /// </summary>
    /// <remarks>
    /// When making a large numbers of reads, consider caching span <see cref="read"/> instead.
    /// </remarks>
    public ref readonly T this[int index] => ref storage.Span[index];
    
    /// <summary>
    /// Access the the memory as a <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    public ReadOnlySpan<T> read => storage.SubSpan(start, length);
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public ReadOnlySpan<T> Span => storage.SubSpan(start, length);
    
    /// <summary>
    /// Access the the memory as a <see cref="Span{T}"/>
    /// </summary>
    public ReadOnlyMemory<T> ReadOnlyMemory => storage.ActualMemory();
    
    /// <inheritdoc cref="ReadOnlyMemory{T}.Length"/>
    public int Length => length;

    public Storage<T> storage { get; init; }
    public int start { get; init; }
    public int length { get; init; }

    public void Deconstruct(out Storage<T> storage, out int start, out int length)
    {
        storage = this.storage;
        start = this.start;
        length = this.length;
    }
}
