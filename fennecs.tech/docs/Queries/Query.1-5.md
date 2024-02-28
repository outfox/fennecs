---
title: Query&lt;C0, ...&gt;
---

# `Query<C0>`
# `Query<C0,C1>`
# `Query<C0,C1,C2>`
# `Query<C0,C1,C2,C3>`
# `Query<C0,C1,C2,C3,C4>`

Queries with Type Parameters (called [Stream Types](Stream%20Types.md)) provide access to Component data of their matched entities.

::: tip
Queries with more than one [Stream Type](Stream%20Types.md) inherit access to all Runners of lesser parametrized Queries, albeit only in the same order, and always starting from `C0`. 

For example, `Query<int, float, string, Vector3>` also has the method `Query<int, float>.For(...)`.
:::


The Type parameters, `C0, C1, C2, C3, C4` are also known as the Query's **Stream Types**. These are the types of Components that a specific Query's Runners (e.g. `For`, `Job` and `Raw`) can supply to your code. 

