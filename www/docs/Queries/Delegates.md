---
title: Delegates 
order: 3
---
# Delegates
Runner methods on Steam Queries expect delegates (Actions) to call. The delegate signatures mirror the count and order of the Query's Stream Types.

## `RefAction<>` and `RefActionU<>`
These are invoked by [`Query<>.For`](Query.For.md) and [`Query<>.Job`](Query.Job.md). The Uniforms are contravariant, which helps with code reuse when you refactor your anonymous, named, or static method signatures to take broader data types.

::: code-group
```cs [plain]
delegate void RefAction<C0>(ref C0 comp0);
delegate void RefAction<C0, C1>(ref C0 comp0, ref C1 comp1);
delegate void RefAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void RefAction<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);
delegate void RefAction<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);
```

```cs [with uniform]
delegate void RefActionU<C0, in U>(ref C0 comp0, U uniform);
delegate void RefActionU<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);
delegate void RefActionU<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);
delegate void RefActionU<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);
delegate void RefActionU<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);
```
:::


## `MemoryAction<>` and `MemoryActionU<>`
These are invoked by [`Query<>.Raw`](Query.Raw.md).

::: code-group
```cs [plain]
delegate void MemoryAction<C0>(Memory<C0> c0);
delegate void MemoryAction<C0, C1>(Memory<C0> c0, Memory<C1> c1);
delegate void MemoryAction<C0, C1, C2>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2);
delegate void MemoryAction<C0, C1, C2, C3>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3);
delegate void MemoryAction<C0, C1, C2, C3, C4>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4);
```

```cs [with uniform]
delegate void MemoryActionU<C0, in U>(Memory<C0> c0, U uniform);
delegate void MemoryActionU<C0, C1, in U>(Memory<C0> c0, Memory<C1> c1, U uniform);
delegate void MemoryActionU<C0, C1, C2, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, U uniform);
delegate void MemoryActionU<C0, C1, C2, C3, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, U uniform);
delegate void MemoryActionU<C0, C1, C2, C3, C4, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4, U uniform);
```
:::

