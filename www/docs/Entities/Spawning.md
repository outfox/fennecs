---
title: Spawning
order: 0
---

# Spawning Entities

In **fenn**ecs, there are two primary ways to spawn entities: using the `World.Spawn()` method and using an `EntitySpawner` obtained via `World.Entity()`. Both approaches allow you to create new entities and add components to them, but they differ in their usage and flexibility.

## Spawning with `World.Spawn()`

The `World.Spawn()` method is the simplest way to create a new entity in the world. It returns an `EntityBuilder` that allows you to add components to the entity before it is spawned.

```csharp
var world = new World();

// Spawn a single entity
var entity = world.Spawn()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity { X = 1, Y = 1 });
```

In this example, we create a new entity using `World.Spawn()` and add two components (`Position` and `Velocity`) to it using the `Add` method. The entity is automatically spawned in the world after the components are added.

## Spawning with `EntitySpawner`

The `EntitySpawner` is a more flexible way to spawn entities, especially when you need to create multiple entities with the same set of components. You can obtain an `EntitySpawner` by calling `World.Entity()`.

```csharp
var world = new World();

// Create an EntitySpawner
var spawner = world.Entity()
    .Add(new Position { X = 0, Y = 0 })
    .Add(new Velocity { X = 1, Y = 1 });

// Spawn multiple entities using the EntitySpawner
var entities = spawner.Spawn(10);
```

In this example, we create an `EntitySpawner` using `World.Entity()` and add the desired components to it. We then use the `Spawn` method of the `EntitySpawner` to create multiple entities (10 in this case) with the same set of components.

The `EntitySpawner` approach is particularly useful when you need to spawn a large number of entities with the same components, as it avoids the overhead of creating individual `EntityBuilder`s for each entity.

Here's another example that demonstrates the power of `EntitySpawner`:

```csharp
var human = world.Entity()
    .Add(new Health { Value = 100 })
    .Spawn(4); // Left 4 Undead?!

var vampires = world.Entity()
    .Add(new Health { Value = 100 })
    .Add<Vampire>()
    .Spawn(100_000); // Not looking good for the humans!
```

In this example, we create two `EntitySpawner`s: one for humans and one for vampires. We add the `Health` component to both spawners and an additional `Vampire` component to the vampire spawner. We then use the `Spawn` method to create 4 human entities and 100,000 vampire entities in a concise and efficient manner.

## Conclusion

Both `World.Spawn()` and `EntitySpawner` provide ways to create entities in **fenn**ecs. `World.Spawn()` is simpler and suitable for spawning individual entities, while `EntitySpawner` is more flexible and efficient for spawning multiple entities with the same set of components.

Choose the spawning approach that best fits your needs based on the number of entities you need to create and the desired component composition.
