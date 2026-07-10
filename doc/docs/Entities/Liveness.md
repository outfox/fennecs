---
title: Liveness
layout: doc
order: 8
outline: [1, 2]
---

# Entity Liveness :neofox_comfy:

::: tip :neofox_thumbsup: Is It Alive?
Every entity knows whether it's alive or not. This is fundamental to safely working with entities in **fenn**ecs!
:::

## `Entity.Alive`

The `Alive` property tells you whether an entity currently exists in its World.

```cs
var fox = world.Spawn();
Console.WriteLine(fox.Alive);  // true

fox.Despawn();
Console.WriteLine(fox.Alive);  // false
```

::: code-group
```cs [Code]
var entity = world.Spawn();
if (entity.Alive) Console.WriteLine(entity);
entity.Despawn();
if (!entity.Alive) Console.WriteLine(entity);
```
```plaintext [Output]
E-00000001:00001 <fennecs.Identity>
E-00000001:00001 -DEAD-
```
:::

## Implicit Bool Conversion :neofox_heart:

Entities can be used directly in boolean contexts – they implicitly convert to their `Alive` state:

```cs
var entity = world.Spawn();

if (entity)  // Same as: if (entity.Alive)
{
    Console.WriteLine("Entity is alive!");
}

entity.Despawn();

if (!entity)  // Same as: if (!entity.Alive)
{
    Console.WriteLine("Entity is dead!");
}
```

## How Identity Recycling Works :neofox_science:

Entities use a **generational index** system (similar to a [Sparse Set](https://www.codeproject.com/Articles/859324/Fast-Implementations-of-Sparse-Sets-in-Cplusplus)):

1. Each entity has an **index** (which slot it occupies)
2. Each entity has a **generation** (how many times that slot has been reused)
3. When despawned, the generation increments and the slot returns to the pool

This means:
- Entity IDs are always unique within a World's lifetime
- Stale entity handles can be detected (generation mismatch)
- Memory is reused efficiently

```cs
var first = world.Spawn();   // Index 1, Generation 1
first.Despawn();

var second = world.Spawn();  // Index 1, Generation 2 (reused slot!)

Console.WriteLine(first.Alive);   // false (generation mismatch)
Console.WriteLine(second.Alive);  // true
```

## Deferred Operations & Liveness :neofox_think:

::: warning :neofox_owo: Entities Stay "Alive" During Deferred Mode
When inside a [Stream](/docs/Streams/) runner or while holding a World lock, structural changes (including despawns) are **deferred**. The entity will report as `Alive` until the deferred operations are applied!
:::

```cs
var stream = world.Query<Health>().Stream();

stream.For((Entity entity, ref Health health) =>
{
    entity.Despawn();
    
    // Still reports alive during deferred execution!
    Console.WriteLine(entity.Alive);  // true
});

// After runner completes, despawns are applied
// Now the entities would report as dead
```

### Manual World Locking

You can manually defer operations using a World lock:

```cs
using var worldLock = world.Lock();

foreach (var entity in query)
{
    if (Random.Shared.NextSingle() >= 0.5f)
    {
        entity.Despawn();
    }
    
    // Entity still "Alive" while lock is held
    if (entity.Alive) 
    {
        Console.WriteLine("Dead Fox Walking!");
    }
}

// Lock disposed here - despawns are applied
```

## Best Practices

### Check Before Operating on Stored Entities

If you store entity references (in lists, dictionaries, components), check liveness before using them:

```cs
// Stored entity reference
private Entity _target;

public void Update()
{
    if (!_target.Alive)
    {
        _target = FindNewTarget();
    }
    
    // Safe to use now
    ref var pos = ref _target.Ref<Position>();
}
```

### Don't Store Entity References Long-Term (If Possible)

Queries and Streams give you fresh, valid entity references each iteration. Prefer these over storing entities when possible.

### Handle Dead Entities Gracefully

```cs
public void DamageEntity(Entity target, int damage)
{
    if (!target.Alive)
    {
        Console.WriteLine("Target already dead!");
        return;
    }
    
    ref var health = ref target.Ref<Health>();
    health.Value -= damage;
    
    if (health.Value <= 0)
    {
        target.Despawn();
    }
}
```

## Quick Reference

| Property/Pattern | Description |
|-----------------|-------------|
| `entity.Alive` | Returns `true` if entity exists in world |
| `if (entity)` | Implicit bool conversion to `Alive` |
| Deferred mode | Entity reports `Alive` until lock released |
| Generational ID | Prevents stale handle collisions |