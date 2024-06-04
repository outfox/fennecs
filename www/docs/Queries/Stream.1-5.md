---
title: Stream &lt; C1, C2, ... &gt;
outline: [2, 3]
order: 1
---

# Stream Views
The amazing Magic Glasses that make your Queries super awesome!

![a fennec painting wall in rainbow with a single smooth brush stroke](https://fennecs.tech/img/fennec-stream.png)

# `Stream<C1>`
# `Stream<C1,C2>`
# `Stream<C1,C2,C3>`
# `Stream<C1,C2,C3,C4>`
# `Stream<C1,C2,C3,C4,C5>`

Entities in Queries (including Worlds) can be enumerated by the extremely light-weight **Stream Views** (colloquially *Streams*, even though they're really what some call a `zip_view`).

Streams are, in and of themselves, free structs to instantiate whereever and whenever needed. There's practically no overhead whatsoever. *(a tiny amount of memory would be allocated when you work with Filter Expressions)*

Streams provide super fast and powerful access to Component data of their matched entities. They inherit all the properties of the `Query` base class, including the [CRUD methods](index.md#crud---create-read-update-delete).

Each Stream has an underlying Query, and any number of Streams can be created as views into the same Query. When it comes to processing data, Stream Views are <ins>practically always</ins> your go-to solution in **fenn**ecs. 

You can get so much work done with these bad bois! *(slaps roof)*

## Stream Types
The Type parameters, `C1, C2, C3, C4, C5` are also known as the **Stream Types**. These are the types of Components that a specific Stream's Runners (e.g. `For`, `Job` and `Raw`) can supply to your code. 

## Executing Workloads

Each Stream View offers a set of ==Runners== to execute code in batches or parallelized on all Entities in the Query - `For`, `Job`, and `Raw`.

You pass a delegate (anonymous lambda/delegate or named method, static or instance), and your code gets run for each Entity. Easy as that for starters, but with some cool twists later!

#### Here's a mnemonic cheat sheet, follow the links in the paragraph headlines for details.

::: info THE CLASSIC
# [`For`](Stream.For.md) / [`For<U>`](Stream.For.md) 
One work item at a time. Call a [`ComponentAction`](Delegates.md#ComponentAction-and-UniformComponentAction) or delegate for each Entity in the Query, providing the Components that match the ==Stream Types== as `ref` to the code.  
:neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_nom_waffle:
:::

::: info THE WORKHORSE
# [`Job`](Stream.Job.md) / [`Job<U>`](Stream.Job.md) 
One work item at a time, multi-threaded. Takes a [`ComponentAction`](Delegates.md#ComponentAction-and-UniformComponentAction) delegate and instantly schedules and executes the workload split into chunks, calling it many times in parallel across CPU cores.  
:neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle:
:::

::: danger THE FREIGHT TRAIN
#  [`Raw`](Stream.Raw.md) / [`Raw<U>`](Stream.Raw.md) 
All work items at once, as contiguous memory. Using a [`MemoryAction`](Delegates.md#memoryaction-and-memoryUniformAction), delivers the *entire stream data* of each Archetype directly into your ~~fox~~ delegate in one `Memory<T>` per Stream Type.
:neofox_waffle_long_blurry::neofox_scream_stare:
:::
