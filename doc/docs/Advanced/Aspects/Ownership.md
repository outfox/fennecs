---
title: Ownership
order: 1
outline: [1, 2]

head:
  - - meta
    - name: description
      content: Declaring component ownership with Aspect.Owns, strict mode, and the freeze-at-first-use rule.
---

# Ownership & Registration

*finders keepers, foxes weepers*

Each component type is stored by exactly **one** Aspect per World. You declare who owns what with `Owns`  –  everything you don't declare belongs to `Main`.

## Declaring Ownership

`Owns` is fluent and comes in a generic, a `params Type[]`, and an all-in-one flavor:

::: code-group

```csharp [🥇 fluent]
var visuals = world.AddAspect("visuals")
    .Owns<Position>()
    .Owns<Velocity>();
```

```csharp [🆗 params Type[]]
var visuals = world.AddAspect("visuals")
    .Owns(typeof(Position), typeof(Velocity));
```

```csharp [🥡 all-in-one]
var visuals = world.AddAspect("visuals", typeof(Position), typeof(Velocity));
```

:::

Re-registering a type to the *same* Aspect is a harmless no-op  –  `Owns` just smiles and returns the Aspect for further chaining.

## Routing Rules

- A component type has **one** owning Aspect per World.
- Types nobody `Owns` are stored in `Main` (unless [strict mode](#strictaspects) says otherwise).
- Registering a type that *another* Aspect already owns throws an `InvalidOperationException` that politely names the current owner. No custody battles.

```csharp
world.AddAspect("visuals").Owns<Position>();
var game = world.AddAspect("game");

game.Owns<Position>(); // 💥 InvalidOperationException:
// Component type Position is already owned by Aspect "visuals"
// and cannot be re-registered to "game".
```

## Ownership Freezes at First Use

::: danger :neofox_scream: THE FREEZE RULE
The moment a component type materializes in *any* Archetype  –  usually because some Entity innocently `Add`ed it  –  its ownership is **frozen** for the lifetime of the World. Registering it afterwards throws:

```csharp
using var world = new World();
var visuals = world.AddAspect("visuals");

world.Spawn().Add(new Position(1, 2)); // Position routes to Main... and freezes there

visuals.Owns<Position>(); // 💥 InvalidOperationException:
// Component type Position is already in use by Aspect "main" - ownership
// freezes at first use. Register it via Owns<T>() before the first Entity
// or Archetype uses the type.
```
:::

::: tip :neofox_thumbsup: THE ANTIDOTE
Register all your Aspects in your World subclass's constructor (as shown in the [Quick Start](index.md#quick-start)). Nothing can materialize a type before the constructor finishes, so nothing can freeze you out.
:::

## StrictAspects

For those who like their storage layout *load-bearing*, `StrictAspects` turns implicit routing off entirely. Every component type must be registered to an Aspect before use  –  unregistered types throw instead of quietly moving in with `Main`.

```csharp
using var world = new World { StrictAspects = true };
world.AddAspect("game").Owns<CrewData>();

var entity = world.Spawn();     // fine - Identity is always exempt
entity.Add(new CrewData(5));    // fine - registered

entity.Add(new Position(1, 2)); // 💥 InvalidOperationException:
// World requires Aspect ownership for all Component types
// (StrictAspects = true), but Position is not owned by any Aspect.
```

::: tip :neofox_glasses: SUPERNERD PRO TIP
Strict mode is a great safety net for larger projects: a typo'd or forgotten registration fails *loudly at the first `Add`*, instead of silently fragmenting `Main` and showing up months later in a profiler trace at 2am.
:::

## The Identity Exception

The built-in `Identity` component lives in **every** Aspect (it's how each Aspect tracks its members), so no single Aspect may own it  –  `Owns<Identity>()` throws. It's also why plain `Spawn()` and `Has<Identity>()` work even in strict mode: Identity is always exempt from registration.

::: details :neofox_magnify: BEHIND THE SCENES  –  a fox with two passports
`Identity` is special enough to carry **two** TypeIDs: a *reserved* one used in expressions, and a *registry-assigned* one from the global type registry. That's why ownership registration guards against it by `Type` rather than by TypeID  –  and why every spot that resolves a Query to its Aspect deliberately skips Identity, so it never drags a Query toward any particular Aspect.
:::

## Where to next?

Ownership settled? Learn how Entities actually [join and leave Aspects](Membership.md).
