---
title: Remove Component
order: 4
outline: [1, 2]
---

# Removing Components :neofox_floof_sad_reach:

::: tip :neofox_thumbsup: Tidying Up
Sometimes components need to go! Removing components is just as important as adding them – it's how entities change over time and respond to game events.
:::

### :neofox_hug_haj: `Remove<C>(...)`

The `Remove` method detaches a component from an entity. It returns the entity itself, enabling fluent method chaining.

## Method Signatures

| Signature | Description |
|-----------|-------------|
| `Entity.Remove<C>()` | Removes a plain component |
| `Entity.Remove<C>(Entity relation)` | Removes a relation to a specific entity |
| `Entity.Remove<L>(L linkedObject)` | Removes a link to a specific object |
| `Entity.Remove<L>(Link<L> link)` | Removes a link by its wrapper |

All overloads return the `Entity`, allowing fluent chaining.

::: warning :neofox_owo: Component Must Exist!
Attempting to remove a component that doesn't exist will throw an exception. Use [`Has<C>()`](ComponentHas.md) to check first if you're unsure.
```cs
entity.Remove<Health>();  // ❌ Throws if no Health component!
```
:::

## Usage Examples

### Basic Removal
```cs
// Remove a status effect
entity.Remove<Poisoned>();

// Remove health component (entity is now invincible? 🤔)
entity.Remove<Health>();
```

### Fluent Chaining
```cs
// Clean up multiple components at once
entity
    .Remove<Stunned>()
    .Remove<Slowed>()
    .Remove<Confused>();
```

### Conditional Removal
```cs
// Only remove if present
if (entity.Has<Shield>())
{
    entity.Remove<Shield>();
    Console.WriteLine("Shield broken!");
}
```

### Removing Relations
```cs
var leader = world.Spawn();
var follower = world.Spawn();

follower.Add<FollowsEntity>(leader);

// Later, stop following
follower.Remove<FollowsEntity>(leader);

Console.WriteLine(follower.Has<FollowsEntity>(leader));  // false
```

### Removing Object Links
```cs
var gameObject = new GameObject("Effect");
entity.Add(Link.With(gameObject));

// Remove the link (doesn't destroy the GameObject!)
entity.Remove(gameObject);
// or equivalently:
entity.Remove(Link.With(gameObject));
```

## What Happens to the Data?

When you remove a component:

1. **The data is discarded**  –  The component value is gone (for value types) or dereferenced (for links)
2. **Archetype changes**  –  The entity moves to a new archetype without that component type
3. **Queries update**  –  The entity will no longer match queries requiring that component

::: info :neofox_think: Reference Types and Links
For object links, removing the link doesn't destroy or dispose the linked object – it just removes the association. The object continues to exist in managed memory.
:::

## Removing Multiple of Same Type

If an entity has multiple components of the same type (via relations), you must specify which one to remove:

```cs
var target1 = world.Spawn();
var target2 = world.Spawn();

entity.Add<int>(50, target1);
entity.Add<int>(25, target2);

// Remove specific relation
entity.Remove<int>(target1);  // Only removes the target1 relation

// target2 relation still exists
Console.WriteLine(entity.Has<int>(target2));  // true
```

## Use Cases

| Scenario | Example |
|----------|---------|
| Status effects expiring | `entity.Remove<Burning>()` |
| Equipment unequipped | `entity.Remove<Sword>(hand)` |
| Buff/debuff ended | `entity.Remove<SpeedBoost>()` |
| Clearing temporary state | `entity.Remove<JustSpawned>()` |
| Breaking relationships | `entity.Remove<Targeting>(enemy)` |

## Constraints

- `C` must be `notnull`
- The component must exist on the entity (throws otherwise)
- For `Link<L>`, `L` must be a reference type (`class`)

::: tip :neofox_comfy: Structural Changes
Removing a component is a **structural change** – it moves the entity to a new archetype. Inside [Stream](/docs/Streams/) runners, these are deferred until the runner completes.
:::
