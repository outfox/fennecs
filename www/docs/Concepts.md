---
title: Concepts
order: 0

head:
  - - meta
    - name: description
      content: Conceptual overview over the fennecs Entity-Component System
---

# Typical ECS Concepts 

::: tip <sub>*(also found in **fenn**ecs)*</sub>
### [Entities](Entities/) are spawned in a [World](World.md). 
They can be spawned solo, or in bulk with pre-configured components.

### [Components](Components/) can be added to / removed from Entities.
They can be backed by any type; Value or Reference, including empty `structs`.

### [Queries](Queries/) filter Entities using [Match Expressions](Queries/MatchExpressions.md).
This matching is done by Component types and targets (presence or absence).

### Queries [Run Code](Queries/Query.For.md) on the components to process them.
You provide [Runner Delegates](Queries/Delegates.md) which are executed on a [single](Queries/Query.For.md) or [multiple](Queries/Query.Job.md) threads.

### Component Data is always kept contiguous* in Memory.
Structurally similar Entities are packed into [Archetypes](Archetype.md) for improved [cache locality](https://en.wikipedia.org/wiki/Locality_of_reference).

<sub>\* per Archetype</sub>

:::

----------------------

# Unique Concepts 

::: warning <sub>*(specific to **fenn**ecs)*</sub>
### There are no formalized Systems.
You have a higher degree of freedom when and how to interact with Queries.

### Queries expose Bulk Operations on matched Entities.
You can efficiently [add](Queries/CRUD.md), [remove](Queries/CRUD.md), or [modify](Queries/SIMD.md) components in bulk.

### [Relations](Relation.md) are Components with an [Entity Target](Queries/MatchExpressions.md#match-targets).
These add expressive and powerful grouping semantics. Relations can be backed by any component.

### [Object Links](Link.md) are Components with a [Shared Object Target](Queries/MatchExpressions.md#match-targets).
These add a way to group & link Entities to shared data, like a a physics world.
:::

