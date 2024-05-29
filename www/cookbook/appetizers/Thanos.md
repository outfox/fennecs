---
title: 6. Thanos (Filters)
outline: [2, 3]
order: 6
---

# How to Snap (only) half the World away

Hey there, mighty Titan! Ready to bring perfect balance to your world? In this example, we'll show you how to use fennecs' `Query.Subset` and `Query.Exclude` methods to randomly remove half the entities in your world, just like Thanos did with a snap of his fingers. We'll create a bunch of entities, give some of them a lucky "Survive" component, and then unleash the power of the Infinity Gauntlet to remove the unlucky ones. Let's get snapping!



```csharp
using fennecs;

// Component tags
struct Alive;
struct Survive;


// The universe is vast, but finite. Its resources, finite.
// (fennecs 1.x can handle around 2^31 entities, so it's not that finite)
var world = new World();

// Life. Unchecked, it will cease to exist. It needs correcting.
var entities = world.Entity()
    .Add<Alive>()
    .Spawn(1_000_000);

// The hardest choices require the strongest wills. 
var random = new Random();
entities.For((Entity entity, ref Alive _) =>
{
    if (random.NextDouble() < 0.5)
    {
        entity.Add<Survive>();
    }
});

// I'm the only one who knows that. At least I'm the only who has the will to act on it.
var thanosQuery = world.Query<Alive>().Compile();  
thanosQuery.Subset<Alive>(Match.Plain); // Perfectly balanced.
thanosQuery.Exclude<Survive>(); // As all things should be.

// I could simply snap my fingers, and they would all cease to exist.
thanosQuery.Despawn();

// I call that... mercy.
int remainingCount = world.Query<Alive>().Compile().Count;
Console.WriteLine($"Entities remaining after Thanos snap: {remainingCount}");
```

In this example:

1. We define two component types: `Alive` and `Survive`.
2. We create a new `World` instance and spawn a large number of entities, all with the `Alive` component.
3. Using a query to iterate over entities with `Alive`, we randomly add the `Survive` component to half of them.
4. We create a new query (`thanosQuery`) that matches entities with `Alive` but without `Survive`. The `Subset` call ensures that all matched entities have `Alive`, while `Exclude` removes entities that have `Survive`.
5. We call `Despawn()` on the `thanosQuery` to remove all entities that match the query, effectively eliminating the entities without the `Survive` component.
6. Finally, we count the remaining entities with `Alive` to verify that approximately half of the original entities are left.

This example demonstrates how `Query.Subset` and `Query.Exclude` can be used together to create a specific subset of entities based on the presence or absence of certain components. The `Survive` tag component acts as a filter to determine which entities are spared from the "Thanos snap".

Let me know if this aligns with your vision for the `Thanos.md` example, or if you have any other specific examples you'd like me to help sketch out. I'm happy to collaborate on fleshing out the documentation further!