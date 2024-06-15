---
title: SIMD
order: 5
content: SIMD Query Interface
---

# SIMD Query Interface
Queries expose a set of SIMD operations that allow you to perform *bulk mutations* of *components* on all matched entities. Simple writes and arithmetic operations finish at blazing speeds, and are a great complement to your [Runners](Stream.For.md) that deal in more complex logic.

**fenn**ecs SIMD operations make use of `System.Intrinsics`, especailly the AVX2, SSE2 and ARM AdvSIMD vector instructions where available.

## `Stream<>.Blit<C>`

![a fennec splashes a paintbucket at an entire wall ](https://fennecs.tech/img/fennec-blit.png)
*"Imagine getting all your work done at once!"*

The most prominent SIMD operation is `Blit`, which writes the component value to all entities in the Stream's Query. `C` must be one of the [Stream Types](index.md#stream-types). It requires no additional setup and is always safe.

```csharp
var myStream = new Query<Velocity, Position>().Stream();

myStream.Blit(default(Position));
myStream.Blit(new Velocity(c, 0, 0));
```

`Blit` is incapable of adding new Components, and is also unable to modify Relation Targets. Use the [Query CRUD](/docs/Queries/CRUD.md) `Add...` function for that, which internally uses Blit to write the new components.

::: details :neofox_magnify: BEHIND THE SCENES
`Blit` uses `Span<T>.Fill` to write the data. This is a fast and safe operation that is optimized by the .NET runtime and JIT compiler.

Fast Vectorization techniques are used to write Component Types that are [Blittable](https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types) (e.g. `struct` with only primitive fields), but practically all Reference types are also supported. (they blit rapidly, but not as fast as the Blittables).
:::

------
## Limitations
::: danger 🔏 ALIASING
For simplicity's sake, if a world is not in `WorldMode.Immediate`, i.e. when any `WorldLock` is active, SIMD operations will throw before executing.

### Wait, that's illegal!
The reason is that SIMD Operations can lead to aliasing, where multiple threads write the same memory location, or a source and destination location becomes accessible under multiple "aliases" for reading and writing.

### This can happen with:
- most commonly, **JOBS** *(oh, if it weren't for those darn meddling threads!)*
- certain storage **JOINS** *(when a Stream outputs the same backing type multiple times)*


*(we hope to lift this restriction in the future, probably with per-query/per-archetype locks!)*

But because Jobs execute in non-deterministic order, you will not be able to Blit during a Job/from inside a job for the foreseeable time. But fret not - future versions of fennecs will have an aliasing system to allow for safe writes (especially to other queries/archetypes!)
:::

## Future SIMD Operations
::: details MORE COMING SOON - (click to preview)
![fennec, translucent and glowy blue](https://fennecs.tech/img/fennec-vectorized-256.png)

### Index (writes a running, contiguous index to the component)

### MatrixMul

### MulMatrix

### VectorAdd, VectorDot, VectorCross, VectorScale, VectorOuter, VectorNormalize

### Count

### `integer` Arithmetic (AddI, SubtractI, MultiplyI, DivideI, ShiftLeftI, ShiftRightI, ModuloI)
### `long` Arithmetic (AddL, SubtractL, MultiplyL, DivideL, ShiftLeftL, ShiftRightL, ModuloL)
### `float` Arithmetic (AddF, SubtractF, MultiplyF, DivideF)
### `double` Arithmetic (AddD, SubtractD, MultiplyD, DivideD)



:::


::: tip QUAD :neofox_glasses: WORD :neofox_glasses: QUAD :neofox_glasses: NERD :neofox_glasses: HACK
Psst... until then, you can implement your own arbitrary SIMD operations as seen in the  [Stream.Raw Example](Stream.Raw.md#examples). And since we like to live fast and foxy, try the new extension types in C# 13 for that!
:::

