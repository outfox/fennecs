---
title: Spawn
menu: Spawn
order: 1
outline: [1, 2]
description: 'Creating single entities in fennecs with World.Spawn - the entity springs to life immediately, ready for fluent Add calls to attach components.'
---

# Spawning Entities

::: tip :neofox_thumbsup: Bringing Entities to Life
Every entity starts here! Spawning is how you create new entities in your World, ready to receive components and participate in your game.
:::

The `World.Spawn()` method is the simplest way to create a new entity. It returns the entity immediately, and you can chain `Add` calls to attach components.

```cs
var world = new World();

// Spawn a single entity
var entity = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add<Velocity>(); // velocity has new(), so default value is used
```

In this example, we create a new entity using `World.Spawn()` and add two components (`Position` and `Velocity`) to it using the `Add` method. The entity is automatically spawned in the world after the components are added.

The handle you get back is a plain `Entity` value — store it in variables, collections, or even as a component on another entity. It stays valid until the entity [despawns](Despawning.md), and it always knows whether it's still [alive](Liveness.md).

::: info :neofox_think: Paws for Thought: Archetype Churning
Each `Add` call moves the entity to a new archetype. For simple entities, this is fine! But for complex entities with many components — or when spawning whole waves — reach for the [`EntitySpawner`](/docs/Advanced/BulkSpawn.md) instead, which spawns entities directly into their final archetype.
:::

## Quick Reference

| Scenario | Recommended |
|----------|-------------|
| Single entity, few components | `World.Spawn()` |
| Prototyping/debugging | `World.Spawn()` |
| Single entity, many components | [`EntitySpawner`](/docs/Advanced/BulkSpawn.md) |
| Bulk spawning (10+ entities) | [`EntitySpawner`](/docs/Advanced/BulkSpawn.md) |
| Entity templates/factories | [`EntitySpawner`](/docs/Advanced/BulkSpawn.md) |

::: tip :neofox_science: Need a Wave, not a Fox?
[Bulk Spawn](/docs/Advanced/BulkSpawn.md) covers the `EntitySpawner`: reusable templates, spawning 100,000 entities in one call, and getting all their handles delivered straight into your `Span<Entity>`.
:::
