---
title: Membership
order: 2
outline: [1, 2]
description: 'How Entities join and leave fennecs Aspects - lazy membership, eviction, the Main Aspect, EntitySpawners, Batches, deferred mode, and despawning.'
---

# Membership & Lifecycle

*club rules: bring a Component, stay for the party*

All Aspects of a World share the same Entities  –  an Entity is one identity that can have component data in several Aspects at once. Membership is simply "does this Entity currently have data here?"

## Lazy Membership

Entities don't join an Aspect until they actually bring something to store. A freshly spawned Entity is a member of `Main` and nothing else:

```csharp
var world = new World();
var visuals = world.AddAspect("visuals").Owns<Position>().Owns<Matrix>();
var game = world.AddAspect("game").Owns<CrewData>();

var entity = world.Spawn();
// world.Count == 1, visuals.Count == 0, game.Count == 0

entity.Add(new Position(1, 2)); // joins "visuals"
// visuals.Count == 1

entity.Add(new CrewData(5));    // also joins "game"
// game.Count == 1
```

`Aspect.Count` counts member *Entities*, not components  –  an Entity with both `Position` and `Matrix` is still just one very stylish member of `"visuals"`.

## Eviction (and Rejoining)

Removing an Entity's *last* owned component evicts it from that Aspect. Removing merely *one of several* does not:

```csharp
var entity = world.Spawn().Add(new Position(1, 2)).Add(new Matrix(7));

entity.Remove<Position>(); // still a member - Matrix remains
entity.Remove<Matrix>();   // evicted from "visuals"

// but very much alive, and still a member of Main:
Console.WriteLine(world.IsAlive(entity)); // True

entity.Add(new Position(5, 6)); // rejoins "visuals" - no hard feelings
```

::: info :neofox_hug: NO FOX LEFT BEHIND
Eviction never despawns. An Aspect losing interest in an Entity is a structural change, not a death sentence  –  the Entity keeps living in `Main` (and any other Aspect it has data in).
:::

## Everyone lives in Main

Every living Entity is a member of `Main`, always  –  even one whose components live entirely in *other* Aspects. `world.Count` (which is `Main`'s count) is therefore always your total population, while `aspect.Count` counts that Aspect's members only.

## Spawners, Batches & Deferred Mode

Bulk operations are fully Aspect-aware, and mostly Just Work™:

**[EntitySpawners](/docs/Advanced/BulkSpawn.md)** split their template across Aspects automatically  –  no special ceremony needed:

```csharp
world.Entity()
    .Add(new Position(1, 2))   // -> "visuals"
    .Add(new CrewData(5))      // -> "game"
    .Spawn(100)
    .Dispose();

// world.Count == 100, visuals.Count == 100, game.Count == 100
```

**[Batches](/docs/Queries/CRUD.md#batch-operations)** operate within a Query  –  and since a Query belongs to a single Aspect *(more on that in [Queries & Streams](Queries.md))*, a Batch may only add or remove types of that same Aspect:

::: warning :neofox_peek_knife: ONE ASPECT PER BATCH
```csharp
var query = game.Query<CrewData>().Compile();

query.Batch().Add(new Cargo(50)).Submit(); // ✅ Cargo lives in "game" too

query.Batch().Add(new Position(1, 2));     // 💥 InvalidOperationException -
                                           // Position belongs to "visuals"
```
Bulk-removing the last owned component evicts the whole matched set at once  –  all those Entities live on in `Main`.
:::

**Deferred mode** works exactly as you'd hope: while a ==World Lock== is held, lazy joins, evictions, and despawns are queued, then routed to their correct Aspects on catch-up:

```csharp
using (var worldLock = world.Lock())
{
    entity.Add(new Position(1, 2)); // deferred join...
    // visuals.Count still 0 here!
}
// ...applied now: visuals.Count == 1
```

## Despawning

Despawning removes an Entity from **all** Aspects  –  its data is cleaned up in every storage universe it ever joined. This includes the fancy cases:

- `world.DespawnAllWith<T>()` and `query.Truncate(n)` cascade across all Aspects of the affected Entities.
- Despawning the *target* of a [Relation](/docs/Advanced/Keys/Relation.md) cleans up relation components in every Aspect that stored one  –  while leaving the holders' other components untouched.

```csharp
var target = world.Spawn();
var follower = world.Spawn().Add(new Position(1, 2)).Add(new Follows(1), target);

target.Despawn();

// the relation is gone (from its Aspect), the rest survives:
Console.WriteLine(follower.Has<Follows>(Match.Any)); // False
Console.WriteLine(follower.Has<Position>());         // True
```

Relations also happily target Entities that never joined the relation's own Aspect  –  a `"game"` crew member can point at a ship that only has `"visuals"` data.

## Enumerating an Aspect

Aspects are `IEnumerable<Entity>` over their members  –  handy for world setup, debugging, and unit tests, and yes, you may LINQ it up:

```csharp
foreach (var member in visuals) { /* ... */ }

var stragglers = visuals.Where(e => !e.Has<Matrix>()).ToArray();
```

Meanwhile, `entity.Components` and `entity.Dump()` aggregate across *all* Aspects  –  from the Entity's point of view, it's all just one big component collection.

## Where to next?

Members sorted! Now learn to query them: [Queries & Streams](Queries.md).
