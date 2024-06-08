---
title: Concepts
order: 0
outline: [1,1]

head:
  - - meta
    - name: description
      content: Conceptual overview over the fennecs Entity-Component System
---

# :neofox_heart: ... a little ECS that loves you back!
The Entity-Component System (ECS) is an architectural pattern that favors composition over inheritance. **Entities** are small, unique identifiers, and **Components** are data attached to them. **Systems** denote the logic that operates on this data. ECS provides flexibility, performance, and maintainability by separating data and behavior, allowing complex game objects to be built from simpler components.

# Typical ECS Concepts 

::: tip <sub>*(these are what makes many ECS architectures tick)*</sub>
### [Entities](Entities/) exist in a [World](World.md). 
Entities can be spawned solo, or in bulk with pre-configured components.

### [Components](Components/) can be added to / removed from Entities.
Components can be backed by any type; Value or Reference, even empty `structs`.

### [Queries](Queries/) filter Entities using [Match Expressions](Queries/Matching.md).
This matching is done by Component types and targets (presence or absence).

### Code [Acts](Streams/Stream.For.md) on Component data in tight, optimized loops.
You provide logic in [Runner Delegates](Streams/Delegates.md) which are executed on a [single](Streams/Stream.For.md) or [multiple](Streams/Stream.Job.md) threads.

### Component data is always kept contiguous* in Memory.
Structurally similar Entities are packed into [Archetypes](/docs/Components/index.md#archetype) for improved [cache locality](https://en.wikipedia.org/wiki/Locality_of_reference).

<sub>\* *per Archetype*</sub>

:::

----------------------

# Unique fennecs Concepts 

::: warning <sub>*(~~weird~~ cool stuff made us you all wide-eyed and bushy tailed again)*</sub>
### [Relations](/docs/Components/Relation.md) are Components with an [Entity Target](Queries/Matching.md#match-targets).
These add expressive, powerful grouping semantics. Relations can be backed by any type.

### [Links](/docs/Components/Link.md) are Components backed by an [Object Target](Queries/Matching.md#match-targets).
Group Entities logically and in memory by linking them to shared data, like a physics world.

### [Streams](Streams/) expose *fast* Iteration and SIMD Ops 
Efficiently and safely [iterate entities](Streams/Stream.For.md), or [blit](Streams/SIMD.md) Components in bulk - read/write entire [memory blocks](Streams/Stream.Raw.md).

### Queries expose Structural Changes (just as Entities do)
Efficiently and safely [add](Queries/CRUD.md), [remove](Queries/CRUD.md) from individual Entities or entire matched Queries.

### Runners let you pass [uniform](Streams/Stream.For.md#uniforms-shmuniforms) data to those Workloads.
A tiny tidbit that streamlines the process of passing data into a job or run.

### Worlds and Queries are `IEnumerable<Entity>`.
It's amazing to be able to every now and then just LINQ it up and <u>*be done*</u> somewhere.

### There are no formalized Systems.
You have a higher degree of freedom when and how to interact with Queries and Stream Views.

### There is no formalized Scheduler.
Parallel Jobs execute synchronously, as fast as possible. Runners are invokable anytime, anywhere.  

### Structural Changes may be submitted\* *at any time.*
Worlds process them at the end of a Query Runner's scope, otherwise immediately.  
<sub>\* *never type* [`EndInitializationEntityCommandBufferSystem`](https://docs.unity3d.com/Packages/com.unity.entities@1.0/api/Unity.Entities.EndInitializationEntityCommandBufferSystem.html) *nor* [`AddJobHandleForProducer(JobHandle)`](https://docs.unity.cn/Packages/com.unity.entities@1.0/api/Unity.Entities.EntityCommandBufferSystem.AddJobHandleForProducer.html) *again!* 🦊</sub>

:::

---------------------

## :neofox_boop_happy: Boop the fox, and you're all done with the first page!
!["using fennecs;"](https://fennecs.tech/img/fennecs-rule.png)
