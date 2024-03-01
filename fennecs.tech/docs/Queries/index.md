---
title: Queries
layout: doc
---

# Queries

A Query is a view into a World, representing a subset of its Entities. It remains associated with this specific World, and Queries can not bridge multiple Worlds.

Queries use [Filter Expressions](FilterExpressions.md) to define this subset.

::: info :neofox_magnify: Behind the Scenes
Each Query object maintains a collection of all Archetypes it matches, and when iterating Components or enumerating Entities, the Query does so *for each Archetype*, in deterministic order.

Whenever a new Archetype materializes, the World will notify _all matching Queries_ of its existence.
:::

## Processing Data in Queries

Each Query with one or more [Stream Type](StreamTypes.md) offer a set of ==Runners== to execute code in batches or parallelized on all Entities in the Query.

#### Here's a mnemonic cheat sheet, follow the links in the paragraph headlines for details.

::: info THE CLASSIC
# [`For`](Query.For.md) / [`For<U>`](Query.For.md) 
One work item at a time. Call a [`RefAction`](Delegates.md#refaction-and-refactionu) delegate for each Entity in the query, providing the Components that match the ==Stream Types== as `ref` to the code.  
:neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_nom_waffle:
:::

::: info THE WORKHORSE
# [`Job`](Query.Job.md) / [`Job<U>`](Query.Job.md) 
Many items, multi-threaded. Takes a [`RefAction`](Delegates.md#refaction-and-refactionu) delegate and instantly schedules and executes the workload split into chunks, calling many times in parallel across CPU cores.  
:neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle:
:::

::: danger THE FOOTGUN
#  [`Raw`](Query.Raw.md) / [`Raw<U>`](Query.Raw.md) 
Work items as contiguous memory. Using a distinct signature, [`MemoryAction`](Delegates.md#memoryaction-and-memoryactionu), delivers the *entire workload* of each Archetype diriectly into your ~~fox~~ delegate via a single call.
:neofox_waffle_long_blurry::neofox_scream_stare:
:::

## Cleaning up
### Entities in Bulk
Use the method `Query.Clear()` to despawn all Entities in that Query.
Alternatively, use `Query.Truncate(int, TruncateMode)` to cut your Query down to a specific size.

### Queries themselves
Queries have a modest memory footprint, but in Worlds with many fragmented Archetypes, the cost of Query update notifications can add up! Call `Query.Dispose()` to de-register a query from its World and free its resources.

