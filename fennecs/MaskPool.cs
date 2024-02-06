// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace fennecs;

public static class MaskPool
{
    private static readonly ConcurrentBag<Mask> Pool = [];
    
    public static Mask Get()
    {
        return Pool.TryTake(out var mask) ? mask : new Mask();
    }

    public static void Add(Mask mask)
    {
        mask.Clear();
        Pool.Add(mask);
    }
}