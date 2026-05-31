---
title: Spawn
order: 1
outline: [1, 2]
---

# Spawning Entities :neofox_hyper:

::: tip :neofox_thumbsup: Bringing Entities to Life
Every entity starts here! Spawning is how you create new entities in your World, ready to receive components and participate in your game.
:::

In **fenn**ecs, there are two primary ways to spawn entities:

| Method | Best For |
|--------|----------|
| `World.Spawn()` | Quick, one-off entities |
| `World.Entity()` | Bulk spawning, templates |

## Quick & Easy Spawns :neofox_comfy:

The `World.Spawn()` method is the simplest way to create a new entity. It returns the entity immediately, and you can chain `Add` calls to attach components.

```cs
var world = new World();

// Spawn a single entity
var entity = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add<Velocity>(); // velocity has new(), so default value is used
```

In this example, we create a new entity using `World.Spawn()` and add two components (`Position` and `Velocity`) to it using the `Add` method. The entity is automatically spawned in the world after the components are added.

::: info :neofox_think: Paws for Thought: Archetype Churning
Each `Add` call moves the entity to a new archetype. For simple entities, this is fine! But for complex entities with many components, or when spawning thousands, consider using `EntitySpawner` instead.
:::

## Fast, Flexible Spawns :neofox_hyper:

The `EntitySpawner` is perfect for bulk spawning or creating entity templates. Get one via `World.Entity()`, configure it with components, then spawn as many entities as you need!

**Benefits:**
- Entities spawn directly into their final archetype (no churning!)
- Data is blitted directly to storage (lightning-fast)
- Reusable for spawning waves of similar entities

### Typical Use
```cs
var world = new World();

using var spawner = world.Entity() // Requests an EntitySpawner
    .Add(new Velocity { X = 50 })
    .Add<Bee>()
    .Spawn(100_000); // AAAAAAAAAA!
```

::: tip :neofox_thumbsup: Dispose for Extra Credit
`EntitySpawner` implements `IDisposable` to return pooled data structures for reuse. No memory leaks either way, but disposing is a nice habit!
:::


### Repeat Spawns & Templates

Spawners can be modified and reused:

```cs
world.Entity()
    .Add(new Health { Value = 100 })
    .Add<Dexterity>(12) // Stats do well with conversion operators for int!
    .Add<Charisma>(15)
    //.Add<...> (more omitted here)
    .Add<Human>()
    .Spawn()    // Spawn a single human
    .Dispose(); // immediately dispose the spawner after we used it

// The using statement disposes the spawner at end of scope/function
using var werewolfSpawner = world.Entity()
    .Add(new Health { Value = 250 })
    .Add<Werewolf>()
    .Spawn(9); // 9 regulars

werewolfSpawner.Add<Elite>(); // anything it spawns from now on has Elite!
werewolfSpawner.Spawn(5); //+5 Elites, giving the BEST NUMBER of werewolves: 14!

return; // werewolfSpawner is automatically disposed here by the using statement.
```


## Quick Reference

| Scenario | Recommended |
|----------|-------------|
| Single entity, few components | `World.Spawn()` |
| Single entity, many components | `EntitySpawner` |
| Bulk spawning (10+ entities) | `EntitySpawner` |
| Entity templates/factories | `EntitySpawner` |
| Prototyping/debugging | `World.Spawn()` |

::: info :neofox_science: Performance Note
For spawning 100+ similar entities, `EntitySpawner` can be 10-100x faster than individual `World.Spawn()` calls due to direct archetype placement and memory blitting.
:::
