---
title: Numbering Entities
outline: [2, 3]
---

# Numbering Entities (with an Index)

What we found while working with ECS was surprisingly often that we needed to assign a unique, contiguous index to all our Entities.
**fenn**ecs makes this possible in a variety of way, depending on your needs. Here are a few examples:

### Preparation

First, let's define the components we'll need:

::: code-group
```csharp [Index Component]
// We use a super-lightweight, record struct as our index component
private record struct Index(int Value);

// records and record structs are a C# 9 feature that makes it easy
// to define simple, expressive, type-safe data types.
```
```csharp [(or) Fancy Index]
// We use a super-lightweight, record struct as our index component
private record struct Index(int Value)
{    
    public static IEnumerator<Index> Ascending(int from = 0, int to = int.MaxValue)
    {   // This (optional) Enumerator yields arbitrary, persistible index ranges
        while (from < to) yield return new(from++); 
    }
}
```
```csharp [Spawn Test Data]
// Simply spawn some entities with zero-initialized Index components
var world = new World();
world.Entity()
    .Add<Index>(default)
    .Spawn(99_999)
    .Dispose();
```
:::

### Grab your Fork and Stream
```csharp
// This is shorthand for a stream query.
var stream = world.Stream<Index>();
```

### Indexer running its (Main) Course
::: code-group

```csharp [Closure]
// This is the shortest way. It's so simple that we won't argue against it.
var i = 0; 
stream.For((ref Index index) => index = new(i++));

// Tiny Boilerplate, good speed, low memory allocations... what's not to love?
// But loosey-goosey integers and closures can be spoopy. You have been warned.
```

```csharp [uniform + Fancy]
// This is the cleanest way and overall has good characteristics.
stream.For(
    uniform: Index.Ascending(from: 0),
    action: static (ref Index index, IEnumerator<Index> enumerator) =>
    {
        enumerator.MoveNext();
        index = enumerator.Current;
    }
);

// ... but it needs the fancy enumerator, e.g. as extension method to Index
```

```csharp [LINQ Enumerator]
// It is fine if you don't want to augment your Type and want an ad-hoc enumerator.
using var range = 
    Enumerable.Range(0, count).Select(i => new Index(i)).GetEnumerator();

stream.For(
    uniform: range,
    action: static (ref Index index, IEnumerator<Index> enumerator) =>
    {
        enumerator.MoveNext();
        index = enumerator.Current;
    }
);
```

```csharp [Concurrent Queue]
// This way works with Jobs but the order is not deterministic!
var queue = new ConcurrentQueue<Index>(Enumerable.Range(0, stream.Count)
    .Select(i => new Index(i)));

stream.Job((ref Index index) => queue.TryDequeue(index));

// Queue could also be passed as uniform. It's so chunky as a
// data object that it does not matter at all. 
// The meager parallelization gains are likely not worth it.
```

```csharp [Raw Loop]
// Fast(-ish) only for people who relish in the caca-poopoo-ness of ints.
var i = 0; 
stream.Raw(indices => 
{   //This foreach is extremely fast, and the JIT loves it more than its siblings.
    foreach (ref var index in indices.Span) index = new Index(i++);
});

// NB: If you had, perchance, a large block of numbers in memory, you could
// memcpy those into the Memory<Index> inside of Raw. That'd be... FAST!
```
::: 

### Bon Appétit!
::: danger THREAD UNSAFE
None of these numbering mechanisms are truly thread safe; and it is actually difficult to make them deterministic in a multi-threaded environment.

In other words, do not submit them as a `Job`... because note that your Runner might iterate multiple archetypes across multiple cores.
:::