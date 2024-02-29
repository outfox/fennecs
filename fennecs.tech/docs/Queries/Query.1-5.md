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

## Prefix Subsets

Queries with more than one [Stream Type](Stream%20Types.md) inherit access to all Runners of lesser parametrized Queries, albeit only in the same order, and always starting from `C0`. 

::: info Example
`Query<int, float, Vector3>` has its parent class's methods `Query<int, float>.For(...)`
and `Query<int>.For(...)` in addition to its own `Query<int, float, Vector3>.For(...)`.

It **does not have** the method ~~`Query<float>.For(...)`~~ or any others where the order of the Stream Type parameters isn't identitcal. For these cases, create a new Query (or discard the unneeded parameters in your delegates).
:::


