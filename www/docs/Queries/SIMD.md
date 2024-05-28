---
title: SIMD
order: 4
---

# Query SIMD Interface
Queries expose a set of SIMD operations that allow you to perform bulk operations on the matched entities. These operations can perform simple operations at blazing speeds, and are a great complement to Runners for more complex logic.

The operations make use of `System.Intrinsics`, especailly the AVX2, SSE2 and ARM AdvSIMD vector instructions where available.

::: tip :neofox_glasses: QUADWORD :neofox_glasses: SUPERNERD :neofox_glasses: QUADALERT :neofox_glasses:
You can also implement your own arbitrary SIMD operations by using [Query.Raw](Query.Raw.md#examples).  
And since we like to live fast, use the amazing new implicit extension types in C# 13!
:::


## `Query<C>.Blit`
The most prominent SIMD operation is `Blit`, which writes the component value to all entities in the Query. `C` must be one of the Query's [Stream Types](index.md#stream-types).

```csharp
using var myQuery = new Query<Velocity, Position>().Compile();

myQuery.Blit(default(Position));
myQuery.Blit(new Velocity(c, 0, 0));
```

`Blit` is incapable of adding new Components, and is also unable to modify Relation Targets. Use the [CRUD](../CRUD.md) `Add...` functions for that, which internally uses Blit to write the new components.

::: warning :neofox_confused: INCONSISTENCY or (IN)CONVENIENCE?
Unlike the other SIMD operations, the `Blit` methods are declared in each `Query<>` class, where the others are in the `Query.SIMD` sub-interface. This may change in the future.
:::

::: details :neofox_magnify: BEHIND THE SCENES
`Blit` uses `Span<T>.Fill` to write the data. This is a fast and safe operation that is optimized by the .NET runtime and JIT compiler.

Fast Vectorization techniques are used to write Component Types that are [Blittable](https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types) (e.g. `struct` with only primitive fields), but practically all Reference types are also supported. (they blit rapidly, but not as fast as the Blittables).
:::

------

::: details MORE COMING SOON - (click to preview)

### Index (writes a running, contiguous index to the component)

### MatrixMul

### MulMatrix

### VectorAdd, VectorDot, VectorCross, VectorScale, VectorOuter, VectorNormalize

### Count

### `integer` Arithmetic (AddI, SubtractI, MultiplyI, DivideI, ShiftLeftI, ShiftRightI, ModuloI)
### `long` Arithmetic (AddL, SubtractL, MultiplyL, DivideL, ShiftLeftL, ShiftRightL, ModuloL)
### `float` Arithmetic (AddF, SubtractF, MultiplyF, DivideF)
### `double` Arithmetic (AddD, SubtractD, MultiplyD, DivideD)


