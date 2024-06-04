---
title: Delegates 
order: 5
---
# Delegates
Runner methods on Steam Queries expect delegates (Actions) to call. The delegate signatures mirror the count and order of the Query's Stream Types.

## `ComponentAction<>` and `UniformComponentAction<>`
These are invoked by [`Stream<>.For`](Stream.For.md) and [`Stream<>.Job`](Stream.Job.md). The Uniforms are contravariant, which helps with code reuse when you refactor your anonymous, named, or static method signatures to take broader data types.

::: code-group
```cs [plain]
delegate void ComponentAction<C0>(ref C0 comp0);
delegate void ComponentAction<C0, C1>(ref C0 comp0, ref C1 comp1);
delegate void ComponentAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void ComponentAction<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);
delegate void ComponentAction<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);
```

```cs [with uniform]
delegate void UniformComponentAction<C0, in U>(ref C0 comp0, U uniform);
delegate void UniformComponentAction<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);
delegate void UniformComponentAction<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);
delegate void UniformComponentAction<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);
delegate void UniformComponentAction<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);
```
:::


## `EntityComponentAction<>` and `EntityUniformComponentAction<>`
These are invokable through [`Stream<>.For`](Stream.For.md). In addition to the Components and optional Uniform, they also receive the Entity that can be used to interact structurally with an Entity right then and there.

::: code-group
```cs [plain]
delegate void EntityComponentAction<EntityC0>(ref C0 comp0);
delegate void EntityComponentAction<C0, C1>(ref C0 comp0, ref C1 comp1);
delegate void EntityComponentAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void EntityComponentAction<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);
delegate void EntityComponentAction<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);
```

```cs [with uniform]
delegate void UniformEntityComponentAction<C0, in U>(ref C0 comp0, U uniform);
delegate void UniformEntityComponentAction<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);
delegate void UniformEntityComponentAction<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);
delegate void UniformEntityComponentAction<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);
delegate void UniformEntityComponentAction<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);
```
:::


## `MemoryAction<>` and `MemoryUniformAction<>`
These are invoked by [`Stream<>.Raw`](Stream.Raw.md).

::: code-group
```cs [plain]
delegate void MemoryAction<C0>(Memory<C0> c0);
delegate void MemoryAction<C0, C1>(Memory<C0> c0, Memory<C1> c1);
delegate void MemoryAction<C0, C1, C2>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2);
delegate void MemoryAction<C0, C1, C2, C3>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3);
delegate void MemoryAction<C0, C1, C2, C3, C4>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4);
```

```cs [with uniform]
delegate void MemoryUniformAction<C0, in U>(Memory<C0> c0, U uniform);
delegate void MemoryUniformAction<C0, C1, in U>(Memory<C0> c0, Memory<C1> c1, U uniform);
delegate void MemoryUniformAction<C0, C1, C2, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, U uniform);
delegate void MemoryUniformAction<C0, C1, C2, C3, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, U uniform);
delegate void MemoryUniformAction<C0, C1, C2, C3, C4, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4, U uniform);
```
:::

