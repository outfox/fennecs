---
title: Glossary
---

![a stylized fennec fox in a huge archive with of stacks of papers](https://fennecs.tech/img/fennec-glossary.png)


# Glossary of Terms
Feeling a little whelmed by all those new and weird terms that **fenn**ecs casually uses (and often makes up out of thin air)?

Here we hope to explain some of them a little more in depth, without creating a whole documentation chapter for each.


## Contains
We contextually sometimes say:

- "the Query contains" a set of Entities, and the base `Query` class also exposes these "contents" via a `IEnumerable<Entity>` interface. This is usable for world setup and in unit tests - but your game logic might want to do its heavy lifting with [Streams](/docs/Streams/index.md) and [SIMD](/docs/Streams/SIMD.md) operations.
- "the World contains", which can refer to both Entites and Archetypes.
- "the Archetype contains", which can refer both to the Entities that share this Archetype, but also the Types that constitute said Archetype.

## Fragmentation
Archetype Fragmentation is an intrinsic disadvantage of an archetype-based ECS, stemming directly from its greatest strength. 

::: info :neofox_magnify: BEHIND THE SCENES
Each *extant* combination of Components, Object Links, and Relations on an Entity will create an Archetype in the Type graph that houses all the Entities and their data for that specific combination of Type Expressions. For Links and Relations, each specific target and backing data type counts as its own unique Type Expression. Plain Components contribute in the same way, albeit to a lesser extent.
:::

::: danger :neofox_nom_fox_nervous: HOW DOES THAT BITE US?
Runners efficiently parallelize their work within and across Archetypes, but if each Archetype only contains a small number of Entities, eventually runtime and memory overheads will eat into these efficiency gains.

An indirect symptom of fragmentation can be that you might be performing frequent or many structural changes, possibly even heterogeneously (per-Entity). [Bulk and Batch Operations](/docs/Queries/CRUD.md#batch-operations) can help streamline these operations, and they also benefit greatly from larger Archetypes (i.e. less fragmentation).
:::


For instance adding a unique "name" string to each entity using an Object Link would create a single Archetype for each single Entity. Instead, you'd always add the "name" as a string component (or struct or class that contains a string).

A certain amount of Fragmentation is natural and often relatively harmless if you have low counts of entities (up to a few thousand), but it can become a detriment to performance when your Entity counts being processed each tick climbs into the hundreds of thousands.

### Mitigating Fragmentation
Mitigations for everyday fragmentation may include enabling/disabling components with a flag and just skipping over them in the runner code (if it's a minority of entities affected), but usually each use case will need custom optimizations when that time comes. 

It's also recommended to perform large bulk operations such as adding or removing components to a large number of Entities through the [Query CRUD](/docs/Queries/CRUD.md), instead of the [per-Entity CRUD](/docs/Entities/CRUD.md).

You'll likely get a long way before Archetype Fragmentation becomes a serious threat, but among the performance risks, this may quickly become the biggest one. 

You can examine your World's `DebugString()` to see how many you have, and how many Entities they contain, at any given time.

```cs
var myWorld = new World();
Console.WriteLine(myWorld.DebugString());
```

## Identity
An Identity is a 64-bit number. When associated with a World, the majority of Identities are called Entities. Identities can represent multiple things:
- a specific Entity (as itself or for targeting)
- a specific Object's Identity (for Link targeting)
- a Wildcard for a Query Filter (see [Match Expressions](/docs/Queries/Matching.md))

## Structural Changes

Changes to the layout of Entities - meaning which Components, Links, or Relations they have - define which [Archetype](/docs/Components/index.md#archetype) they falls into. 

### Each of these constitutes a structural change:
 - Adding a Component, Link, or Relation
 - Removing a Component, Link, or Relation
 - Despawning an Entity

### The special case of Spawning

- Spawning instantly returns a fully usable Entity builder struct... however, its Identity and Components will only be written to the World later, so it is invisible to Queries and even to `World.GetEntity` and `World.IsAlive` while a ==World Lock== is in effect.

### Reasons for Locking
Structural Changes will invariably cause data to be moved around internally, and sometimes internal data structures need to resize. This may cause individual Entities or even entire memory regions to be moved within their affected Archetype.

Structural changes to the world are deferred while a ==World Lock== is taken out, until _ALL_ locks are disposed. Once that happens, they are all applied in order of submission.

### Batch versus Individual Operations
When removing components, it's often good practice to perform structural changes in bulk through the [Query CRUD](/docs/Queries/CRUD.md) functions where possible. 


::: details :neofox_magnify: BEHIND THE SCENES
When a Structural Change is applied to an Entity, it is moved out of its current Archetype's Component data storage structures and appended to a new Archetype. The "hole" that is now in the original Archetype is plugged by copying the last Entity of that Archetype into the newly vacated spot, ensuring that Memory is always contiguous.

No need to manually remove a component from each entity in a Query, enqueing the change in the Deferred Operations Queue, and having the World execute them all afterwards (and in sub-optimal order, leading to additional, entirely unnecessary data copies).
:::


## World Lock

An `IDisposable` that can be aquired from a World to set it to Deferred Mode, meaning all structural changes are queued and executed only after the last lock has been returned.

All the Stream Runners use this internally to defer structural changes until the Runner has completed.

::: code-group

```cs [how to use it]
var myWorld = new World();
// using statement will ensure the lock is disposed when it goes out of scope.
using var worldLock1 = myWorld.Lock;

// or

using (var worldLock2 = myWorld.Lock)
{
    // do stuff
}
```
:::