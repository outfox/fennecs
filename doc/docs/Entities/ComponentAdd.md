---
title: Add Component
order: 3
outline: [1, 2]
---

# Adding Components :neofox_cute_reach:

::: tip :neofox_thumbsup: Building Blocks of Your Entities
Components are data! Adding them to entities is how you give your game objects properties, behaviors, and meaning. You'll be doing this *a lot*!
:::

### :neofox_hug_haj_heart: `Add<C>(...)`

The `Add` method attaches a component to an entity. It returns the entity itself, enabling fluent method chaining.

## Method Signatures

| Signature | Description |
|-----------|-------------|
| `Entity.Add<C>()` | Adds a default component (type must have `new()`) |
| `Entity.Add<C>(C value)` | Adds a component with the specified value |
| `Entity.Add<C>(Entity relation)` | Adds a relation component to another entity |
| `Entity.Add<C>(C value, Entity relation)` | Adds a relation with a backing value |
| `Entity.Add<L>(Link<L> link)` | Adds an object link to a managed object |

All overloads return the `Entity`, allowing fluent chaining.

::: warning :neofox_dizzy: No Duplicates Allowed!
You cannot add the same component type twice (with the same match expression). Attempting to do so will throw an exception.
```cs
entity.Add<int>(42);
entity.Add<int>(99);  // ❌ Throws! Already has plain int
```
Use [`Ensure<C>()`](ComponentEnsure.md) if you want "add if missing" semantics.
:::

## Usage Examples

### Plain Components
```cs
// Add with explicit value
entity.Add(new Position { X = 10, Y = 20 });
entity.Add(new Health { Value = 100 });

// Add with default value (type needs new())
entity.Add<Velocity>();  // Velocity with default values
```

### Tag Components (Zero-Size)
```cs
// Tags are empty structs - great for marking/categorizing
public struct Enemy;
public struct Poisoned;
public struct CanFly;

entity.Add<Enemy>();
entity.Add<Poisoned>();
entity.Add<CanFly>();
```

### Fluent Chaining
```cs
var player = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity())
    .Add(new Health { Value = 100 })
    .Add(new Mana { Value = 50 })
    .Add<Player>()
    .Add<Controllable>();
```

### Entity Relations
```cs
var parent = world.Spawn();
var child = world.Spawn();

// Relation with default backing value
child.Add<ChildOf>(parent);

// Relation with explicit backing value
child.Add(new Offset { X = 5, Y = 0 }, parent);

// The child now has a relation component pointing to parent
Console.WriteLine(child.Has<ChildOf>(parent));  // true
```

### Object Links
```cs
// Link to a managed object (Unity GameObject, texture, etc.)
var gameObject = new GameObject("Player");
entity.Add(Link.With(gameObject));

// Multiple links of different types
var texture = LoadTexture("player.png");
var audioClip = LoadAudio("footsteps.wav");

entity
    .Add(Link.With(gameObject))
    .Add(Link.With(texture))
    .Add(Link.With(audioClip));
```

::: info :neofox_science: Object Links Create Archetypes
Each unique object link creates a distinct archetype. Entities linked to the *same* object share an archetype, which can be useful for grouping!
:::

## Multiple Components of the Same Type

An entity can have multiple components of the same backing type if they have different **match expressions**:

```cs
var target1 = world.Spawn();
var target2 = world.Spawn();

// All of these are different components!
entity.Add<int>(100);              // Plain int
entity.Add<int>(50, target1);      // int relation to target1
entity.Add<int>(25, target2);      // int relation to target2

// Entity now has THREE int components
Console.WriteLine(entity.Get<int>(Match.Any).Length);  // 3
```

## When to Use `Add` vs `Ensure`

| Scenario | Recommended |
|----------|-------------|
| Initial entity setup | `Add` ✅ |
| Component definitely doesn't exist | `Add` ✅ |
| Component might already exist | `Ensure` |
| Counter/accumulator patterns | `Ensure` |
| Lazy initialization | `Ensure` |

## Constraints

- `C` must be `notnull` (no nullable types)
- For `Add<C>()` (no value), `C` must also have `new()` constraint
- Cannot add a component that already exists (same type + match)
- For `Link<L>`, `L` must be a reference type (`class`)

::: tip :neofox_comfy: Structural Changes
Adding a component is a **structural change** – it moves the entity to a new archetype. Inside [Stream](/docs/Streams/) runners, these are deferred until the runner completes.
:::
