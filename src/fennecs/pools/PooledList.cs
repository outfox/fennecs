using System.Collections.Concurrent;
// ReSharper disable StaticMemberInGenericType

namespace fennecs.pools;

/// <summary>
/// A pooled List implementation. Must be Disposed after use!
/// </summary>
/// <typeparam name="T">Type of the List elements.</typeparam>
internal class PooledList<T> : List<T>, IDisposable
{
    /// <summary>
    /// Starting capacity of a new instance.
    /// </summary>
    public static int DefaultInstanceCapacity = 64;
    
    /// <summary>
    /// Maximum capacity of a returned instance.
    /// </summary>
    public static int ReturnedInstanceCapacityLimit = 512;

    
    private static readonly ConcurrentBag<PooledList<T>> Recycled = [];
    

    private const int BagCapacity = 32;
    static PooledList()
    {
        for (var i = 0; i < BagCapacity; i++) Recycled.Add(new());
    }
    
    /// <summary>
    /// Rents a List from the Pool.
    /// </summary>
    /// <remarks>
    /// Use Dispose() to return it.
    /// </remarks>
    public static PooledList<T> Rent()
    {
        return Recycled.TryTake(out var list) ? list : new();
    }
    
    /// <summary>
    /// Clears the List and returns it to the Pool.
    /// </summary>
#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
    {
        Clear();
        Capacity = Math.Clamp(Capacity, DefaultInstanceCapacity, ReturnedInstanceCapacityLimit);
        Recycled.Add(this);
    }
    
    private PooledList() : base(DefaultInstanceCapacity)
    {
    }
}