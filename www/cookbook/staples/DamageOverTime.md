---
title: Damage over Time
outline: [2, 3]
order: 3
---

# Turns out, Vampires aren't Survivors?!

In this recipe, we'll explore how to implement a simple "Damage over Time" (DoT) system using **fenn**ecs. We'll create entities that represent characters, some of which are "vampires" that take damage from a global `sunIntensity` value over time.

## Ingredients

First, let's define the components we'll need:

```csharp
// Represents the current health of a character
public struct Health
{
    public float Value;
}

// A tag component indicating that a character is a vampire
public struct Vampirism;
```

## Preparation

Now, let's create some entities representing characters, both vampires and non-vampires:

```csharp
var world = new World();

var human = world.Entity()
    .Add(new Health { Value = 100 })
    .Spawn(4); // Left 4 Undead?!

var vampires = world.Entity()
    .Add(new Health { Value = 100 })
    .Add<Vampirism>()
    .Spawn(100_000); // Not looking good for the humans!
```

## Applying the Damage

Now, let's create a query that applies damage to all vampires based on the sunlight intensity:

```csharp
var sunIntensity = 10.0f;
var vampireHealth = world.Query<Health>().Has<Vampirism>().Stream();

// We use an EntityAction to apply the damage and also queue the
// structural change - in this case, full despawn of the Vampire
vampireHealth.For(static (Entity vampire, ref Health health, float sunBurn) => 
{   
    health.Value -= sunBurn;
    if (health.Value <= 0) vampire.Despawn();
    // give it ~10 seconds and your humans will be safe    
}, uniform: Time.deltaTime * sunIntensity);
```

This query finds all entities that have both a `Vampire` and a `Health` component, and applies damage to their health based on the intensity of the sun.

The damage to be dealt is integrated (scaled with deltaTime) outside the tight loop, and passed in as a simple uniform float.


## Serving Suggestions

You can extend this example in various ways:

- Add a `Heal` component that restores health over time for non-vampire entities
- Introduce a `Shade` component that reduces the intensity of sunlight for entities that are under cover, or make the sunlight a vector and calculate the local intensity based on the position of the entity.

And there you have it - a simple but effective example of how to implement damage over time using fennecs! 

