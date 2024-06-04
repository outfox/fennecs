---
title: Stream &lt; C1, C2, ... &gt;
outline: [2, 3]
order: 1
---

# Stream Views
::: info The magic goggles that turn your Queries into super awesome rainbow roads!
![a fennec painting wall in rainbow with a single smooth brush stroke](https://fennecs.tech/img/fennec-stream.png)

:::

#### `Stream<C1>`
#### `Stream<C1,C2>`
#### `Stream<C1,C2,C3>`
#### `Stream<C1,C2,C3,C4>`
#### `Stream<C1,C2,C3,C4,C5>`

Entities in Queries (including Worlds) and their Components can be read and modified using the extremely light-weight **Stream Views**. Dubbed `Stream<>` in **fenn**ecs code and loosely related to [zip iterators](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.zip), their inner workings are tailored specifically to high-performance Entity-Component System use cases.

::: tip :neofox_rainbow: STREAM VIEWS WILL LET YOU ...
1. ... pass a set of Runners for [`Delegate Actions`](Delegates.md) where you read and write the components in your own custom code. Your code will be run in heavily optimized, super-tight, unrolled loops; most which can even be run in parallel.

1. ... enumerate each Entity in its underlying Query, together with each Component matching the View's Stream Types, as `ValueTuple(Entity, C1, C2, ...)`. This is awesome for Unit tests and easy inspection of your data, and prototyping behaviours using LINQ.
 
3. ... give you safe (and `unsafe`) access to all the components of all the Entities in the Query contiguous blocks of `Memory<C>`, for you do with anything you can imagine. 
:::

Each Stream has an underlying Query, and any number of Streams can be created as views into the same Query. When it comes to processing data, Stream Views are <ins>practically always</ins> your first go-to solution in **fenn**ecs. 

You can get so much work done with these bad bois! *(slaps roof)*

## Stream Types
The Type parameters, `C1, C2, C3, C4, C5` are also known as the **Stream Types**. These are the types of Components that a specific Stream's Runners (e.g. `For`, `Job` and `Raw`) can supply to your code. 

## Runners - Executing Workloads

Each Stream View offers a set of ==Runners== to execute code in batches or parallelized on all Entities in its Query - `For`, `Job`, and `Raw`, or the `ValueTuples` in the `Stream` itself. Their order is determined from your declaration of the `Stream<>`. 

You then pass a delegate (anonymous lambda/delegate or named method, static or instance) to one of three functions, and your code gets run for each Entity. Easy as that for starters, but with some cool twists later!

#### Here's a mnemonic cheat sheet, follow the links in the paragraph headlines for details.

::: info THE CLASSIC
# [`For`](Stream.For.md) / [`For<U>`](Stream.For.md) 
One work item at a time. Call a [`ComponentAction`](Delegates.md#ComponentAction-and-UniformComponentAction) or delegate for each Entity in a Query, providing the Components that match the ==Stream Types== as `ref` to the code.  
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

