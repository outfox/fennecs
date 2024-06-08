---
title: Fox Typing
order: 5
---

# :neofox_hug_duck_heart: Fox Typing

~~There are three primary types of Components in **fenn**ecs: **Duck**, **Fox**, and **Hybrid**.~~
<br/>Oh, *shut up,* Copilot.

## Making Components Simple
How do we make our Components as useful as possible, whilst keeping them as simple as possible?

The idea is to have safe, easily identifiable types that make our Runner Delegate code easier to write and understand, and also don't clash when we need multiple values of the same underlying type, e.g. `System.Numerics.Vector3` for both Position, Velocity, and Acceleration.

## `record` and `record struct` Types
C# 9 introduced `record` types, which would be a decent fit for Components, because they have value semantics and are easy to reason about, have nice equality and ToString methods. Unfortunately, they are reference types - this makes them good for [Shareables](Shareables.md), but otherwise not as ideal for Components.

Then, C# 10 brought us `record structs`, which are perfect for our bread-and-butter data Components because they can be packed tightly into Archetype storage, and retain most of the benefits of `record` types.

### If it doesn't walk like a Fox and doesn't talk like a Fox, but it contains a Fox, it surely must be a Fox.
```csharp
public record struct Position(Vector3 value);
public record struct Velocity(Vector3 value);
public record struct Acceleration(Vector3 value);
```
`records` can also be partially reinstantiated with the `with`keyword, which is useful for creating new instances with only a few fields changed. The rest is duplicated by a shallow copy.
```csharp
public record struct TR(Vector3 position, Quaternion rotation);

var tr = new TR(Vector3.Zero, Quaternion.Identity);
var tr2 = tr with { position = new Vector3(1, 2, 3) };
var tr3 = tr with { rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f) };
```

## Future Interfaces / Decorators

::: details Future Foxiness (WIP)
# `Fox128<T>` and `Fox256<T>`
Future SIMD operations will allow us to make a promise by these two interfaces that a Type aligns well with a certain size (in bits). This interface is special because it required a Value property that is a representation of the struct as `System.Numerics.Vector128<T>` or `System.Numerics.Vector256<T>`, respectively. Usually, this is done through marshalling and explicit `[StructLayout]`.

```csharp
public interface Fox128<T> where T : struct
{
    public Vector128<T> Value { get; }
}
```

The enhanced SIMD operations are still a work in progress, so this mechanism may evolve.
:::