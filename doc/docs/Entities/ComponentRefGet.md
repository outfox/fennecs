---
title: Read/Write Component
order: 6
outline: [1, 2]
---

# Reading & Writing Components :neofox_peek:

::: warning :neofox_flop_blep: Consider Streams First!
For bulk operations, [Streams](/docs/Streams/) are the way to go – they're faster and more ergonomic for processing many entities.

But for one-off access? `Ref` and `Get` are *perfect*. Don't overthink it! 💙
:::

## When Direct Access Makes Sense

- **UI event handlers**  –  User clicked a button, update the entity
- **Serialization**  –  Save/load entity state
- **Debugging**  –  Inspect values at runtime
- **Tests**  –  Verify component values
- **Initialization**  –  Set up entity after spawning

```cs
// This is totally fine!
ref var health = ref entity.Ref<Health>();
health.Value = slider.Value;
```

## `Ref<C>()` - Get a Reference :neofox_thumbsup:

Returns a **reference** to the component, allowing both reading and writing.

### Method Signatures

| Signature | Description |
|-----------|-------------|
| `Entity.Ref<C>()` | Reference to a plain component |
| `Entity.Ref<C>(Match match)` | Reference with match expression |
| `Entity.Ref<L>(Link<L> link)` | Reference to a linked object |

### Usage Examples

```cs
// Read a component
ref var pos = ref entity.Ref<Position>();
Console.WriteLine($"Entity at ({pos.X}, {pos.Y})");

// Modify a component
ref var health = ref entity.Ref<Health>();
health.Value -= 10;
health.LastDamageTime = DateTime.Now;

// Modify struct fields directly
entity.Ref<Position>().X += velocity.X * deltaTime;
entity.Ref<Position>().Y += velocity.Y * deltaTime;
```

### With Relations
```cs
var target = world.Spawn();
entity.Add(new Distance { Value = 100 }, target);

// Get reference to relation component
ref var dist = ref entity.Ref<Distance>(target);
dist.Value -= 5;  // Getting closer!
```

### With Object Links
```cs
var gameObject = new GameObject("Player");
entity.Add(Link.With(gameObject));

// Get reference to the linked object itself
ref var go = ref entity.Ref(Link.With(gameObject));
go.SetActive(true);
```

::: warning :neofox_dizzy: Dangling References!
The returned reference becomes invalid if the entity's archetype changes. Don't hold references across structural changes!
```cs
ref var health = ref entity.Ref<Health>();  // ✅ Valid
entity.Add<Shield>();                        // Archetype changes!
health.Value = 100;                          // ❌ Dangling reference!
```
**Rule of thumb:** Use the reference immediately, don't store it.
:::

## `Get<C>()` - Get Multiple Values :neofox_science:

Returns an **array** of all matching component values. Useful with wildcards!

### Method Signature

| Signature | Description |
|-----------|-------------|
| `Entity.Get<C>(Match match)` | Array of all matching components |

### Usage Examples

```cs
// Get all int relations
var target1 = world.Spawn();
var target2 = world.Spawn();

entity.Add<int>(10, target1);
entity.Add<int>(20, target2);

int[] scores = entity.Get<int>(Entity.Any);
// scores = [10, 20]

// Get all linked GameObjects
GameObject[] objects = entity.Get<GameObject>(Link.Any);
```

## `Components` Property - Get Everything :neofox_magnify:

Returns all components on the entity as boxed values. Great for debugging and serialization!

```cs
IReadOnlyList<Component> components = entity.Components;

foreach (var component in components)
{
    Console.WriteLine($"{component.Type.Name}: {component.Value}");
}
```

::: info :neofox_think: About Boxing
The `Components` property boxes all values each time it's called. That's fine for debugging, serialization, or occasional use – just don't call it every frame in a hot loop!
:::

## Comparison Table

| Method | Returns | Use Case |
|--------|---------|----------|
| `Ref<C>()` | `ref C` | Read/write a single known component |
| `Ref<C>(match)` | `ref C` | Read/write with specific match |
| `Get<C>(match)` | `C[]` | Get all components matching expression |
| `Components` | `IReadOnlyList<Component>` | Inspect all components (boxed) |
| `Ensure<C>()` | `ref C` | Get or create component |

## Safe Patterns

### Check Before Access
```cs
if (entity.Has<Health>())
{
    ref var health = ref entity.Ref<Health>();
    health.Value -= damage;
}
```

### Use Ensure for Optional Components
```cs
// Don't check + add, just ensure!
ref var counter = ref entity.Ensure<HitCount>();
counter.Value++;
```

### Immediate Use Pattern
```cs
// ✅ Good - use reference immediately
entity.Ref<Position>().X += 10;

// ❌ Risky - storing reference
ref var pos = ref entity.Ref<Position>();
DoSomethingThatMightAddComponents(entity);  // Danger!
pos.X += 10;  // Might be dangling
```

## Constraints

- `C` must be `notnull`
- Component must exist (throws `KeyNotFoundException` otherwise)
- For `Link<L>`, `L` must be a reference type (`class`)
- References are invalidated by structural changes


