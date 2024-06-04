---
title: 1,2,3 Numbering Entities
outline: [2, 3]
order: 1
---

# Numbering Entities (with an Index)

What we found while working with ECS was surprisingly often that we needed to assign a not only unique (for that, you could just use the Entity struct), but also contiguous index to all our Entities, for example to sample noise or functions for each Entity deterministically.
**fenn**ecs makes this possible in a variety of way, depending on your needs.

### Preparation

First, let's define the components we'll need:

::: code-group
```csharp [Index Component]
// We use a super-lightweight, record struct as our index component
record struct Index(int Value);
```
```csharp [(or) Fancy Index]
// We use a super-lightweight, record struct as our index component
record struct Index(int Value)
{    
    public static IEnumerator<Index> Ascending(int from = 0, int to = int.MaxValue)
    {   // This (optional) Enumerator yields arbitrary, persistible index ranges
        while (from < to) yield return new(from++); 
    }
}
```
```csharp [(then) Test Data]
// Simply spawn some entities with zero-initialized Index components
var world = new World();
world.Entity()
    .Add<Index>(default)
    .Spawn(99_999)
    .Dispose();
```
:::

### Grab your Fork and Stream
Make your ~~order~~ Query! There's a neat shorthand to get a Stream View since **fenn**ecs 0.5.x!
```csharp
var stream = world.Stream<Index>();
```

### Indexer coming right up!
... lots of variety in our diet, and probably we can cook up a dozen more.
::: code-group

```csharp [Enumerator (fancy)]
// This is the cleanest way and overall has good characteristics.
stream.For(
    uniform: Index.Ascending(from: 0),
    action: static (IEnumerator<Index> enumerator, ref Index index) =>
    {
        enumerator.MoveNext();
        index = enumerator.Current;
    }
);
// The method needs the fancy enumerator, e.g. as extension method to Index.
// Bonus: We can also keep the iterator to auto-number Entities spawned later!
// (if we spawn 1-by-1, or place them in an interstitial Archetype)
```

```csharp [Closure]
// This is the shortest way. It's so simple that we can't help but love it.
var i = 0; 
stream.For((ref Index index) => index = new(i++));

// Not only that, but YOU actually clicked here to read this. You earned this!

// Tiny Boilerplate, good speed, low memory allocations... what's not to love?
// Sure, loosey-goosey integers and closures can be spoopy in program design.
// But we are all adults here. Some like their food plain, some like it spicy.
```

```csharp [Enumerator (store-bought)]
// LINQ is fine! No need to augment your Type, take an ad-hoc enumerator.
using var range = 
    Enumerable.Range(0, count).Select(i => new Index(i)).GetEnumerator();

stream.For(
    uniform: range,
    action: static (IEnumerator<Index> enumerator, ref Index index) =>
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