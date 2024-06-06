---
title: Spawning
order: 0
---

# Spawning Entities

In **fenn**ecs, there are two primary ways to spawn entities: using the `World.Spawn()` method and using an `EntitySpawner` obtained via `World.Entity()`. Both approaches allow you to create new entities and add components to them, but they differ in their usage and flexibility.

## Quick & Easy Spawns

The `World.Spawn()` method is the simplest way to create a new entity in the world. It returns an `EntityBuilder` that allows you to add components to the entity before it is spawned.

```csharp
var world = new World();

// Spawn a single entity
var entity = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity { X = 1, Y = 1 });
```

In this example, we create a new entity using `World.Spawn()` and add two components (`Position` and `Velocity`) to it using the `Add` method. The entity is automatically spawned in the world after the components are added.

::: tip :neofox_think: PAWS FOR THOUGHT: How appropriate is it to do things one-by-one in an ECS?
The trivial spawning method is great for many scenarios (even beyond early development)!

However if you have particularly complex entities with dozens of components, or wish to assemble them procedurally, or spawn many, it will hit its limits!

Each `Add` call moves the Entity to another archetype, which leaves a trail of empty interstitial Archetypes in the World. Then for the next entity, the same is done again... and again... and again.
:::

## Fast, Flexible Spawns

The `EntitySpawner` is a more flexible way to spawn entities, especially when you need to create multiple entities with the same set of components. You can obtain an `EntitySpawner` by calling `World.Entity()`.

Add Components and Relations to taste, then spawn a number of Entities by calling `Spawn(int count)`.

All Entities will be spawned exactly in the Archetype that they belong, right from the start. Data is blitted directly to the Archetype's storages, so it's lightning-fast; and no empty Archetypes are left behind.

In this example, we create an `EntitySpawner` using `World.Entity()` and add the desired components to it. We then use the `Spawn` method of the `EntitySpawner` to create multiple entities (10 in this case) with the same set of components. Finally, we call `Dispose()` and are done.


### Typical Use
```csharp
var world = new World();

using var spawner = world.Entity() // Requests an EntitySpawner
    .Add(new Velocity { X = 50 })
    .Add<Bee>()
    .Spawn(100_000); // AAAAAAAAAA!
```

::: info :neofox_thumbsup: BONUS POINTS: Optional Cleanup with `Dispose()`
`EntitySpawner` uses some internal pooled data structures to further conserve memory and accelerate Entity setup. Spawners are lightweight and no memory would be leaked either way, but they implement `IDisposable` to return the data structures to their pools for faster reuse. 

It's up to you to hold on to them for as long as you like, and dispose of them when you're done.
:::


The `EntitySpawner` approach is particularly useful when you need to spawn a large number of entities with the same components, as it avoids the overhead of creating individual `EntityBuilder`s for each entity.


### Repeat Spawns
Here's another example that demonstrates the versatility of `EntitySpawner`:
```csharp

world.Entity()
    .Add(new Health { Value = 100 })
    .Add<Dexterity>(12) // Stats do well with conversion operators for int!
    .Add<Charisma>(15)
    //.Add<...> (more omitted here)
    .Add<Human>()
    .Spawn()    // Spawn a single human
    .Dispose(); // immediately dispose the spawner after we used it

// The using statement disposes the spawner at end of scope/function
using var werewolfSpawner = world.Entity()
    .Add(new Health { Value = 250 })
    .Add<Werewolf>()
    .Spawn(9); // 9 regulars

werewolfSpawner.Add<Elite>(); // anything it spawns from now on has Elite!
werewolfSpawner.Spawn(5); //+5 Elites, giving the BEST NUMBER of werewolves: 14!

return; // werewolfSpawner is automatically disposed here by the using statement.
```


## Advanced: Procedural Spawns
(Work in progress - coming soon!)

## Conclusion

Both `World.Spawn()` and `EntitySpawner` provide ways to create entities in **fenn**ecs. `World.Spawn()` is simpler and suitable for spawning individual entities, while `EntitySpawner` is more flexible and efficient for spawning multiple entities with the same set of components.

Choose the spawning approach that best fits your needs based on the number of entities you need to create and the desired component composition.
