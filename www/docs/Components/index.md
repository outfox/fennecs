---
title: Components
order: 4
outline: [1, 3]
---

# Components

Components are data attached to Entities!

![a cartoon fennec with a hand truck moving a large stack of colored boxes](https://fennecs.tech/img/fennec-components.png)

Whenever a Component is added or removed, the Entity moves to a new [Archetype](#archetypes).

## Entities can have ...
1. **zero or one** of each Component Type 
    - *more precisely: of each `TypeExpression`: Type <u>and</u> Relation Target*
2. **any number** of components 
    - *as long as their `TypeExpressions` are unique. **fenn**ecs ensures this!*


### Some Examples
::: code-group
```csharp [Plain Component]
// Plain Data Components - The most common case
record struct Cash(decimal Amount);
record struct Height(float Meters);

bob.Add<Cash>(new(3.25M)); // bob has $3 and 25 cents (Plain component)
bob.Add(new Height(1.85f)); // bob is 1.85 meters tall (alternate syntax)
```

```csharp [Entity-Entity Relation]
// All Components can also carry a Relation to another Entity
record struct Owes(decimal Amount);

bob.Add<Owes>(new(10M), alice); // he owes alice $10 (Relation Owes->alice)
bob.Add<Owes>(new(23M), eve); // and owes eve $23 (Relation Owes->eve)

bob.Add<Owes>(new(7M), eve); // ❌ERROR! instead, add the $7 to existing balance

if (!bob.Has<Owes>(eve)) { 
    bob.Add<Owes>(eve);
}

bob.Ref<Owes>(eve).Amount += 7M;
```

```csharp [Shared Component]
// References can be shared as Plain components between Entities
Bank chase = new("Chase"); // Bank is a class / reference type!

// Many entities can have chase as their bank
eve.Add<Bank>(chase);
bob.Add<Bank>(chase);

bob.Add<Bank>(targo); // ❌ERROR! (bob already has a Plain bank!)
```

```csharp [Object Link Relation]
// Components can BE an object and be considered a Relation to that Object
// ... as opposed to just "having a bank: chase"
// bob has two Bank relations (each backed a reference to the object)
bob.Add(Link.With(chase)); // bob banks at chase (Type Bank->chase)
bob.Add(Link.With(targo)); // bob also banks at targo (Type Bank->targo)
```


## Components may ...
1. be added/removed to Entities (single, or in bulk)
1. be backed any C# language type
1. have an optional [Target](/docs/Queries/Matching.md#match-targets) (secondary key) associated with them
1. be shared between Entities (by reference)
1. be [Queried](/docs/Queries/index.md) for and processed by [Streams](/docs/Streams/index.md)


## Archetypes

Archetypes are internal collections of Entities that share the same set of Components.

### **fenn**ecs is an Archetype-Based ECS
It shares many properties (including strengths and weaknesses) with other ECS of this family. As an ECS user, you never interact with Archetypes directly, but understanding them is helpful to get the most out of **fenn**ecs.

By grouping Entities into Archetypes, the ECS (and by extension the CPU) can process them very efficiently. The system ensures that Entities with the same Components are stored tightly packed together in memory, which is a key factor in achieving high performance.  

:neofox_packed: :neofox_packed: :neofox_packed:

This means that in large projects, there's a performance incentive to make frequently processed Archetypes either rare (e.g. aim for just a few dozen small ones), or chunky and large (e.g. 10k entities for a start, or even more).

There's no practical limit to Archetypes and their sizes. Archetypes can get Compacted or Removed by the World's Garbage Collector. this is especially true for Archetypes of Relation Targets when the Target despawns - because that Archetype is extremely unlikely to ever get any Entities.