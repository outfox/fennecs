---
title: Aspects
order: 1
outline: [1, 2]
description: 'Aspects split a fennecs World into contiguous component storage universes - declare them with AddAspect and Owns to group hot data and fight fragmentation.'
---

# Aspects

*Same World... Different Neighborhoods!*

## What's an Aspect?

An ==Aspect== is a self-contained collection of [Archetypes](/docs/Components/index.md#archetypes) inside a World  –  its very own, contiguously laid-out component storage universe. All Aspects of a World share the same Entities; only the *component data* lives apart.

Every World comes with one Aspect built in: `World.Main`, fittingly named `"main"`. It's always first in `world.Aspects`, proudly reports `IsMain == true`, and every living Entity is a member. Any component type you don't explicitly assign elsewhere is stored there  –  which is why, until this very page, you never needed to know Aspects existed. *You're welcome.*

Adding more Aspects lets you *group hot data*  –  say, the `Position` and `Matrix` components your renderer touches every frame  –  into their own storage universe, where the comings and goings of gameplay components can't fragment them.

::: info :neofox_magnify: WHY THOUGH?
==Fragmentation== is the archetype-based ECS's natural predator: every distinct component combination creates its own Archetype, and many small Archetypes mean many small memory hops. An Aspect that owns just your hot types collapses that combinatorial explosion  –  Entities that differ wildly in *gameplay* components can still share one big, happy, cache-friendly Archetype in the `"visuals"` Aspect.
:::

## Quick Start

Declare an Aspect with `AddAspect`, tell it which component types it `Owns`, and... that's it. Components route themselves to their owning Aspect automatically.

::: code-group

```csharp [🥇 recommended: subclass World]
private class GameWorld : World
{
    public readonly Aspect Visuals;
    public readonly Aspect Game;

    public GameWorld()
    {
        // register everything up front, before any Entity materializes a type
        Visuals = AddAspect("visuals").Owns<Position>().Owns<Velocity>();
        Game = AddAspect("game").Owns<CrewData>();
    }
}

using var world = new GameWorld();

var entity = world.Spawn()
    .Add(new Position(1, 2))  // stored in the "visuals" Aspect
    .Add(new CrewData(3));    // stored in the "game" Aspect

// one Entity, one identity - data in two Aspects
Console.WriteLine(world.Visuals.Count); // 1
Console.WriteLine(world.Game.Count);    // 1
Console.WriteLine(entity.Has<Position>() && entity.Has<CrewData>()); // True
```

```csharp [🆗 inline]
using var world = new World();

var visuals = world.AddAspect("visuals").Owns<Position>().Owns<Velocity>();
var game = world.AddAspect("game", typeof(CrewData)); // params Type[] shorthand

world.Spawn().Add(new Position(1, 2)).Add(new CrewData(3));
world.Spawn().Add(new Position(3, 4));

// stream just the "visuals" universe - contiguous and cozy
visuals.Stream<Position>().For((ref position) => position.X += 1f);
```

:::

## When to reach for Aspects

When profiling tells you ==Fragmentation== is eating your frame time  –  usually once entity counts climb into the hundreds of thousands and your hot loops hop between many small Archetypes.

::: tip :neofox_sip: DON'T PREMATURELY FOXIMIZE
One `Main` Aspect is plenty for most games, and **fenn**ecs is already fast as a fox without any of this. Profile first (your World's `DebugString()` shows Archetype counts and sizes), *then* carve out an Aspect for the data that actually runs hot.
:::

## The Main Aspect & IAspect

Both `World` and `Aspect` implement the same interface: `IAspect`. A World simply delegates its entire query surface to its `Main` Aspect  –  so all your existing code keeps working, unchanged, blissfully unaware.

- `world.Query<T>()` automatically resolves to the Aspect that owns `T`  –  you rarely need to name an Aspect to query it.
- `aspect.All` is the universal Query matching all of an Aspect's members; `world.All` *is* `world.Main.All` (the same object).
- Code that takes an `IAspect` parameter happily accepts either a World or an Aspect.

## Cheat Sheet

| Member | What it does |
|--------|--------------|
| `World.Main` | the built-in Aspect `"main"`; every living Entity is a member |
| `World.Aspects` | all Aspects of the World (`Main` is always first) |
| `World.AddAspect(name)` | adds a new Aspect (unique name, or `ArgumentException`) |
| `World.AddAspect(name, params Type[])` | add + register owned types in one call |
| `World.StrictAspects` | init-only; unregistered types throw instead of routing to `Main` → [Ownership](Ownership.md) |
| `Aspect.Owns<T>()` / `.Owns(params Type[])` | fluently declares the types this Aspect stores → [Ownership](Ownership.md) |
| `Aspect.Name` / `.World` / `.IsMain` | what it says on the tin |
| `Aspect.Count` | number of member Entities → [Membership](Membership.md) |
| `foreach (var entity in aspect)` | Aspects are `IEnumerable<Entity>` over their members |
| `Aspect.Query<C1...C5>()` / `.Stream<...>()` / `.All` | the full query surface, per Aspect → [Queries & Streams](Queries.md) |

## Where to next?

1. [Ownership](Ownership.md)  –  who stores what, `StrictAspects`, and the freeze rule
2. [Membership](Membership.md)  –  how Entities join, leave, and live across Aspects
3. [Queries & Streams](Queries.md)  –  the single-Aspect rule and how to work with it
