---
title: Concepts
order: 0

head:
  - - meta
    - name: description
      content: Conceptual overview over the fennecs Entity-Component System
---

# Typical ECS Concepts 

::: tip <sub>*(these are what makes most ECS architectures tick)*</sub>
### [Entities](Entities/) are spawned in a [World](World.md). 
Entities can be spawned solo, or in bulk with pre-configured components.

### [Components](Components/) can be added to / removed from Entities.
Components can be backed by any type; Value or Reference, even empty `structs`.

### [Queries](Queries/) filter Entities using [Match Expressions](Queries/MatchExpressions.md).
This matching is done by Component types and targets (presence or absence).

### Queries [Run Code](Queries/Query.For.md) on data streams of Components.
You provide logic in [Runner Delegates](Queries/Delegates.md) which are executed on a [single](Queries/Query.For.md) or [multiple](Queries/Query.Job.md) threads.

### Component data is always kept contiguous* in Memory.
Structurally similar Entities are packed into [Archetypes](Archetype.md) for improved [cache locality](https://en.wikipedia.org/wiki/Locality_of_reference).

<sub>\* *per Archetype*</sub>

:::

----------------------

# Unique Concepts 

::: warning <sub>*(~~weird~~ cool stuff specific to **fenn**ecs)*</sub>
### [Relations](Relation.md) are Components with an [Entity Target](Queries/MatchExpressions.md#match-targets).
These add expressive, powerful grouping semantics. Relations can be backed by any type.

### [Object Links](Link.md) are Components backed by a [Shared Object Target](Queries/MatchExpressions.md#match-targets).
Group Entities logically and in memory by linking them to shared data, like a physics world.

### Queries expose *fast* SIMD & Structural Ops on matched Entities.
You can efficiently [add](Queries/CRUD.md), [remove](Queries/CRUD.md), or [modify](Queries/SIMD.md) components in bulk.

### Worlds and Queries are `IEnumerable<Entity>`.
It's amazing to be able to once in a while just LINQ it up and <u>*be done*</u> somewhere.

### There are no formalized Systems.
You have a higher degree of freedom when and how to interact with Queries.

### There is no formalized Scheduler.
Parallel Jobs execute synchronously, as fast as possible. Runners are invokable anytime, anywhere.  

### Structural Changes can be *submitted at any time.*
The World will process them immediately, or at the end of the current Runner's scope.  
<sub>\* *srsly WTAF even is* `EndInitializationEntityCommandBufferSystem.AddJobHandleForProducer(JobHandle)`, *smh...*</sub>

:::


