---
layout: doc
title: Entities
order: 3
outline: [1, 2]
---

# Entities

![fennecs in a box](/img/fennecs-512.png)

*cuddly, lively, come in litters of `1,073,741,824`* :neofox_snuggle:

::: tip :neofox_thumbsup: The Heart of Your Game State
Entities are your actors, your game objects, your... well, *things*! They're lightweight handles that you attach components to, creating rich, composable game state.
:::

## What is an Entity?

An `Entity` is a lightweight, immutable handle – technically a `readonly record struct`. It's just an identity paired with a reference to its World.

```cs
var player = world.Spawn();           // Create an entity
player.Add(new Health { Value = 100 }); // Give it components
player.Add<Player>();                   // Tag it as the player
```

You can store entities anywhere: in variables, collections, or even as components on other entities!

## Quick Reference

| Operation | Method | Description |
|-----------|--------|-------------|
| Create | [`World.Spawn()`](Spawning.md) | Spawn a new entity |
| Destroy | [`Entity.Despawn()`](Despawning.md) | Remove entity from world |
| Add data | [`Entity.Add<C>()`](ComponentAdd.md) | Attach a component |
| Remove data | [`Entity.Remove<C>()`](ComponentRemove.md) | Detach a component |
| Check data | [`Entity.Has<C>()`](ComponentHas.md) | Check if component exists |
| Read/Write | [`Entity.Ref<C>()`](ComponentRefGet.md) | Get reference to component |
| Get or Create | [`Entity.Ensure<C>()`](ComponentEnsure.md) | Ensure component exists |
| Check alive | [`Entity.Alive`](Liveness.md) | Is entity still valid? |

## Lifecycle :neofox_snug:

Entities have a simple, predictable lifecycle:

1. **Spawn** - Created via `World.Spawn()` or `EntitySpawner`
2. **Live** - Exists in the world, can have components attached
3. **Despawn** - Removed from world, recycled for reuse

```cs
var fox = world.Spawn();
Console.WriteLine(fox.Alive); // true

fox.Despawn();
Console.WriteLine(fox.Alive); // false
```

Each Entity knows if it is [alive](Liveness.md) inside a World. An Entity can only live in one World at a time – and it needs a World to be alive. *(don't we all!)*

::: info :neofox_science: Memory Efficient
Despawned entities are recycled, so spawning and despawning is extremely cheap – even in large waves – without runaway memory consumption.
:::

## Composition :neofox_heart:

Entities can have any number of [Components](/docs/Components/) attached to them. This is how **fenn**ecs provides composable, structured data semantics.

```cs
// Build up an entity with multiple components
var enemy = world.Spawn()
    .Add(new Position { X = 10, Y = 20 })
    .Add(new Velocity { X = -1, Y = 0 })
    .Add(new Health { Value = 50 })
    .Add<Enemy>();  // Tag component
```

### Component Types

| Type | Example | Use Case |
|------|---------|----------|
| **Plain** | `Add(new Position())` | Regular data |
| **Tag** | `Add<Enemy>()` | Zero-size markers |
| **Relation** | `Add<Likes>(otherEntity)` | Entity-to-entity relationships |
| **Object Link** | `Add(Link.With(gameObject))` | Links to managed objects |

Entities can also serve as the *secondary key* in a [Relation](/docs/Keys/Relation.md) between two entities.

### Archetypes

Entities with identical combinations of component [Type Expressions](/docs/Components/Expressions.md) share the same **Archetype**. This is how **fenn**ecs achieves blazing-fast iteration – entities with the same "shape" are stored together in contiguous memory.

::: warning :neofox_think: A Dead Entity Has No Components
When an entity is despawned, all its components are removed. The entity handle becomes a stale reference to a recycled identity.
:::

## Internals

::: details :neofox_magnify: Tidbits for the Curious
The defining property of an entity is its `Identity` – a 64-bit value combining an index and a generation counter. Paired with a specific [World](/docs/World.md), this gives us a unique handle to operate on.

A dead Entity doesn't exist in any World – it's just stale data with a leftover `Identity` whose successor was already returned to the internal `IdentityPool`.

Living Entities occupy a slot in the world's storage structure:

- A `Meta` entry in the world's Meta-Set (tracking archetype membership)
- A row in their current Archetype's storage (`Storage<Identity>`)

The generation counter ensures that even recycled entity slots produce unique identities, preventing accidental access to "wrong" entities.
:::
