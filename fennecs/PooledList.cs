using System.Collections.Concurrent;

namespace fennecs;

public class PooledList<T> : List<T>, IDisposable
{
    private const int BagCapacity = 16;
    private const int DefaultInstanceCapacity = 64;
    private const int ReturnedInstanceCapacityLimit = 256;
    
    private static readonly ConcurrentBag<PooledList<T>> Bag = [];

    static PooledList()
    {
        for (var i = 0; i < BagCapacity; i++) Bag.Add(new PooledList<T>());
    }
    
    public static PooledList<T> Rent()
    {
        return Bag.TryTake(out var list) ? list : new PooledList<T>();
    }

    public void Dispose()
    {
        Clear();
        Capacity = Math.Clamp(Capacity, DefaultInstanceCapacity, ReturnedInstanceCapacityLimit);
        Bag.Add(this);
    }

    private PooledList() : base(DefaultInstanceCapacity)
    {
        
    }
}