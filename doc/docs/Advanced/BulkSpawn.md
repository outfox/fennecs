---
title: Bulk Spawn
menu: Bulk Spawn
order: 3
outline: [1, 2]
description: 'Fast bulk spawning in fennecs with the EntitySpawner (World.Entity): reusable templates, spawning waves straight into their final Archetype, and getting spawned Entity handles back via Span.'
---

# Bulk Spawning

::: tip :neofox_thumbsup: Waves, Templates & Factories
Need a hundred thousand entities? A reusable template for your werewolf packs? The `EntitySpawner` spawns entities *directly into their final Archetype* — no churning, no ceremony, all speed.
:::

The `EntitySpawner` is perfect for bulk spawning or creating entity templates. Get one via `World.Entity()`, configure it with components, then spawn as many entities as you need!

**Benefits:**
- Entities spawn directly into their final archetype (no churning!)
- Data is blitted directly to storage (lightning-fast)
- Reusable for spawning waves of similar entities

## Typical Use
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


## Repeat Spawns & Templates

Spawners can be modified and reused:

```cs
using var humanSpawner = world.Entity()
    .Add(new Health { Value = 100 })
    .Add<Dexterity>(12) // Stats do well with conversion operators for int!
    .Add<Charisma>(15)
    //.Add<...> (more omitted here)
    .Add<Human>();

var him = humanSpawner.Spawn(); // Spawn a single human... 
var her = humanSpawner.Spawn(); // ... and another! (Spawn() returns the Entity)

// The using statement disposes the spawner at end of scope/function
using var werewolfSpawner = world.Entity()
    .Add(new Health { Value = 250 })
    .Add<Werewolf>()
    .Spawn(9); // 9 regulars

werewolfSpawner.Add<Elite>(); // anything it spawns from now on has Elite!
werewolfSpawner.Spawn(5); //+5 Elites, giving the BEST NUMBER of werewolves: 14!

return; // werewolfSpawner is automatically disposed here by the using statement.
```


## Entity Returns (for further processing)

Sometimes you spawn a wave and want the handles right away — to wire up relations, hand them to game logic, or track them somewhere. Each `Spawn` overload has you covered:

| Overload | Returns | Use When |
|----------|---------|----------|
| `Spawn()` | `Entity` | You want one entity and its handle |
| `Spawn(int count)` | `EntitySpawner` (fluent) | Fire-and-forget bulk spawning |
| `Spawn(Span<Entity> destination)` | `EntitySpawner` (fluent) | You want the handles of a whole wave |

The `Span` overload spawns **one entity per element** of the span and writes their handles into it — the span's length *is* the spawn count. The handles are plain `Entity` values (not views into World storage), so they stay valid until the entities despawn. Keep them as long as you like!

```cs
// A single entity, handle returned directly:
var leader = world.Entity()
    .Add<Werewolf>()
    .Spawn();

// A whole pack, delivered into your buffer (array, stackalloc, wherever):
var pack = new Entity[13];
using var spawner = world.Entity()
    .Add<Werewolf>()
    .Spawn(pack); // fills all 13 slots, still fluent!

foreach (var wolf in pack) wolf.Add<PackMember>(leader); // relation to the leader
```

::: tip :neofox_science: Zero-Cost Delivery
The handles are minted directly into your span during the spawn — no copying, no allocation. Fire-and-forget `Spawn(count)` stays just as fast as before.
:::


## Quick Reference

| Scenario | Recommended |
|----------|-------------|
| Single entity, few components | [`World.Spawn()`](/docs/Entities/Spawning.md) |
| Single entity, many components | `EntitySpawner` |
| Bulk spawning (10+ entities) | `EntitySpawner` |
| Entity templates/factories | `EntitySpawner` |
| Prototyping/debugging | [`World.Spawn()`](/docs/Entities/Spawning.md) |

::: info :neofox_science: Performance Note
For spawning 100+ similar entities, `EntitySpawner` can be 10-100x faster than individual `World.Spawn()` calls due to direct archetype placement and memory blitting.
:::
