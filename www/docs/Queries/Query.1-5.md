---
title: Query&lt;&gt;
outline: [2, 3]
order: 2
---

# Stream Queries
# `Query<C0>`
# `Query<C0,C1>`
# `Query<C0,C1,C2>`
# `Query<C0,C1,C2,C3>`
# `Query<C0,C1,C2,C3,C4>`

Queries with Type Parameters are called **Stream Queries**, and they provide super fast and powerful access to Component data of their matched entities. They inherit all the properties of the `Query` base class, including the [CRUD methods](index.md#crud---create-read-update-delete).

When it comes to processing data, Stream Queries are <ins>practically always</ins> your go-to solution in **fenn**ecs. You can get so much work done with these bad bois! *(slaps roof)*

## Stream Types
The Type parameters, `C0, C1, C2, C3, C4` are also known as the Query's **Stream Types**. These are the types of Components that a specific Query's Runners (e.g. `For`, `Job` and `Raw`) can supply to your code. 

## Executing Workloads

Each Stream Query offers a set of ==Runners== to execute code in batches or parallelized on all Entities in the Query - `For`, `Job`, and `Raw`.

You pass a delegate (anonymous lambda/delegate or named method, static or instance), and your code gets run for each Entity. Easy as that for starters, but with some cool twists later!

#### Here's a mnemonic cheat sheet, follow the links in the paragraph headlines for details.

::: info THE CLASSIC
# [`For`](Query.For.md) / [`For<U>`](Query.For.md) 
One work item at a time. Call a [`RefAction`](Delegates.md#refaction-and-refactionu) delegate for each Entity in the query, providing the Components that match the ==Stream Types== as `ref` to the code.  
:neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_waffle::neofox_nom_waffle:
:::

::: info THE WORKHORSE
# [`Job`](Query.Job.md) / [`Job<U>`](Query.Job.md) 
One work item at a time, multi-threaded. Takes a [`RefAction`](Delegates.md#refaction-and-refactionu) delegate and instantly schedules and executes the workload split into chunks, calling it many times in parallel across CPU cores.  
:neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle:
:::

::: danger THE FREIGHT TRAIN
#  [`Raw`](Query.Raw.md) / [`Raw<U>`](Query.Raw.md) 
All work items at once, as contiguous memory. Using a [`MemoryAction`](Delegates.md#memoryaction-and-memoryactionu), delivers the *entire stream data* of each Archetype directly into your ~~fox~~ delegate in one `Memory<T>` per Stream Type.
:neofox_waffle_long_blurry::neofox_scream_stare:
:::

## Prefix Inheritance

Queries with more than one [Stream Type](#stream-types) inherit access to all Runners of lesser parametrized Queries, albeit only in the same order, and always starting from `C0`. 

::: info Example
`Query<int, float, Vector3>` has its parent class's methods `Query<int, float>.For(...)`
and `Query<int>.For(...)` in addition to its own `Query<int, float, Vector3>.For(...)`.

It **does not have** the method ~~`Query<float>.For(...)`~~ or any others where the order of the Stream Type parameters isn't identitcal. For these cases, create a new Query (or discard the unneeded parameters in your delegates).
:::


