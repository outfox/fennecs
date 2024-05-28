---
title: Damage over Time
outline: [2, 3]
---

# Turns out, Vampires aren't Survivors?!

In this recipe, we'll explore how to implement a simple "Damage over Time" (DoT) system using fennecs ECS. We'll create entities that represent characters, some of which are "vampires" that take damage from a "Sunlight" component over time.

## Ingredients

First, let's define the components we'll need:

```csharp
public struct Health
{
    public float Value;
}

public struct Vampire { }

public struct Sunlight
{
    public float Intensity;
}
```

- `Health`: Represents the current health of a character
- `Vampire`: A tag component indicating that a character is a vampire
- `Sunlight`: Represents the intensity of sunlight in the environment

## Preparation

Now, let's create some entities representing characters, both vampires and non-vampires:

```csharp
var world = new World();

var human = world.Spawn()
    .Add(new Health { Value = 100 });

var vampire1 = world.Spawn()
    .Add(new Health { Value = 100 })
    .Add<Vampire>();

var vampire2 = world.Spawn()
    .Add(new Health { Value = 100 })
    .Add<Vampire>();
```

We also need to create a `Sunlight` component and add it to the world:

```csharp
var sunlight = world.Spawn()
    .Add(new Sunlight { Intensity = 10 });
```

## Applying the Damage

Now, let's create a query that applies damage to all vampires based on the sunlight intensity:

```csharp
var damageQuery = world.Query<Health>().Has<Vampire>().Compile();
var sunlightQuery = world.Query<Sunlight>().Compile();

// In your game loop:
sunlightQuery.ForEach((ref Sunlight sunlight) =>
{
    damageQuery.ForEach((ref Health health) =>
    {
        health.Value -= sunlight.Intensity * Time.deltaTime;
    });
});
```

This query finds all entities that have both a `Vampire` and a `Health` component, and applies damage to their health based on the intensity of the `Sunlight` component.

## Serving Suggestions

You can extend this example in various ways:

- Add a `Heal` component that restores health over time for non-vampire entities
- Introduce a `Shade` component that reduces the intensity of sunlight for entities that are under cover
- Spawn and despawn `Sunlight` entities based on the time of day in your game world

And there you have it - a simple but effective example of how to implement damage over time using fennecs ECS! :neofox_knife:
```

This example demonstrates how to use components to represent character attributes and environmental factors, and how to use queries to apply changes to entities based on those components.

The code samples show how to define the necessary components, create entities with those components, and use a query to apply damage to vampire entities based on the sunlight intensity.

Let me know if you have any further questions or if you'd like me to elaborate on any part of the example!