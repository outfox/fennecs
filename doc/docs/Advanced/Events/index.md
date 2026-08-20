---
title: Events
order: 33
outline: [1, 2]
description: 'Component Signals in fennecs - subscribe to a component type with World.On<T>() and react when Entities gain or lose it.'
---

# Component Signals (a.k.a. Events)

## What's a Signal?

A ==Signal== is fennecs' observer surface for ==structural changes==. You subscribe to a component type, and fennecs tells you whenever an Entity **gains** or **loses** it.

```csharp
world.On<Position>().Added   += (entity, ref position) => spatialIndex.Insert(entity, position);
world.On<Position>().Removed += (entity, in  position, cause) => spatialIndex.Remove(entity);
```

That's the whole idea. `World.On<T>()` hands you the `Signal<T>` for that component type; `Added` and `Removed` are plain C# events, so `+=` and `-=` work exactly as you'd expect.

Handlers get an [`EntityRef`](/docs/Basic/Entities/EntityRef.md)  –  the same live, already-located handle the Stream runners hand out. Its accessors read the Archetype storage directly, skipping the liveness check, so poking at the Entity's *other* components inside a handler is cheap.

::: info :neofox_magnify: STRUCTURAL, NOT VALUE
Signals fire when a component **appears** or **disappears** on an Entity. They do *not* fire when you change a value  –  a `Stream.For` runner that writes through a `ref` is invisible to them, and gloriously so: that's the hot loop, and it stays free of any bookkeeping.
:::

Adding and removing the same Component during a World Lock triggers both Signals  –  even though, once the catch-up is done, the Entity ends up without the Component.

## Quick Start

```csharp
using var world = new World();

var spawned = 0;
var buried = 0;

world.On<Health>().Added   += (_, ref _) => spawned++;
world.On<Health>().Removed += (_, in _, _) => buried++;

var fox = world.Spawn().Add(new Health(100));  // spawned == 1
fox.Remove<Health>();                          // buried == 1
```

Signals are *memoized per expression*  –  calling `world.On<Health>()` twice returns the very same `Signal<Health>`. Reach for `On<T>()` every time you subscribe; there is no reason to hold on to what it gives you.

::: danger :neofox_dizzy: DON'T STORE THE SIGNAL, CALL `On<T>()`
Garbage collections of the World clean up empty Signal. 
A empty `Signal<T>` you kept in a field survives that drop as an **orphan**: 
the World has forgotten it, `On<T>()` will hand out a fresh instance to everyone else, and your `+=` on the stale one is accepted in silence and **never fires again**.

`On<T>()` is a dictionary probe over a handful of entries. Subscribing is not a hot path; pay the lookup, keep the guarantee.
:::

## When exactly do they fire?

This is the important part, and it's simple:

| Event | Fires | Why there |
|-------|-------|-----------|
| `Added` | **after** the value is stored | the component exists, so you get a live reference to it |
| `Removed` | **just before** the value is discarded | it's the last moment the outgoing value can still be read |

Because `Removed` runs just *before* the structural change, the Entity still has the Component, which is what lets you access it.

```csharp
world.On<Health>().Removed += (entity, in health, cause) =>
{
    Console.WriteLine($"{entity} died with {health.Value} HP left"); // the value is still there
    Console.WriteLine(entity.Has<Health>());                         // True
};
```

::: tip :neofox_thumbsup: NO PHANTOM SIGNALS
A Batch only signals what actually *changes*. Re-writing a component that Entities already have (via `AddConflict.Replace`) is a value change, not an addition  –  so it stays silent. Likewise, a tolerated no-op removal (`RemoveConflict.Allow` on an Entity that never had the component) raises nothing.
:::

## Removed... but why?

A Component can go away for three quite different reasons, and your handler usually cares which. That's the third parameter, `RemoveCause`:

| Cause | What happened | Does the Entity survive? |
|-------|---------------|--------------------------|
| `Removed` | the Component was removed on its own  –  `Remove`, a Wildcard removal, a Batch | **yes** |
| `Despawned` | the Entity itself is going away, and takes everything with it | **no** |
| `TargetDespawned` | a Relation or Link lost its *target*, so it gets cleaned up | **yes**  –  only the relation dies |

The distinction matters the moment your handler wants to *do* something with the Entity:

```csharp
world.On<Sprite>().Removed += (entity, in sprite, cause) =>
{
    renderer.Unregister(sprite);                  // always - the sprite is going away
    if (cause == RemoveCause.Despawned) return;   // no point dressing up a dying Entity
    entity.Add(new NeedsRepaint());
};
```

::: warning :neofox_angel_pleading: DON'T REMOVE WHAT'S ALREADY LEAVING
Inside `Removed` the Component is still stored, so `Has<T>()` answers `true`, but a removal is already under way. Calling `Remove<T>()` for *that* Component queues a second removal, which finds nothing once it is applied.

The same logic applies to a despawning Entity: it will be gone by the time the catch-up occurs. That means if the cause is `Despawned`, do not `Add` or `Remove`.

