---
title: Check for Component
order: 5
outline: [1, 2]
---

# Checking for Components :neofox_verified:

::: warning :neofox_flop_blep: Usually Not Needed!
In ECS-land, we prefer to let **Queries** do the filtering! A Query with `<Health, Position>` automatically gives you only entities that have both.

But sometimes you *do* need to check manually – and that's okay!
:::

### `Has<C>(...)`

The `Has` method checks whether an entity has a specific component. It returns `true` if the component exists, `false` otherwise.

## Method Signatures

| Signature | Description |
|-----------|-------------|
| `Entity.Has<C>()` | Checks for a plain component |
| `Entity.Has<C>(Entity relation)` | Checks for a relation to a specific entity |
| `Entity.Has<L>(L linkedObject)` | Checks for a link to a specific object |
| `Entity.Has<L>(Link<L> link)` | Checks for a link by its wrapper |
| `Entity.Has<C>(Match match)` | Checks using a match expression (supports wildcards) |

All overloads return `bool`.

## Usage Examples

### Basic Check
```cs
if (entity.Has<Health>())
{
    Console.WriteLine("This entity can take damage!");
}

if (!entity.Has<Invincible>())
{
    ref var health = ref entity.Ref<Health>();
    health.Value -= 10;
}
```

### Checking Relations
```cs
var team = world.Spawn();

if (entity.Has<MemberOf>(team))
{
    Console.WriteLine("Entity is on the team!");
}

// Check if entity has ANY team relation
if (entity.Has<MemberOf>(Entity.Any))
{
    Console.WriteLine("Entity belongs to some team");
}
```

### Checking Object Links
```cs
var player = GetPlayerGameObject();

if (entity.Has(player))
{
    Console.WriteLine("This entity is linked to the player!");
}

// Check if entity has ANY link of this type
if (entity.Has<GameObject>(Link.Any))
{
    Console.WriteLine("Entity has some GameObject link");
}
```

### Using Match Expressions
```cs
// Check for plain component
entity.Has<int>(Match.Plain);

// Check for ANY int component (plain, relation, or link)
entity.Has<int>(Match.Any);

// Check for any Entity relation of type int
entity.Has<int>(Entity.Any);

// Check for any Object Link of type MyClass
entity.Has<MyClass>(Link.Any);
```

## When to Use `Has`

::: tip :neofox_comfy: The Right Tool for the Job
Most of the time, Queries are your friend. But `Has` shines in these scenarios:
:::

| Scenario | Why Use `Has` |
|----------|---------------|
| **Conditional logic in handlers** | UI button clicked, need to check entity state |
| **Guard clauses** | Before calling `Ref<C>()` on uncertain entities |
| **Debugging** | Inspecting entity composition at runtime |
| **Serialization** | Checking what to serialize |
| **One-off checks** | Not worth creating a Query for a single check |

### Anti-Pattern: Filtering in Loops

```cs
// ❌ Don't do this - use a Query instead!
foreach (var entity in allEntities)
{
    if (entity.Has<Enemy>() && entity.Has<Health>())
    {
        // process enemy...
    }
}

// ✅ Do this - Query filters for you!
var enemyQuery = world.Query<Enemy, Health>().Build();
foreach (var entity in enemyQuery)
{
    // All entities here have Enemy AND Health
}
```

## Has vs Query Matching

| Approach | Use When |
|----------|----------|
| `entity.Has<C>()` | One-off check on a single entity |
| `Query<C>.Build()` | Repeatedly iterating entities with component |
| Stream filters | Dynamic filtering within iteration |

## Safe Access Pattern

Combine `Has` with `Ref` for safe component access:

```cs
// Check before access
if (entity.Has<Health>())
{
    ref var health = ref entity.Ref<Health>();
    health.Value -= damage;
}

// Or use Ensure for get-or-create semantics
ref var health = ref entity.Ensure(new Health { Value = 100 });
```

## Constraints

- `C` must be `notnull`
- For `Link<L>`, `L` must be a reference type (`class`)
- Returns `bool` only – cannot be chained

::: info :neofox_science: Performance Note
`Has` performs a lookup in the entity's archetype signature. It's fast, but iterating a Query is faster when you need to process many entities with the same component requirements.
:::
