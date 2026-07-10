---
title: Bulk CRUD
outline: [1, 2]
order: 3
---

# :neofox_hyper: Bulk CRUD

::: tip :neofox_thumbsup: POWER IN NUMBERS!
`Query` lets you Add or Remove components (and even Despawn!) in bulk  –  just like an individual `Entity`, but for thousands at once!
:::

## Quick Reference

| Method | Description |
|--------|-------------|
| `Query.Add<C>(value)` | Add a component to all matched Entities |
| `Query.Remove<C>()` | Remove a component from all matched Entities |
| `Query.Despawn()` | Despawn all matched Entities |
| `Query.Truncate(n, mode)` | Reduce matched Entities to a specific count |
| `Query.Batch()` | Begin a batch for multiple structural changes |

## Usage Examples

::: code-group
```cs [Damage System]
// Query for all entities with Health that also took Damage
var whoTookDamage = world.Query<Health, Damage, Identity>().Stream();

// Do something for each Entity in Query
whoTookDamage.For((in Entity entity, ref Health health, ref Damage damage) => 
{
    health.hp -= damage.amount;
    if (health.hp < 0) entity.Add<Exploding>();
});

// 👇 All damage values were accounted for! Bulk remove the values.
whoTookDamage.Remove<Damage>();
```

```cs [Death Explosion System]
// Query for all Entities that are about to Explode (and have a Position)
var whosExplodingWhere = world.Query<Position>().Has<Exploding>().Stream();

// Do something for each Entity in Query
whosExplodingWhere.For((ref Position position) => Game.SpawnExplosion(position));

// 👇 Despawn all Entities matched by Query
whosExplodingWhere.Despawn();
```

```cs [Multiple Bulk Operations]
var loadedGuns = world.Query()
    .Has<Gun>()
    .Not<Loaded>().Not<Cooldown>().Not<RequestProjectileSpawn>()
    .Stream();

// ⚠️ Multiple bulk operations may require batching, because
// entities wouldn't necessarily match our query after each change
// and thus "escape" before subsequent operations would be applied.
loadedGuns.Batch()
    .Add<Cooldown>(2.0f)  // for our cooldown system
    .Add<RequestProjectileSpawn>()  // for another system
    .Remove<Loaded>()
    .Submit();
```
:::

## Adding & Removing Components

| Method | Behavior |
|--------|----------|
| `Query.Add<C>(value)` | Adds component to each Entity. **Throws** if Query already matches that Component. |
| `Query.Remove<C>()` | Removes component from each Entity. **Throws** if Query doesn't match that Component. |

::: info :neofox_think: WHY THE EXCEPTIONS?
These safety checks prevent silent no-ops. If you're adding a component the Query already matches, either ALL entities already have it, or the query is empty. Same logic for removal.
:::

## Adding Links & Relations

Object Links and Entity Relations can also be added/removed in bulk using the same pattern:

```cs
// Add an object link to all matched entities
query.Add(Link.With(sharedTexture));

// Add a relation to all matched entities  
query.Add(Relate.To(targetEntity));
```

## Batch Operations

Structural changes often cause Entities to no longer match the Query. Use `Query.Batch()` to queue multiple changes:

```cs
var batch = query.Batch()
    .Add<Tag>()
    .Add<Health>(100)
    .Remove<Invulnerable>();

batch.Submit();  // Execute all at once
```

::: warning :neofox_nom_verified: BATCH LIFECYCLE
Once you `Submit()` a Batch, the World takes ownership. You only need to `Dispose()` the Batch if you decide *not* to submit it.
:::

### Handling Conflicts

Batch operations can encounter semantic conflicts when adding/removing components:

| Conflict | Example |
|----------|--------|
| `Add<T>` conflict | Some or all Entities already have that component |
| `Remove<T>` conflict | Not every Entity has that component |

Pass conflict resolution strategies to `Query.Batch(AddConflict, RemoveConflict)`:

#### AddConflict Options

| Option | Behavior |
|--------|----------|
| `Disallow` *(default)* | Throw if adding a component not explicitly excluded from query |
| `Preserve` | Keep existing values, add only where not present |
| `Replace` | Overwrite existing values, add where not present |

#### RemoveConflict Options

| Option | Behavior |
|--------|----------|
| `Disallow` *(default)* | Throw unless component is expressly included in Query |
| `Allow` | Skip Archetypes where component isn't present (idempotent, near-zero cost) |

```cs
// Example: Replace health values on all entities, even those that already have Health
query.Batch(AddConflict.Replace, RemoveConflict.Allow)
    .Add<Health>(100)
    .Submit();
```
