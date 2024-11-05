// SPDX-License-Identifier: Unlicense
// Inspired by https://okyrylchuk.dev/blog/when-to-use-frozen-collections-in-dotnet/

using System.Collections.Frozen;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Conceptual;

[ShortRunJob]
public class CollectionsBenchmarks
{
    [Params(1000)]
    public int CollectionSize { get; set; }

    [Params(1, 2, 10, 100)]
    public int MissRatio { get; set; }

    private List<int> _list = null!;
    private SortedList<int, int> _sortedList = null!;
    private ImmutableList<int> _immutableList = null!;
    private HashSet<int> _hashSet = null!;
    private ImmutableHashSet<int> _immutableSet = null!;
    private ImmutableSortedSet<int> _immutableSortedSet = null!;
    private FrozenSet<int> _frozenSet = null!;

    private Random _random = null!;

    [IterationSetup]
    public void SetUpIteration()
    {
        _random = new(69);
    }

    [GlobalSetup]
    public void SetUp()
    {
        _random = new(69);

        _list = Enumerable.Range(0, CollectionSize).ToList();
        _immutableList = Enumerable.Range(0, CollectionSize).ToImmutableList();
        _sortedList = new();
        foreach (var i in Enumerable.Range(0, CollectionSize))_sortedList.Add(i, i);
        _hashSet = Enumerable.Range(0, CollectionSize).ToHashSet();
        _immutableSet = Enumerable.Range(0, CollectionSize).ToImmutableHashSet();
        _immutableSortedSet = Enumerable.Range(0, CollectionSize).ToImmutableSortedSet();
        _frozenSet = Enumerable.Range(0, CollectionSize).ToFrozenSet();
    }

    [Benchmark(Baseline = true)]
    public bool LookupList()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _list.Contains(_random.Next(CollectionSize * MissRatio));
        return found;
    }

    [Benchmark]
    public bool LookupImmutableList()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _immutableList.Contains(_random.Next(CollectionSize * MissRatio));
        return found;
    }
    
    [Benchmark]
    public bool LookupSortedList()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _sortedList.ContainsKey(_random.Next(CollectionSize * MissRatio));
        return found;
    }

    [Benchmark]
    public bool LookupHashSet()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _hashSet.Contains(_random.Next(CollectionSize * MissRatio));
        return found;
    }

    [Benchmark]
    public bool LookupImmutableSet()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _immutableSet.Contains(_random.Next(CollectionSize * MissRatio));
        return found;
    }
    
    [Benchmark]
    public bool LookupImmutableSortedSet()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _immutableSortedSet.Contains(_random.Next(CollectionSize * MissRatio));
        return found;
    }

    [Benchmark]
    public bool LookupFrozenSet()
    {
        var found = false;
        for (var i = 0; i < CollectionSize; i++)
            found ^= _frozenSet.Contains(_random.Next(CollectionSize * MissRatio));
        return found;
    }
}
