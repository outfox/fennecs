---
title: Tags
order: 2
outline: [1, 2]
---

# Tag Components :neofox_bongo_down:

![an invisible fennec](/img/fennec-void-darkmode.png){.dark-only} ![an invisible fennec](/img/fennec-void-lightmode.png){.light-only}

::: tip :neofox_thumbsup: Zero-Cost Markers
Tags are components that store no data – just their presence carries meaning! They're perfect for categorizing entities and filtering with Queries. Best of all? They use practically zero memory.
:::

## What is a Tag?

A Tag is simply an empty struct. It marks an entity as having a certain trait or belonging to a category, without storing any actual data.

```cs
public struct Enemy;
public struct Poisoned;
public struct CanFly;
public struct Player;
```

## Usage Examples

### Marking Entities

```cs
var player = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Health { Value = 100 })
    .Add<Player>();  // Tag: this is the player!

var enemy = world.Spawn()
    .Add(new Position { X = 50, Y = 30 })
    .Add(new Health { Value = 25 })
    .Add<Enemy>()    // Tag: this is an enemy
    .Add<CanFly>();  // Tag: it can fly!
```

### Querying with Tags

```cs
// Find all enemies
var enemies = world.Query<Health>()
    .Has<Enemy>()
    .Build();

// Find all flying enemies
var flyingEnemies = world.Query<Position, Health>()
    .Has<Enemy>()
    .Has<CanFly>()
    .Build();

// Find entities that are NOT enemies
var friendlies = world.Query<Position>()
    .Not<Enemy>()
    .Build();
```

### Adding and Removing Tags

```cs
// Add a status effect tag
entity.Add<Poisoned>();

// Check for tag
if (entity.Has<Poisoned>())
{
    Console.WriteLine("Entity is poisoned!");
}

// Remove the tag
entity.Remove<Poisoned>();
```

### The Classic Example :neofox_comfy:

```cs
public struct Pretty;
public struct Smart;
public struct Cool;

var you = world.Spawn()
    .Add<Pretty>()
    .Add<Smart>()
    .Add<Cool>();

var fennecsUsers = world.Query()
    .Has<Pretty>()
    .Has<Smart>()
    .Has<Cool>()
    .Compile();

Assert.Contains(you, fennecsUsers);  // ✅ You're in!
```

## Defining Tags

::: code-group
```cs [Simple Struct]
// Most minimal definition
public struct Enemy;
public struct Friendly;
public struct Active;
```

```cs [Record Struct]
// Works too, nicer formatting in mixed codebases
public record struct Enemy;
public record struct Friendly;
public record struct Active;
```
:::

## Common Use Cases

| Use Case | Example Tags |
|----------|--------------|
| **Entity Types** | `Player`, `Enemy`, `NPC`, `Projectile` |
| **Status Effects** | `Poisoned`, `Stunned`, `Invincible`, `Dead` |
| **States** | `Active`, `Paused`, `Loading`, `Ready` |
| **Categories** | `Friendly`, `Hostile`, `Neutral` |
| **Capabilities** | `CanFly`, `CanSwim`, `CanAttack` |
| **Markers** | `JustSpawned`, `NeedsUpdate`, `Dirty` |

## Tags in Queries :neofox_heart:

Tags are *excellent* for Query filtering – that's their primary purpose!

```cs
// Good: Use Has<> for filtering
var enemies = world.Query<Position, Health>()
    .Has<Enemy>()
    .Build();

foreach (var entity in enemies)
{
    // Process enemy...
}
```

::: warning :neofox_think: Tags in Stream Types
Tags can be used as Stream Types, but they're not particularly useful there since they have no data to access:

```cs
// This works, but the Enemy ref is useless
stream.For((ref Position pos, ref Enemy tag) =>
{
    // 'tag' has no data - waste of a stream slot!
});

// Better: use Has<> filter instead
var enemyStream = world.Query<Position>()
    .Has<Enemy>()
    .Stream();

enemyStream.For((ref Position pos) =>
{
    // Cleaner, and you have all 5 stream slots for real data!
});
```
:::

## Tags as Relation Markers

Tags can also be used with relations – the tag itself has no backing data, but the relation target provides meaning:

```cs
public struct Likes;
public struct MemberOf;

// Entity-to-entity relations with tag backing
alice.Add<Likes>(bob);     // Alice likes Bob
alice.Add<MemberOf>(team); // Alice is member of team
```

## Quick Reference

| Aspect | Tags |
|--------|------|
| **Memory** | Zero storage cost |
| **Purpose** | Marking, categorizing, filtering |
| **Query Use** | `Has<T>()`, `Not<T>()`, `Any<T>()` |
| **Best For** | Entity classification |

::: info :neofox_science: Why No Memory?
Empty structs in .NET have a minimum size of 1 byte in isolation, but **fenn**ecs optimizes tag storage. The archetype knows which entities have which tags through its signature – no per-entity storage needed!
:::