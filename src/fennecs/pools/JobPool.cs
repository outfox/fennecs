// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace fennecs.pools;

internal static class JobPool<T> where T : class, new()
{
    private static readonly ConcurrentBag<T> Pool = [];

    static JobPool()
    {
        for (var i = 0; i < 32; i++) Pool.Add(new T());
    }


    public static T Rent()
    {
        return Pool.TryTake(out var job) ? job : new T();
    }


    public static void Return(T job)
    {
        Pool.Add(job);
    }


    public static void Return(List<T> jobs)
    {
        foreach (var job in jobs) Return(job);
        jobs.Clear();
    }
}