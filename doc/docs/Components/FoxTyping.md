---
title: Fox Typing
order: 15
outline: [1, 2]
---

# Fox Typing :neofox_hug_duck_heart:

::: tip :neofox_thumbsup: Designing Great Components
How do we make components as useful as possible while keeping them simple? This guide covers best practices for component design in **fenn**ecs!
:::

## The Challenge

When building an ECS, you want components that are:
- **Type-safe** – Easy to identify and impossible to mix up
- **Readable** – Self-documenting in delegate code
- **Distinct** – No clashes between similar data (e.g., multiple `Vector3` uses)
- **Efficient** – Fast to store and process

## Record Structs: The Sweet Spot

C# 10's `record struct` is perfect for **fenn**ecs components:

```cs
public record struct Position(Vector3 Value);
public record struct Velocity(Vector3 Value);
public record struct Acceleration(Vector3 Value);
```

### Why Record Structs?

| Feature | Benefit |
|---------|---------|
| **Value semantics** | Predictable equality comparison |
| **Minimal boilerplate** | One line per component |
| **Nice `ToString()`** | Great for debugging |
| **Tightly packed** | Excellent cache performance |
| **`with` expressions** | Easy partial updates |

### Using `with` Expressions

Records support partial reinstantiation:

```cs
public record struct Transform(Vector3 Position, Quaternion Rotation);

var tr = new Transform(Vector3.Zero, Quaternion.Identity);

// Create new instances with some fields changed
var tr2 = tr with { Position = new Vector3(1, 2, 3) };
var tr3 = tr with { Rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f) };
```

## Type Wrapping: Avoiding Confusion :neofox_think:

Without proper typing, you might end up with ambiguous components:

```cs
// ❌ Confusing: What do these floats represent?
entity.Add<float>(10.0f);  // Is this speed? health? what?
entity.Add<float>(100.0f); // Can't even have two floats!
```

With fox typing:

```cs
// ✅ Clear: Each type is self-documenting
public record struct Speed(float Value);
public record struct Health(float Value);

entity.Add(new Speed(10.0f));
entity.Add(new Health(100.0f));
```

## Practical Examples

### Game Stats

```cs
public record struct Health(int Current, int Max);
public record struct Mana(int Current, int Max);
public record struct Stamina(float Value);
public record struct Experience(int Value);
public record struct Level(int Value);
```

### Physics

```cs
public record struct Position(Vector3 Value);
public record struct Velocity(Vector3 Value);
public record struct Acceleration(Vector3 Value);
public record struct Mass(float Kg);
public record struct Radius(float Value);
```

### Implicit Conversions

For ergonomic use, add conversion operators:

```cs
public record struct Health(int Value)
{
    public static implicit operator Health(int value) => new(value);
    public static implicit operator int(Health health) => health.Value;
}

// Now you can do:
entity.Add<Health>(100);  // Implicit conversion from int
int hp = entity.Ref<Health>(); // Implicit conversion to int
```

## Quick Reference

| Pattern | Example | Use Case |
|---------|---------|----------|
| Simple wrapper | `record struct Speed(float Value)` | Single value |
| Multi-field | `record struct Health(int Current, int Max)` | Related values |
| With operators | Add `implicit operator` | Ergonomic API |
| Plain struct | `struct Enemy;` | Zero-size tags |

## Best Practices :neofox_comfy:

1. **One purpose per type** – Don't reuse types for different meanings
2. **Descriptive names** – `Health`, not `HP` or `H`
3. **Use record structs** – Unless you need sharing or special behavior
4. **Consider operators** – For frequently used numeric types
5. **Group related data** – Better to have `Transform(Position, Rotation)` than separate components if they're always used together

::: info :neofox_science: Future SIMD Support
Future versions of **fenn**ecs may introduce `Fox128<T>` and `Fox256<T>` interfaces for SIMD-optimized operations on 128-bit and 256-bit aligned types.
:::