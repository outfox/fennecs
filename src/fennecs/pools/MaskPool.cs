// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace fennecs.pools;

internal static class MaskPool
{
    internal static readonly ConcurrentBag<Mask> Pool = [];
    
    public static Mask Rent() => Pool.TryTake(out var mask) ? mask : new();
    
    
    public static void Return(Mask mask)
    {
        mask.Clear();
        Pool.Add(mask);
    }


    static MaskPool()
    {
        for (var i = 0; i < 32; i++)
        {
            Return(new());
        }
    }
}