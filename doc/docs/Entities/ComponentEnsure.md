---
title: Ensure Component
order: 7
outline: [1, 2]
---

# Ensuring a Component Exists

::: tip :neofox_thumbsup: Get-or-Create Pattern
`Ensure<C>` is perfect for when you want to work with a component without caring whether it already exists. It's the "just make sure it's there" method!
:::

### :neofox_comfy: `Ensure<C>(...)` :neofox_heart:

The `Ensure` method guarantees a component exists on an entity and returns a reference to it. If the component doesn't exist, it's created with the specified default value. If it already exists, the existing value is preserved.

## Method Signatures

- `Entity.Ensure<C>()`: Ensures a plain component of type `C` exists, initializing with `default(C)` if missing.
- `Entity.Ensure<C>(C defaultValue)`: Ensures a plain component exists, initializing with the specified value if missing.
- `Entity.Ensure<C>(C defaultValue, Entity relation)`: Ensures a relation component exists to the specified entity.
- `Entity.Ensure<C>(C defaultValue, Match match)`: Ensures a component exists with the specified match expression.

All overloads return a `ref C` - a direct reference to the component that you can read or modify.

::: warning :neofox_dizzy: Dangling References
The returned reference becomes invalid if the Entity's archetype changes (e.g., by adding or removing other components). Don't hold references across structural changes!
```cs
ref var a = ref entity.Ensure<int>();    // ✅ Valid
entity.Add<float>();                      // Archetype changes!
a = 42;                                   // ❌ Dangling reference!
```
:::

## Usage Examples

### Basic Usage - Counter Pattern
```cs
// Increment a counter, creating it if needed
entity.Ensure<int>()++;
entity.Ensure<int>()++;
entity.Ensure<int>()++;
// entity now has int component with value 3
```

### Initialize with Default Value
```cs
// Ensure entity has Health, defaulting to 100 if missing
ref var health = ref entity.Ensure(100);
health -= 10; // Take damage
```

### Working with Structs
```cs
public struct Stats
{
    public int Strength;
    public int Agility;
}

// Ensure Stats exist with initial values
ref var stats = ref entity.Ensure(new Stats 
{ 
    Strength = 10, 
    Agility = 15 
});

// Modify directly through the reference
stats.Strength += 5;
```

### With Entity Relations
```cs
var target = world.Spawn();

// Ensure a damage relation to target, defaulting to 50
ref var damage = ref entity.Ensure(50, target);

// Both plain and relation components are independent
entity.Ensure(100);        // Plain int component
entity.Ensure(200, target); // Relation int component to target
// Entity now has two int components!
```

## When to Use `Ensure` vs `Add` + `Ref`

| Scenario | Recommended |
|----------|-------------|
| Component might or might not exist | `Ensure` ✅ |
| Component definitely doesn't exist | `Add` |
| Accumulating/counting | `Ensure` ✅ |
| One-time initialization | `Add` |
| Lazy initialization pattern | `Ensure` ✅ |

## Constraints

- `C` must be a `struct` type (value types only)
- For reference types, use `Add` and `Ref` separately

::: info :neofox_think: Why structs only?
The `Ensure` method uses `default(C)` as the fallback value. For reference types, `default` is `null`, which would violate **fenn**ecs' non-null component philosophy. Use `Add<T>(Link<T>)` for object links instead.
:::
