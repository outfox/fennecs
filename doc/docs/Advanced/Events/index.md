---
title: Events
order: 33
outline: [1, 2]
description: 'Component Signals in fennecs - subscribe to a component type with World.On<T>() and react when Entities gain or lose it.'
---

# Component Signals (a.k.a. Events)

## What's a Signal?

A ==Signal== is fennecs' observer surface for ==structural changes==. You subscribe to a component type, and fennecs tells you whenever Entities **gain** or **lose** it.

```csharp
using var added   = world.SubscribeAdded<Position>((entities, positions) => spatialIndex.Insert(entities, positions));
using var removed = world.SubscribeRemoved<Position>((entities, positions, cause) => spatialIndex.Remove(entities));
```

That's the whole idea. Each subscription hands you back a `Subscription`; dispose it, and the handler is gone.

Handlers are called **once per structural operation**, with two index-aligned spans: the Entities that changed, and their component values (`values[i]` belongs to `entities[i]`). A single `Add` is simply a span of length one; a Batch or a Template spawn is one call for a whole Archetype's worth of Entities.

::: info :neofox_magnify: STRUCTURAL, NOT VALUE
Signals fire when a component **appears** or **disappears** on an Entity. They do *not* fire when you change a value  –  a `Stream.For` runner that writes through a `ref` is invisible to them, and gloriously so: that's the hot loop, and it stays free of any bookkeeping.
:::

Adding and removing the same Component during a World Lock triggers both Signals  –  even though, once the catch-up is done, the Entity ends up without the Component.

## Quick Start

```csharp
using var world = new World();

var spawned = 0;
var buried = 0;

using var onAdded   = world.SubscribeAdded<Health>((entities, _) => spawned += entities.Length);
using var onRemoved = world.SubscribeRemoved<Health>((entities, _, _) => buried += entities.Length);

var fox = world.Spawn().Add(new Health(100));  // spawned == 1
fox.Remove<Health>();                          // buried == 1
```

## Subscribing and unsubscribing

There are two ways in, and they reach the very same `Signal<T>`:

```csharp
// 1. Disposable: owns the handler for you, ideal for lambdas.
using var subscription = world.SubscribeAdded<Health>((entities, healths) => { /* ... */ });

// 2. Plain C# events on the Signal itself: += and -= work exactly as you'd expect.
world.On<Health>().AfterAdded += OnHealthAdded;
world.On<Health>().AfterAdded -= OnHealthAdded;
```

::: tip :neofox_thumbsup: PREFER THE SUBSCRIPTION FOR LAMBDAS
`-=` needs the very same delegate you subscribed  –  a lambda you didn't keep can never be removed again. A `Subscription` keeps it for you: `Dispose()` it (or let `using` do it) and the handler is gone. Disposing twice, or after the World itself was disposed, does nothing.
:::

Signals are *memoized per expression*  –  calling `world.On<Health>()` twice returns the very same `Signal<Health>`.

::: tip :neofox_think: STORING THE SIGNAL IS SAFE
`World.GC()` drops Signals nobody subscribes to from the World's registry, but only *weakly*.
A `Signal<T>` you kept in a field is handed out again by `On<T>()`, and subscribing to it re-registers it, so it never ends up deaf.
Signals nobody holds are simply collected by the runtime.
:::

Signal registration is not thread-safe: do not subscribe from another thread.

## When exactly do they fire?

This is the important part, and it's simple:

| Event             | Fires                                  | Why there                                                    |
|-------------------|----------------------------------------|--------------------------------------------------------------|
| `AfterAdded`      | **after** the values are stored        | the components exist, so you get a writable slice of storage |
| `BeforeRemoved`   | **just before** the values are discarded | it's the last moment the outgoing values can still be read |

Because `BeforeRemoved` runs just *before* the structural change, the Entities still have the Component, which is what lets you access it.

```csharp
world.SubscribeRemoved<Health>((entities, healths, cause) =>
{
    for (var i = 0; i < entities.Length; i++)
    {
        Console.WriteLine($"{entities[i]} died with {healths[i].Value} HP left"); // the value is still there
        Console.WriteLine(entities[i].Has<Health>());                             // True
    }
});
```

::: warning :neofox_think: THE SPANS ONLY LIVE FOR THE CALL
Both spans are views: `values` in `AfterAdded` is a slice of the Archetype's own storage, and writing through it is visible to everyone. Don't keep them  –  copy what you need.
:::

::: tip :neofox_thumbsup: NO PHANTOM SIGNALS
A Batch only signals what actually *changes*. Re-writing a component that Entities already have (via `AddConflict.Replace`) is a value change, not an addition  –  so it stays silent. Likewise, a tolerated no-op removal (`RemoveConflict.Allow` on an Entity that never had the component) raises nothing.
:::

## Removed... but why?

A Component can go away for three quite different reasons, and your handler usually cares which. That's the third parameter, `RemoveCause`:

| Cause             | What happened                                                                     | Does the Entity survive?          |
|-------------------|-----------------------------------------------------------------------------------|-----------------------------------|
| `Removed`         | the Component was removed on its own  –  `Remove`, a Wildcard removal, a Batch    | **yes**                           |
| `Despawned`       | the Entity itself is going away, and takes everything with it                     | **no**                            |
| `TargetDespawned` | a Relation or Link lost its *target*, so it gets cleaned up                       | **yes** (but the relation dies)   |

All Entities of one call share the same cause, so you only need to check it once:

