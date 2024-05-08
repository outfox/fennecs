using System.Collections.Concurrent;

namespace fennecs.pools;

internal class PooledList<T> : List<T>, IDisposable
{
    private const int BagCapacity = 16;
    private const int DefaultInstanceCapacity = 64;
    private const int ReturnedInstanceCapacityLimit = 256;

    private static readonly ConcurrentBag<PooledList<T>> Recycled = [];


    static PooledList()
    {
        for (var i = 0; i < BagCapacity; i++) Recycled.Add(new PooledList<T>());
    }


    public static PooledList<T> Rent()
    {
        return Recycled.TryTake(out var list) ? list : new PooledList<T>();
    }


    public void Dispose()
    {
        Clear();
        Capacity = Math.Clamp(Capacity, DefaultInstanceCapacity, ReturnedInstanceCapacityLimit);

        Recycled.Add(this);
    }


    private PooledList() : base(DefaultInstanceCapacity)
    {
    }
}