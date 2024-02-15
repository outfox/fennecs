// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace fennecs;

public static class ListPool<T>
{
    private static readonly ConcurrentBag<List<T>> Bag = [];
    private const int Capacity = 32;
    
    public static List<T> Rent()
    {
        return Bag.TryTake(out var list) ? list : new List<T>(Capacity);
    }
    
    public static void Return(List<T> list)
    {
        list.Clear();
        Bag.Add(list);
    }

    static ListPool()
    {
        for (var i = 0; i < 128; i++) Bag.Add(new List<T>(Capacity));
    }
}