Either way, the failure surfaces as a [`SignalException`](#when-a-handler-throws)  –  so it can never be mistaken for a mistake in the call you actually wrote.
:::

## Wildcards, Relations & Links

`On<T>()` on its own watches *plain* components  –  a relation `Health` targeting some other Entity won't trip it. Pass a [Match expression](/docs/Advanced/Expressions/index.md) to widen or narrow the net:

```csharp
world.On<Health>()             // plain Health only
world.On<Health>(Match.Any)    // plain + every relation + every Object Link
world.On<Health>(Match.Entity) // only Entity-Entity relations
world.On<Health>(nemesis)      // only the relation targeting this one Entity
```

A Wildcard Signal fires once **per concrete expression**, and your handler receives that expression's own value:

```csharp
var entity = world.Spawn().Add(new Health(1), alpha).Add(new Health(2), beta);

world.On<Health>(Match.Any).Removed += (_, in health, _) => Console.WriteLine(health.Value);

entity.Remove<Health>(Match.Any);  // prints 1 and 2 - two Signals, one structural change
```

## Handlers may change the World

Dispatch happens while a ==World Lock== is held  –  the very same [Manual World Locking](/docs/Basic/Entities/Liveness.md#manual-world-locking) you can take out yourself  –  so anything structural your handler does is queued and applied once the whole operation completes:

```csharp
// every Entity that gains a Position gains a Health, and loses it again with the Position
world.On<Position>().Added   += (entity, ref _) => entity.Add(new Health(100));
world.On<Position>().Removed += (entity, in  _, _) => entity.Remove<Health>();
```

## When a handler throws

A buggy handler must never look like a bug in the call that triggered it. So anything escaping a Signal is wrapped in a `SignalException`, carrying your original exception as its `InnerException`:

```csharp
world.On<Health>().Added += (_, ref _) => throw new InvalidOperationException("boom");

try
{
    entity.Add(new Health(1));   // this Add is perfectly valid
}
catch (SignalException e)
{
    Console.WriteLine(e.Signal);    // Signal<Health>(Plain) - who misbehaved
    Console.WriteLine(e.Entity);    // which Entity was being signalled
    Console.WriteLine(e.Deferred);  // False - the handler threw while running
    throw e.InnerException!;        // ...boom
}
```

Handlers can fail at two very different moments, and `Deferred` tells them apart: either the handler itself threw, or it queued an invalid Structural Change that only failed later, when the World caught up (the `Deferred` case).

```csharp
world.On<Health>().Removed += (entity, in _, _) => entity.Remove<Health>();  // already leaving!

entity.Remove<Health>();
// SignalException { Deferred = true, InnerException = InvalidOperationException }
// ...and NOT an error attributed to the Remove call above
```

::: tip :neofox_thumbsup: THE POINT OF ALL THIS
Catch `SignalException` and you are looking at an observer's fault. Anything else that comes out of `Add` / `Remove` / `Despawn` / `Submit` is a fault in the call you actually wrote.
:::

::: info :neofox_think: FIRST FAILURE WINS
Dispatch stops at the first throwing handler  –  the remaining subscribers, and the remaining Entities of a bulk operation, do not run. There is no aggregation, and a `SignalException` is never wrapped inside another one.
:::

## What does it cost?

**You pay for what you subscribe to, and nothing else.** A Signal with no handlers costs a single field compare; a Signal on a Component type your loop doesn't touch costs one dictionary probe. Neither takes the World Lock, and neither allocates.

Measured over 10 000 Entities (`SignalBenchmarks`, .NET 10), against the same operation with no Signals at all:

| Path | Signal elsewhere / unsubscribed | 1 handler | 4 handlers |
|------|--------------------------------:|----------:|-----------:|
| `Add` + `Remove`, per Entity | *baseline* | +14 % | +20 % |
| `Add` + `Remove`, via Batch | *baseline* | ×6.6 | ×10.3 |
| Spawn + Despawn wave | *baseline* | ×2.1 | ×2.6 |

The Batch row is the honest worst case, and it is worth understanding rather than fearing: a Batch performs **two** Archetype migrations for ten thousand Entities, but still owes you **ten thousand Signals**. There is no bulk work left for the dispatch to hide behind. In absolute terms it is still about 7 ns per Signal.

Per-Entity CRUD barely notices, because the Archetype move dwarfs the dispatch. Dispatch allocates nothing; only a Batch does, capturing the Entity handles it must signal after the migration.

To clear out unused Signals, call `World.GC`: it drops every Signal left without subscribers. This is also precisely why a stored `Signal<T>` goes stale  –  always subscribe through [`On<T>()`](#quick-start).

::: info :neofox_think: MAKE SURE YOU CAN UNSUBSCRIBE
Use named methods, so you can unsubscribe  –  `world.On<T>().Added -= Handler`. Lambdas are great for prototyping, but hard to clean up afterwards.
Signal registration is not thread-safe: do not subscribe from another thread.
:::
