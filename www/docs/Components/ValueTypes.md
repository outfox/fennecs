---
title: Value Types
order: 1
---

# Value Type Components

In **fenn**ecs, components are typically defined as [value types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types) (e.g., structs or primitives). Value type components are stored directly within the memory of each entity, providing excellent performance and memory efficiency.

::: info :neofox_verified: SIMPLY THE BEST
By default, components in **fenn**ecs are value types. This means that each entity has its own copy of the component. Their data is neatly arranged in memory so it's easy to access and process efficiently and with speed.
:::

## Defining Value Type Components

To define a value type component, simply create a struct:

```csharp
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

A special case are `Tags` - empty structs. These use no memory at all for storage, and carry their meaning just through their presence.

```csharp
public struct PlayerTag;
```


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