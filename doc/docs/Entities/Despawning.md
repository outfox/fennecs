---
title: Despawn
order: 2
outline: [1, 2]
---

# Despawning Entities :neofox_x_x:

::: tip :neofox_nom_verified: The Circle of Life
What is spawned must eventually despawn. It's natural, it's healthy, and it keeps your World tidy!
:::

## `Entity.Despawn()`

Removes an entity from its World, freeing its identity for reuse and cleaning up all its components.

```cs
var fox = world.Spawn();
fox.Add(new Position { X = 10, Y = 20 });
fox.Add<Fluffy>();

fox.Despawn();  // Goodbye, little fox! 🦊

Console.WriteLine(fox.Alive);  // false
```

## What Happens When You Despawn?

When an entity is despawned:

1. **All components are removed** - The entity's data is cleaned up
2. **Relations are severed** - Any relations pointing *to* this entity from others are also removed
3. **Identity is recycled** - The slot becomes available for new entities
4. **The handle becomes stale** - The `Entity` struct still exists, but `Alive` returns `false`

::: warning :neofox_owo: The Handle Lives On (But Shouldn't Be Used)
After despawning, you still have the `Entity` struct in your variable – it's just a value type. But it's now a "dead" reference. Attempting CRUD operations on it will throw.
```cs
var fox = world.Spawn();
fox.Despawn();

fox.Add<Fluffy>();  // ❌ Throws ObjectDisposedException!
```
:::

## Usage Examples

### Basic Despawn
```cs
var enemy = world.Spawn().Add<Enemy>();
// ... enemy gets defeated ...
enemy.Despawn();
```

### Conditional Despawn
```cs
if (entity.Has<Health>())
{
    ref var health = ref entity.Ref<Health>();
    if (health.Value <= 0)
    {
        entity.Despawn();
    }
}
```

### Bulk Despawn via Query
```cs
// Despawn all dead entities
var deadQuery = world.Query<Health>().Build();

foreach (var entity in deadQuery)
{
    if (entity.Ref<Health>().Value <= 0)
    {
        entity.Despawn();
    }
}
```

### Despawn with Deferred Execution
```cs
// Inside a Stream runner, despawns are deferred
var stream = world.Query<Health>().Stream();

stream.For((Entity entity, ref Health health) =>
{
    if (health.Value <= 0)
    {
        entity.Despawn();  // Deferred until runner completes
    }
});
// All despawns happen here, after the runner finishes
```

## Deferred Despawning :neofox_think:

When you're inside a [Stream](/docs/Streams/) runner (like `For`, `Job`, or `Raw`), despawns are **deferred** until the runner completes. This is important!

```cs
stream.For((Entity entity, ref Health health) =>
{
    entity.Despawn();
    
    // Entity is still "Alive" here during deferred mode!
    Console.WriteLine(entity.Alive);  // true (until runner ends)
});
// NOW they're all despawned
```

::: info :neofox_science: Why Deferred?
Immediate despawns during iteration would invalidate the iterator and cause chaos. **fenn**ecs automatically batches structural changes (spawns, despawns, component adds/removes) until it's safe to apply them.
:::

To manually control this behavior, you can use World locks:

```cs
using var worldLock = world.Lock();

foreach (var entity in query)
{
    if (shouldDespawn)
    {
        entity.Despawn();  // Deferred while lock is held
        Console.WriteLine(entity.Alive);  // Still true!
    }
}
// Lock disposed here - despawns applied
```

## Relations and Despawning

When you despawn an entity that is the **target** of relations, those relations are automatically cleaned up:

```cs
var parent = world.Spawn();
var child = world.Spawn();

child.Add<ChildOf>(parent);  // child --ChildOf--> parent

parent.Despawn();  // The ChildOf relation on 'child' is also removed!

Console.WriteLine(child.Has<ChildOf>(parent));  // false
```

::: tip :neofox_comfy: No Dangling Relations
**fenn**ecs ensures you never have relations pointing to dead entities. When a relation target is despawned, all relations to it are automatically removed.
:::

## When to Despawn

| Scenario | Approach |
|----------|----------|
| Entity defeated/destroyed | Despawn immediately or after death animation |
| Temporary effects | Despawn when duration expires |
| Pooled objects | Consider removing components instead of despawning |
| Scene transitions | Despawn all entities, or use multiple Worlds |

## Constraints

- Only living entities can be despawned (despawning twice throws)
- Despawn is immediate outside of locked/runner contexts
- Despawn is deferred inside Stream runners or while World is locked
