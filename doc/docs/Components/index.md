---
title: Components
order: 4
outline: [1, 2]
---

# Components

![a cartoon fennec with a hand truck moving a large stack of colored boxes](/img/fennec-components.png)

*data, relationships, and meaning – packed neatly in boxes* :neofox_box:

::: tip :neofox_thumbsup: The Building Blocks of Your Entities
Components are data attached to Entities! They're how you give entities properties, behaviors, and relationships. From simple values to complex relations – components make your game world come alive.
:::

## What is a Component?

A Component is any piece of data attached to an Entity. In **fenn**ecs, components can be value types, reference types, tags, or even relationships to other entities and objects!

```cs
// Simple data component
entity.Add(new Position { X = 10, Y = 20 });

// Tag component (zero-size marker)
entity.Add<Enemy>();

// Relation to another entity
entity.Add<ChildOf>(parentEntity);

// Link to a managed object
entity.Add(Link.With(gameObject));
```

Whenever a Component is added or removed, the Entity moves to a new [Archetype](#archetypes).

## Quick Reference

| Component Type | Description | Documentation |
|----------------|-------------|---------------|
| **Value Types** | Structs stored contiguously – fast & cache-friendly | [Values](ValueTypes.md) |
| **Tags** | Zero-size markers for categorization | [Tags](Tags.md) |
| **Shareables** | Reference types shared between entities | [Shareables](Shareables.md) |
| **Relations** | Entity-to-entity relationships | [Relations](/docs/Keys/Relation.md) |
| **Object Links** | Links to managed objects | [Links](/docs/Keys/Link.md) |
| **Expressions** | Meta-level component references | [Expressions](Expressions.md) |

## Component Rules :neofox_comfy:

Entities can have:
1. **Zero or one** of each Component Type Expression *(Type + Relation Target)*
2. **Any number** of components *(as long as their Type Expressions are unique)*

::: info :neofox_science: Type Expressions
A `TypeExpression` combines a component's backing type with an optional relation target. This means you can have multiple components of the same type if they have different targets!
:::

### Examples

::: code-group
```cs [Plain Component]
// Plain Data Components - The most common case
record struct Cash(decimal Amount);
record struct Height(float Meters);

bob.Add<Cash>(new(3.25M)); // bob has $3 and 25 cents
bob.Add(new Height(1.85f)); // bob is 1.85 meters tall
```

```cs [Entity-Entity Relation]
// Components can carry a Relation to another Entity
record struct Owes(decimal Amount);

bob.Add<Owes>(new(10M), alice); // he owes alice $10
bob.Add<Owes>(new(23M), eve);   // and owes eve $23

// Each relation is a separate component!
bob.Ref<Owes>(eve).Amount += 7M; // modify eve's balance
```

```cs [Shared Component]
// Reference types can be shared between Entities
Bank chase = new("Chase"); // Bank is a class!

// Many entities can reference the same instance
eve.Add<Bank>(chase);
bob.Add<Bank>(chase);
// Both now share the same Bank object
```

```cs [Object Link]
// Components can BE an object (the link IS the data)
bob.Add(Link.With(chase)); // bob banks at chase
bob.Add(Link.With(targo)); // bob also banks at targo
// Two Bank relations, each backed by the object itself
```
:::

## What Components Can Do :neofox_heart:

| Capability | Description |
|------------|-------------|
| Add/Remove | Attach or detach from entities (single or bulk) |
| Any Type | Backed by any C# language type |
| Relations | Have an optional [Target](/docs/Queries/Matching.md#match-targets) as secondary key |
| Sharing | Reference types can be shared between entities |
| Queries | [Queried](/docs/Queries/index.md) and processed by [Streams](/docs/Streams/index.md) |

## Archetypes

::: tip :neofox_packed: **fenn**ecs is an Archetype-Based ECS
Archetypes are internal collections of Entities that share the same set of Components. As an ECS user, you never interact with Archetypes directly, but understanding them helps you get the most out of **fenn**ecs.
:::

By grouping Entities into Archetypes, the ECS (and by extension the CPU) can process them very efficiently.

**fenn**ecs constantly ensures that Entities with the same Components are stored tightly packed together in memory, which is a key factor in achieving high performance.

:neofox_packed_blue: :neofox_packed: :neofox_packed_green:

::: info :neofox_science: Performance Tip
In large projects, aim for either:
- **Few, small archetypes** (dozens of entities)
- **Large, chunky archetypes** (10k+ entities)

This maximizes cache efficiency during iteration.
:::

::: details :neofox_magnify: Archetype Internals
There's no practical limit to Archetypes and their sizes. Archetypes can get compacted or removed by the World's garbage collector.

This is especially true for Archetypes of Relation Targets when the target despawns – because that Archetype is extremely unlikely to ever get any Entities again.
:::