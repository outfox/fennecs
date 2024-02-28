---
title: Query&lt;C0, ...&gt;
---

# `Query<C0>`
# `Query<C0,C1>`
# `Query<C0,C1,C2>`
# `Query<C0,C1,C2,C3>`
# `Query<C0,C1,C2,C3,C4>`

Queries with Type Parameters (called [Stream Types](Stream%20Types.md)) provide access to Component data of their matched entities.

## Component Type Parameters

The Type parameters, `C0, C1, C2, C3, C4` are also known as the Query's **Stream Types**. These are the types of Components that a specific Query's Runners (e.g. `For`, `Job` and `Raw`) can supply to your code. 

## Runners

Each Query with one or more [Stream Type](Stream%20Types.md) offer a set of Runners to execute code in batches or parallelized on all Entities in the Query.

### [`For`](Query.For.md) / [`For<U>`](Query.For.md)
> Call a [`RefAction`](RefAction.md) delegate for each Entity in the query, providing the Components that match the Stream Types as `ref` to the code.

### [`Job`](Query.Job.md) / [`Job<U>`](Query.Job.md)
> Calls a [`RefAction`](RefAction.md) delegate and instantly schedules and executes the workload split into chunks, Parallel Processing across CPU cores.

###  [`Raw`](Query.Raw.md) / [`Raw<U>`](Query.Raw.md)
> Calls a different signature delegate, a [`MemoryAction`](MemoryAction.md), but passes a `Memory<T>` for the entire workload of a Stream Type, not individual references for each Entity.

### Prefix Subsets

Queries with more than one [Stream Type](Stream%20Types.md) inherit access to all Runners of lesser parametrized Queries, albeit only in the same order, and always starting from `C0`. 

::: info Example
`Query<int, float, Vector3>` has its parent class's methods `Query<int, float>.For(...)`
and `Query<int>.For(...)` in addition to its own `Query<int, float, Vector3>.For(...)`.

It **does not have** the method ~~`Query<float>.For(...)`~~ or any others where the order of the Stream Type parameters isn't identitcal. For these case, create a new Query.
:::


