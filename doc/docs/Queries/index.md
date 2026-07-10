---
title: Queries
layout: doc
outline: [1, 2]
order: 5
---

# :neofox_magnify: Queries

::: tip :neofox_thumbsup: FIND YOUR FOXES!
Queries are your lens into the World  –  fast, reactive views that match Entities by their Component signatures.
:::

Each Query is a view into a World, representing a subset of its Entities. Queries are incredibly fast and update automatically whenever entities spawn or their component structure changes.

## Quick Reference

| Topic | Description |
|-------|-------------|
| [Matching](Matching.md) | Define which Entities your Query contains using Match Expressions |
| [Bulk CRUD](CRUD.md) | Add, remove components, or despawn all matched Entities at once |
| [Stream Views](../Streams/) | Process component data with typed iteration and delegates |
| [Filters](../Streams/Filters.md) | Dynamically narrow down matched Archetypes |

## What is a Query?

A Query maintains a collection of all [Archetypes](../Components/#archetypes) it matches. When iterating Components or enumerating Entities, the Query does so for each matched Archetype in deterministic order.

::: details :neofox_peek_owo: (expand to see World diagram)
A World contains Entities and their Components, as well as their structure and Relations.
![World Example: blue circle labeled world filled with fox emojis with many different traits](/img/diagram-world.png)
:::

![Query Visualization: fox emojis with various traits grouped by common traits in several colored boxes](/img/diagram-queries.png)

## Three Main Purposes

### 1. Matching & Filtering Entities

Queries use [Match Expressions](Matching.md) to define which Entities they contain ("match"). A Query remains associated with a specific World and cannot bridge multiple Worlds.

```cs
// Match all entities with Position and Velocity components
var movers = world.Query<Position, Velocity>().Stream();
```

### 2. Processing Data (through Stream Views)

The most powerful feature of Queries is that they provide a [Stream View](../Streams/) that can run code on all matched Entities, providing mutable references to component data.

```cs
// Update all positions based on velocity
movers.For((ref Position pos, ref Velocity vel) =>
{
    pos.X += vel.X * deltaTime;
    pos.Y += vel.Y * deltaTime;
});
```

### 3. Bulk CRUD Operations

Queries expose methods to operate quickly on all matched entities  –  [read more!](CRUD.md)

```cs
// Despawn all matched entities
query.Despawn();

// Or truncate to a specific size
query.Truncate(100, TruncateMode.KeepFirst);
```

## Cleaning Up Queries

::: warning :neofox_nom_verified: MEMORY MANAGEMENT
Queries have a modest memory footprint, but in Worlds with many fragmented Archetypes, the cost of Query update notifications can add up!
:::

Call `Query.Dispose()` to de-register a query from its World and free its resources.

```cs
query.Dispose();
```

