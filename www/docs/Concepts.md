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

### Queries let your code [Act](Queries/Query.For.md) on streams of Component data.
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

### [Links](Link.md) are Components backed by an [Object Target](Queries/MatchExpressions.md#match-targets).
Group Entities logically and in memory by linking them to shared data, like a physics world.

### Runners let you pass [uniform](Queries/Query.For.md#uniforms-shmuniforms) data to your Workloads.
A tiny tidbit that streamlines the process of passing data into a job or run.

### Queries expose *fast* Structural Change, SIMD, and Memory Ops
Efficiently and safely [add](Queries/CRUD.md), [remove](Queries/CRUD.md), or [modify](Queries/SIMD.md) components in bulk - even entire [memory blocks](Queries/Query.Raw.md).

### Worlds and Queries are `IEnumerable<Entity>`.
It's amazing to be able to every now and then just LINQ it up and <u>*be done*</u> somewhere.

### There are no formalized Systems.
You have a higher degree of freedom when and how to interact with Queries.

### There is no formalized Scheduler.
Parallel Jobs execute synchronously, as fast as possible. Runners are invokable anytime, anywhere.  

### Structural Changes may be submitted\* *at any time.*
Worlds process them at the end of a Query Runner's scope, otherwise immediately.  
<sub>\* *never type* [`EndInitializationEntityCommandBufferSystem`](https://docs.unity3d.com/Packages/com.unity.entities@1.0/api/Unity.Entities.EndInitializationEntityCommandBufferSystem.html) *nor* [`AddJobHandleForProducer(JobHandle)`](https://docs.unity.cn/Packages/com.unity.entities@1.0/api/Unity.Entities.EntityCommandBufferSystem.AddJobHandleForProducer.html) *again!* 🦊</sub>

:::


