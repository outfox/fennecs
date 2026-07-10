---
title: Queries & Streams
order: 3
outline: [1, 2]

head:
  - - meta
    - name: description
      content: Querying and streaming fennecs Aspects - the single-Aspect rule, crossing Aspects with Ref, and the shared query cache.
---

# Queries & Streams

*one Aspect per Query  –  no double-dipping!*

Both `World` and `Aspect` implement `IAspect`, which carries the entire familiar query surface: the `Query()` / `Query<C1...C5>()` builders, `Stream<...>()`, and the universal `All` Query.

## Querying an Aspect

You usually don't even have to say which Aspect you mean  –  `world.Query<T>()` resolves to the Aspect that owns `T` all by itself:

::: code-group

```csharp [🥇 via the World (auto-resolve)]
// Position is owned by "visuals" - this Query targets "visuals" automatically
var positions = world.Query<Position>().Compile();

var crewSum = 0;
world.Stream<CrewData>().For((ref crew) => crewSum += crew.Count);
```

```csharp [🆗 via the Aspect (explicit)]
var positions = visuals.Query<Position>().Compile();

var xSum = 0f;
visuals.Stream<Position>().For((ref position) => xSum += position.X);
```

:::

Both routes are equivalent  –  pick whichever reads better. *(They're literally the same Query  –  see [the shared cache](#shared-query-cache).)*

## The Single-Aspect Rule

Here's the one commandment: **a Query can only match component types stored in a single Aspect.** Ask for types from two different storage universes and **fenn**ecs will refuse, with an itemized receipt:

::: danger :neofox_sign_no: THOU SHALT NOT SPAN ASPECTS
```csharp
world.Query<Position, CrewData>().Compile(); // 💥 InvalidOperationException:

// A Query can only match Component types stored in a single Aspect,
// but this Query's types span several:
//   Position -> Aspect "visuals"
//   CrewData -> Aspect "game"
// Group hot data into the same Aspect, or access other Aspects'
// components via entity.Ref<T>() inside the loop.
```
:::

Why? An Aspect is its own memory universe with its own Archetypes  –  there simply *is* no contiguous storage that holds both types side by side. That contiguity is the entire point of Aspects, so the rule is less a limitation and more the feature's job description.

## Filters count too

`Has`, `Not`, and `Any` filters are part of the Query's type mask, so they play by the same rule:

```csharp
world.Query<Position>().Has<CrewData>().Compile(); // 💥 spans "visuals" + "game"

// filtering WITHIN one Aspect is business as usual:
var loaded = world.Query<CrewData>().Has<Cargo>().Compile();  // ✅ all "game"
var empty = game.Query<CrewData>().Not<Cargo>().Compile();    // ✅ still "game"
```

[Relations](/docs/Advanced/Keys/Relation.md) work per-Aspect as well  –  including [Match Expressions](/docs/Queries/Matching.md) like `Match.Entity`  –  and may target Entities that never joined the Aspect at all.

## Crossing Aspects inside a loop

Need data from another Aspect *while* iterating? The sanctioned move is `entity.Ref<T>()`  –  Entity CRUD sees across all Aspects:

```csharp
// hot loop over contiguous "visuals" data...
visuals.Stream<Position>().For((in entity, ref position) =>
{
    // ...with an occasional peek into the "game" universe
    if (!entity.Has<CrewData>()) return;
    position.Y += entity.Ref<CrewData>().Count * 0.1f;
});
```

This is a random-access read, so keep it to the *cold* side of your data. If you find yourself `Ref`-ing a type in every iteration of every frame... congratulations, you've discovered hot data! Move it into the same Aspect.

## Shared Query Cache

A World Query and its Aspect-side twin aren't just equivalent  –  they're the *same object*, served from the same per-Aspect cache:

```csharp
var viaWorld = world.Query<Position>().Compile();
var viaAspect = visuals.Query<Position>().Compile();
// ReferenceEquals(viaWorld, viaAspect) == true
```

::: details :neofox_magnify: BEHIND THE SCENES
Each Aspect keeps its own Query cache and notifies its cached Queries when new Archetypes appear  –  compile a Query first and spawn matching Entities later, and the Query picks them up just fine. And since a World delegates everything to `Main`, `world.All` *is* `world.Main.All`. The universal `aspect.All` of every other Aspect matches that Aspect's members only.
:::

## Design Tips

- **Group components that are queried together into the same Aspect.** The single-Aspect rule is your layout linter: if a Query wants types from two Aspects, either those types belong together, or the Query wants too much.
- **Start with `Main`, carve out later**  –  but remember [ownership freezes at first use](Ownership.md#ownership-freezes-at-first-use), so do the carving in your World's constructor.
- **Keep cross-Aspect reads occasional.** `entity.Ref<T>()` is the pressure valve, not the plumbing.

## Where to next?

That's the whole tour! Head back to the [Aspects overview](index.md), or check what else lurks in [Advanced](/docs/Advanced/index.md).
