---
title: Values
order: 1
outline: [1, 2]
---

# Value Type Components :neofox_verified:

::: tip :neofox_thumbsup: The Performance Champions
Value types are the bread and butter of **fenn**ecs! Stored directly in archetype memory, they provide excellent performance and cache locality. Most of your components will be value types.
:::

## What Makes Them Fast?

In **fenn**ecs, value type components are stored directly within the memory of each Archetype. Each entity has its own copy, and all copies are neatly arranged in contiguous memory – perfect for CPU cache efficiency!

```cs
var entity = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity { X = 1, Y = 1 });
```

::: info :neofox_packed: Cache Locality FTW!
Since value type components are stored contiguously in memory, iterating over entities with the same component layout is extremely efficient. The CPU cache can prefetch and cache component data, minimizing costly RAM reads!
:::

## Defining Value Types

::: tip :neofox_heart: Recommended: Record Structs
C# 10's `record struct` is perfect for components – minimal boilerplate, value semantics, and nice `ToString()` output for debugging!
:::

::: code-group
```cs [🌟 Record Structs 🌟]
// Minimal Boilerplate™️ - it's what fennecs crave!
public record struct Position(float X, float Y);
public record struct Velocity(float X, float Y);
public record struct Health(int Value);
```

```cs [Records with Getters/Setters]
// You can add explicit getters/setters if needed
public record struct Position(float X, float Y)
{
    public float X { get; set; } = X;
    public float Y { get; set; } = Y;
}
```

```cs [Traditional Structs]
// Classic structs work great too!
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

## Usage Examples

### Adding to Entities

```cs
var player = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity { X = 5, Y = 0 })
    .Add(new Health { Value = 100 });
```

### Processing with Streams

```cs
var stream = world.Query<Position, Velocity>().Stream();

stream.For((ref Position position, ref Velocity velocity) =>
{
    position.X += velocity.X;
    position.Y += velocity.Y;
});
```

### Modifying via Ref

```cs
ref var health = ref entity.Ref<Health>();
health.Value -= 10;

// Or inline
entity.Ref<Position>().X += 100;
```

## Tags: The Zero-Size Special Case

Empty structs use no memory at all – they carry meaning purely through their presence!

::: code-group
```cs [Struct Tag]
// Minimal!
public struct PlayerTag;
```

```cs [Record Struct Tag]
// Still good, indents nicely with other records
public record struct PlayerTag;
```
:::

See [Tags](Tags.md) for more details on zero-size components.

## Quick Reference

| Aspect | Value Types |
|--------|-------------|
| **Storage** | Directly in archetype memory |
| **Performance** | Fastest to iterate |
| **Sharing** | Each entity has its own copy |
| **Cache** | Excellent locality |
| **Best For** | Most components! |

## Value Types vs Reference Types

| Value Types | Reference Types |
|-------------|-----------------|
| Stored contiguously | Stored on heap |
| No indirection | Requires pointer dereference |
| Best cache performance | May cause cache misses |
| Copied on archetype change | Reference copied |
| Cannot be shared | Can be [shared](Shareables.md) |

::: warning :neofox_think: When to Use Reference Types
Reference types require at least one indirect memory lookup when accessed. Use [Shareables](Shareables.md) when you need:
- Shared state between entities
- Large, expensive-to-copy data
- Objects from external systems (game engine nodes, etc.)
:::

## Constraints

- Must be `notnull` (no nullable types)
- For `Add<C>()` without value, must have `new()` constraint
- Copied when entity moves between archetypes