```csharp
world.SubscribeRemoved<Sprite>((entities, sprites, cause) =>
{
    foreach (var sprite in sprites) renderer.Unregister(sprite);  // always - the sprites are going away
    if (cause == RemoveCause.Despawned) return;                   // no point dressing up dying Entities
    foreach (var entity in entities) entity.Add(new NeedsRepaint());
});
```

::: warning :neofox_angel_pleading: DON'T REMOVE WHAT'S ALREADY LEAVING
Inside `BeforeRemoved` the Component is still stored, so `Has<T>()` answers `true`, but a removal is already under way. Calling `Remove<T>()` for *that* Component queues a second removal, which finds nothing once it is applied.

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

`SubscribeAdded` and `SubscribeRemoved` take the same Match as their first argument.

A Wildcard Signal fires once **per concrete expression**, and your handler receives that expression's own values:

```csharp
var entity = world.Spawn().Add(new Health(1), alpha).Add(new Health(2), beta);

world.SubscribeRemoved<Health>(Match.Any, (_, healths, _) => Console.WriteLine(healths[0].Value));

entity.Remove<Health>(Match.Any);  // prints 1 and 2 - two Signals, one structural change
```

## Handlers may change the World

Dispatch happens while a ==World Lock== is held  –  the very same [Manual World Locking](/docs/Basic/Entities/Liveness.md#manual-world-locking) you can take out yourself  –  so anything structural your handler does is queued and applied once the whole operation completes:

```csharp
// every Entity that gains a Position gains a Health, and loses it again with the Position
world.SubscribeAdded<Position>((entities, _) => { foreach (var e in entities) e.Add(new Health(100)); });
world.SubscribeRemoved<Position>((entities, _, _) => { foreach (var e in entities) e.Remove<Health>(); });
```

::: info :neofox_think: SPAWNING FROM A HANDLER
Spawning is allowed too. A Template spawn while the World is locked hands you the new Entity handles straight away, but their Components only arrive at the catch-up  –  that's what keeps the spans you were given pointing at live storage. (The same holds for a Template spawn inside a `Stream.For` runner.)
:::

## When a handler throws

A buggy handler must never look like a bug in the call that triggered it. So anything escaping a Signal is wrapped in a `SignalException`, carrying your original exception as its `InnerException`:

```csharp
world.SubscribeAdded<Health>((_, _) => throw new InvalidOperationException("boom"));

try
{
    entity.Add(new Health(1));   // this Add is perfectly valid
}
catch (SignalException e)
{
    Console.WriteLine(e.Signal);    // Signal<Health>(Plain) - who misbehaved
    Console.WriteLine(e.Entities[0]); // which Entities were being signalled
    Console.WriteLine(e.Deferred);  // False - the handler threw while running
    throw e.InnerException!;        // ...boom
}
```

Handlers can fail at two very different moments, and `Deferred` tells them apart: either the handler itself threw, or it queued an invalid Structural Change that only failed later, when the World caught up (the `Deferred` case).

```csharp
world.SubscribeRemoved<Health>((entities, _, _) => entities[0].Remove<Health>());  // already leaving!

entity.Remove<Health>();
// SignalException { Deferred = true, InnerException = InvalidOperationException }
// ...and NOT an error attributed to the Remove call above
```

::: tip :neofox_thumbsup: THE POINT OF ALL THIS
Catch `SignalException` and you are looking at an observer's fault. Anything else that comes out of `Add` / `Remove` / `Despawn` / `Submit` is a fault in the call you actually wrote.
:::

::: info :neofox_think: FIRST FAILURE WINS, AND THE WORLD STAYS WHOLE
Exceptions are fatal to the operation: dispatch stops at the first throwing handler, and the remaining subscribers do not run. There is no aggregation, and a `SignalException` is never wrapped inside another one.

A throwing `BeforeRemoved` handler aborts the operation *before* anything changed: every Aspect is signalled before a single row is deleted, so a failed `Remove`, `Despawn` or `Truncate` leaves the Entities exactly as they were. A throwing `AfterAdded` handler runs after its own Add is done; the Add stays.
:::

## What does it cost?

**You pay mostly for what you subscribe to.** A World where nobody subscribed bails out of every structural change on a single field compare; a Signal on a Component type your loop doesn't touch costs one bit lookup. Neither takes the World Lock, and neither allocates.

Measured over 10 000 Entities (.NET 10). First, what merely *having* Signals costs, with no subscriber at all, against the same operation on a build without them (`SignalOverheadBenchmarks`):

| Path                          | without Signals | with Signals, nobody subscribed |
|-------------------------------|----------------:|--------------------------------:|
| `Add` + `Remove`, per Entity  | *baseline*      | ±0 to +8 %                      |
| `Add` + `Remove`, via Batch   | *baseline*      | ±0 %                            |
| Spawn + Despawn wave          | *baseline*      | ±0 %                            |

Then, what the dispatch itself costs, against the same World with no subscriber (`SignalBenchmarks`):

| Path                          | nobody subscribed | 1 handler | 4 handlers |
|-------------------------------|------------------:|----------:|-----------:|
| `Add` + `Remove`, per Entity  | *baseline*        | +17 %     | +21 %      |
| `Add` + `Remove`, via Batch   | *baseline*        | +35 %     | +35 %      |
| Spawn + Despawn wave          | *baseline*        | ×2        | ×2.25      |

Because handlers get a whole span per operation, a Batch pays for **one call per Archetype**, not one per Entity: ten thousand Entities cost about the same dispatch as ten. Per-Entity CRUD barely notices either, because the Archetype move dwarfs the dispatch. Dispatch allocates nothing; only a Batch rents a buffer for the Entity handles it must signal after the migration.

To clear out unused Signals, call `World.GC`: it lets go of every Signal left without subscribers.
