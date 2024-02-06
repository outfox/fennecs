// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace fennecs;

public static class ListPool<T>
{
    private static readonly ConcurrentBag<List<T>> Bag = [];
    
    public static List<T> Get()
    {
        return Bag.TryTake(out var list) ? list : [];
    }
    
    public static void Add(List<T> list)
    {
        list.Clear();
        Bag.Add(list);
    }
}