---
title: Values
order: 1
---

# Value Type Components

In **fenn**ecs, components are often defined as [value types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types) (e.g., structs or primitives). Value type components are stored directly within the memory of each Archetype, providing excellent performance and memory efficiency.

::: info :neofox_verified: SIMPLY THE BEST
The fastest Components to process with **fenn**ecs are indeed value types - pure data. This means that each entity has its own copy of the component. Their data is neatly arranged in memory so it's easy to access and process efficiently and with speed.
:::

Reference types, although very useful, always require at least one indirect memory lookup when accessed, and it is not guaranteed that they are even all in the same memory region - which (when pushed to extremes) will lead to CPU cache misses and somewhat slower processing.


## Defining Value Type Components

To define a value type component, simply create a record struct (recommended!) or a simple struct:

::: code-group

```csharp [🌟 record structs 🌟]
// Minimal Boilerplate™️ - it's what fennecs crave!
public record struct Position(float X, float Y);
public record struct Velocity(float X, float Y);
```

```csharp [records, with extra steps]
// you can do all the things with these getters and setters if you need
public record struct Position(float X, float Y)
{
    public float X { get; set; } = X;
    public float Y { get; set; } = Y;
}

public record struct Velocity(float X, float Y)
{
    public float X { get; set; } = X;
    public float Y { get; set; } = Y;
}
```

```csharp [old-timey structs]
// not here to judge, these work great, too!
public struct Position
{
    public float X;
    public float Y;
}

public struct Velocity
{
    public float X;
    public float Y;
}
```
:::

A special case are `Tags` - empty structs. These use no memory at all for storage, and carry their meaning just through their presence.

::: code-group

```csharp [plain struct]
// look at me, I'm the minimalist now!
public struct PlayerTag;
```
```csharp [record struct]
// still good, indents better in groups with other records
public record struct PlayerTag;
```
:::

## Adding Value Type Components to Entities

When you add a value type component to an entity, the component's data is stored directly within the entity's memory:

```csharp
var entity = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity { X = 1, Y = 1 });
```

Each entity has its own unique instance of the `Position` and `Velocity` components, with the data stored contiguously in memory.

## Efficient Iteration with Runners

One of the key benefits of using value type components is the favorable cache locality when iterating over entities using Runners like `For` and `Job`.

::: info :neofox_packed: NEATLY PACKED - CACHE LOCALITY FTW!
Since value type components are stored contiguously in memory, iterating over entities with the same component layout (i.e., within the same archetype) is extremely efficient. The CPU cache can effectively prefetch and cache the component data, minimizing costly RAM reads and writebacks!
:::

```csharp
var query = world.Query<Position, Velocity>().Stream();

stream.For((ref Position position, ref Velocity velocity) =>
{
    position.X += velocity.X;
    position.Y += velocity.Y;
});
```

In this example, the `For` runner efficiently iterates over entities with `Position` and `Velocity` components, taking advantage of the cache locality provided by the contiguous memory layout.

## Considerations

1. Value type components are the default and recommended choice for most scenarios in **fenn**ecs.

2. Value types provide the best performance and memory efficiency due to their contiguous storage and cache-friendly iteration.

3. Each entity has its own unique instance of a value type component, allowing for independent modification without affecting other entities.

4. When an entity's component layout changes (e.g., adding or removing components), the entity is moved to a different archetype, and the value type components are copied to the new memory location.

## Conclusion

Value type components are the foundation of **fenn**ecs, providing excellent performance and memory efficiency. By storing components directly within entities and enabling cache-friendly iteration, value types unlock the full potential of the ECS architecture.

When defining components, consider using value types (structs) by default, and leverage the power of Runners like `For` and `Job` to efficiently process entities and their components.