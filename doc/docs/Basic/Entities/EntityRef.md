---
title: EntityRef
order: 10
outline: [1, 2]
description: 'EntityRef is a light Entity use when nowing the entity is alive. Allowing faster access to component.'
---

# EntityRef :neofox_peek_owo:

*an Entity that already knows where it lives*

::: tip :neofox_thumbsup: You Don't Create These
`EntityRef` is handed **to** you  –  by [Stream runners](/docs/Basic/Streams/Stream.For.md) and by [Signal handlers](/docs/Advanced/Events/index.md). 
Everywhere else, you work with the plain 64-bit [`Entity`](index.md).
:::

## What is an EntityRef?

An `Entity` is an identity: a 64-bit handle that has to be *looked up* before its data can be touched. 
An `EntityRef` is that lookup already done  –  it holds the Entity's Archetype and its row in it.

That has two consequences, and they're the whole story:

- **It's fast.** `Ref<C>()` goes straight into the Archetype's storage, skipping both the Meta lookup and the liveness check.
- **It's a `ref struct`.** It cannot be stored, boxed, or captured. It's valid for the call that gave it to you, and not one instruction longer.

## Why it's always alive

Runners and Signal dispatch both hold a [World Lock](Liveness.md#manual-world-locking), so nothing can despawn or move an Entity while you hold its `EntityRef`. That's the guarantee that lets it skip the liveness check; `entity.Alive` inside a runner is `true` by construction.

::: warning :neofox_think: DEFERRED, NOT IGNORED
A `Despawn()` (or `Remove`) you issue on an `EntityRef` is *queued*, not applied. 
The reference stays usable for the rest of the iteration  –  but reads after a deferred `Remove` still show you the old data. 
See [Deferred Operations](Liveness.md#deferred-operations-liveness).
:::

## Keeping one around

You can't. That's the point  –  and the compiler will tell you so.

```cs
// ❌ won't compile: ref struct in a generic
var wounded = new List<EntityRef>();
world.Stream<Health>().For((entity, ref health) =>
{
    // ❌ nope
    if (health.Value < 10) wounded.Add(entity);
});
```

Convert it to a storable `Entity` first  –  via the `Entity` property, or just by assigning it where an `Entity` is expected (the conversion is implicit):

```cs
// ✅
var wounded = new List<Entity>();
world.Stream<Health>().For((entity, ref health) =>
{
    // ✅ 64-bit handle, carries its generation
    if (health.Value < 10) wounded.Add(entity.Entity);
});

// later, after the runner - the World may have moved on, so check
foreach (var fox in wounded) if (fox.Alive) fox.Add<Bandaged>();
```

::: info :neofox_science: WHY THE ASYMMETRY?
An `Entity` carries a *generation* counter, so a stale handle can be detected long after the fact ([Liveness](Liveness.md)).
An `EntityRef` carries a row number instead  –  meaningful only while the World is frozen.
Storing one would be storing a pointer into memory that's about to be rearranged.
:::

## What you can do with it

Everything you'd do with an `Entity`, minus the ability to keep it.

| Member | What it does |
|--------|--------------|
| `Ref<C>(match)` | `ref` to the component, straight from Archetype storage → [Read/Write](ComponentRefGet.md) |
| `Has<C>()` / `Has<C>(match)` / `Has<R>(target)` | component, relation, and link checks → [Check](ComponentHas.md) |
| `Add<C>(...)` / `Remove<C>(...)` | structural changes, **deferred** while the lock is held → [Add](ComponentAdd.md) · [Remove](ComponentRemove.md) |
| `Despawn()` | also deferred → [Despawning](Despawning.md) |
| `Entity` | the storable 64-bit handle (also an implicit conversion) |
| `World` | the World this Entity lives in |
| `Alive` | always `true` while you hold the reference |

::: details :neofox_magnify: BEHIND THE SCENES
An `EntityRef` is exactly two fields: the `Archetype` and an `int` row. 
`Ref<C>()` first tries that Archetype's own storage  –  one dictionary probe, then an indexed span read. 
If the component lives in a *different* [Aspect](/docs/Advanced/Aspects/index.md), it transparently falls back to the World lookup, so you never have to care where the data is stored.

Because it can never be boxed, generic code reaches its interfaces through `where T : IAddRemove<T>, allows ref struct`  –  constrained, allocation-free dispatch.
:::

## Where to next?

1. [Liveness](Liveness.md)  –  the World Lock that makes an EntityRef safe
2. [Stream Delegates](/docs/Basic/Streams/Delegates.md)  –  every runner shape that hands you one
3. [Component Signals](/docs/Advanced/Events/index.md)  –  the other place they turn up
