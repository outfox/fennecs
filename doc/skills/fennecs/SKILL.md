---
name: fennecs
description: >-
  Develop games and simulations with fennecs, the tiny, high-energy
  Entity-Component System for modern C#/.NET. Use when writing, reviewing, or
  debugging code that uses the fennecs library — Worlds, Entities, Components,
  Queries, Streams, Relations, Object Links, or Aspects — or when the user asks
  how to structure ECS logic with fennecs.
---

# Developing with fennecs

fennecs is an archetype-based ECS for .NET 10+ (`dotnet add package fennecs`,
namespace `fennecs`). Zero codegen, no reflection, no formalized systems or
scheduler — you call query runners whenever and wherever you like.

This skill targets **fennecs 0.7.0**. When in doubt, check the user's package
version and the XML docs shipped with the package.

## Mental model

- A `World` holds Entities. **Components** are data attached to Entities — any
  value or reference type, including empty structs (tags) and records.
- Entities with the same component composition live together in an
  **Archetype**; component data is contiguous per Archetype.
- A **Query** matches Entities by the components they must have (`Has`), must
  not have (`Not`), or may have (`Any`). Queries are compiled once and cached;
  they update automatically as Archetypes appear.
- A **Stream** (`Stream<C1, …, C5>`) is a lightweight view over a Query that
  runs your delegates over component data (`For` / `Job` / `Raw`), and is also
  `IEnumerable<(Entity, C1, …)>` for LINQ and tests.
- Components can carry a **secondary key**: none (plain), an Entity target
  (**Relation**), or an object target (**Object Link**). Queries match on these
  targets too — this is fennecs' signature feature.

## Quickstart

```csharp
using fennecs;

// Components: plain types. Wrap primitives in distinct types.
record struct Position(Vector3 Value);
record struct Velocity(Vector3 Value);
struct Grounded; // empty struct = tag

using var world = new World();

var entity = world.Spawn()
    .Add(new Position(Vector3.Zero))
    .Add(new Velocity(Vector3.UnitX));   // fluent; returns the Entity

var stream = world.Query<Position, Velocity>().Not<Grounded>().Stream();

// Run every frame; uniform passes per-call data into the delegate.
stream.For(
    uniform: deltaTime,
    action: (float dt, ref Position pos, ref Velocity vel) =>
    {
        pos = pos with { Value = pos.Value + vel.Value * dt };
    });
```

Build queries and streams once (at startup or on first use) and reuse them;
they are cached and cheap to hold.

## Entity CRUD (fluent, chainable)

```csharp
entity.Add<TagType>();               // newable component, default value
entity.Add(new Health(100));         // with data
entity.Add(damage, attacker);        // Relation: component + Entity target
entity.Add(Link.With(physicsWorld)); // Object Link (target: class instance)

entity.Has<Health>();                // plain only (default Match.Plain)
entity.Has<Damage>(Match.Entity);    // any entity-relation of that type
entity.Has<Damage>(attacker);        // relation to a specific entity

ref var hp = ref entity.Ref<Health>();          // direct ref access
ref var st = ref entity.Ensure<Stamina>(full);  // get-or-add (structs)

entity.Remove<Health>();
entity.Remove<Damage>(attacker);
entity.Despawn();
if (entity.Alive) { … }              // or just: if (entity) — implicit bool
```

`Entity` is a small struct handle — store it freely in components or
collections. A despawned/stale handle reports `Alive == false`.

## Matching: the part everyone gets wrong

`default(Match)` is **`Match.Plain`** in 0.7.0. Stream types and
`Has<T>()`/`Not<T>()`/`Any<T>()` match **plain components only** unless you say
otherwise:

| Expression | Matches |
|---|---|
| `Match.Plain` *(default)* | plain components only — no relations, no links |
| `Match.Any` | everything: plain + entity relations + object links |
| `Match.Target` | any relation or link (excludes plain) |
| `Match.Entity` | any entity-entity relation |
| `Match.Object` | any object link |
| an `Entity` (implicit) / `Match.Relation(e)` | relation to that specific entity |
| `Link.With(obj)` / `Match.Link(obj)` | link to that specific object |

```csharp
var followers = world.Query<Position>().Has<Following>(leader).Stream();
var anyDamaged = world.Query<Health>().Has<Damage>(Match.Entity).Stream();
var damages = world.Query<Damage>(Match.Entity).Stream(); // relation values as stream type
```

⚠️ Wildcards (`Any`/`Target`/`Entity`/`Object`) as *stream types* can iterate
an entity once per matching component (cross-join). Deliberate feature; use
sparingly. `Job`, `Raw`, and `Blit` reject wildcard stream types entirely.

## Runners

```csharp
stream.For((ref Position p) => …);                        // single-thread
stream.For(dt, (float dt, ref Position p) => …);          // with uniform
stream.For((in EntityRef e, ref Position p) =>            // with entity access
{
    if (p.Value.Y < killPlane) e.Despawn();               // deferred, safe
});
stream.Job((ref Position p) => …);        // parallel across cores; delegate
                                          // and uniforms must be thread-safe
stream.Raw(memory => …);                  // one Memory<C> block per archetype
stream.Blit(new Velocity(Vector3.Zero));  // bulk-write one value to all
foreach (var (e, pos) in stream) { … }    // LINQ/tuples: tests, prototyping
```

- C# 14+ (the default for .NET 10) allows the shorter untyped lambda forms
  with modifiers, and the fennecs docs use them throughout:
  `stream.For((ref pos, ref vel) => …)`, `stream.For(dt, (dt, ref pos) => …)`,
  `stream.For((in entity, ref pos) => …)`. On C# ≤ 13, parameters with
  `ref`/`in` modifiers must spell out their types as shown above.
- `EntityRef` is a `ref struct` — don't store it; convert via `.Entity` to keep.
- Structural changes (Add/Remove/Despawn) made inside a runner are **deferred
  automatically** and applied when the runner's scope ends. Never invalidates
  iteration. For manual control elsewhere: `using var _ = world.Lock();`.
- Prefer `For`. Use `Job` for large workloads (it's synchronous, just
  parallel). Use `Raw`/SIMD only after profiling.

## Bulk operations

```csharp
using var spawner = world.Entity()                 // EntitySpawner
    .Add<Enemy>().Add(new Health(50)).Add(Link.With(level));
spawner.Spawn(10_000);

query.Add(new Cooldown(2f));   // bulk add — throws if query already matches it
query.Remove<Loaded>();        // bulk remove — throws unless query guarantees it
query.Despawn();               // despawn all matched
query.Truncate(1000);          // reduce matched count

// Multiple structural changes on one query: batch them.
query.Batch(Batch.AddConflict.Replace, Batch.RemoveConflict.Allow)
    .Add(new Health(100))
    .Remove<Invulnerable>()
    .Submit();                 // Dispose() the batch only if NOT submitting
```

Conflict modes: `AddConflict.{Strict, Preserve, Replace}`,
`RemoveConflict.{Strict, Allow}` — `Strict` (default) throws when the operation
isn't guaranteed valid for every matched entity.

## Sharp edges (check these first when debugging)

1. **A relation didn't match a query** → the default match is `Plain`; you need
   `Match.Entity`, a specific target, or `Match.Any` on the stream type or
   `Has`.
2. **Entities processed multiple times** → wildcard stream type cross-join.
3. **`Job` throws or misbehaves** → wildcard stream types (rejected), non
   thread-safe uniform/delegate, or WASM (single-threaded; use `For`).
4. **Bulk `Add`/`Remove` throws** → `Strict` conflict semantics; exclude the
   component via `Not<T>()` in the query, or use `Batch(...)` with
   `Preserve`/`Replace`/`Allow`.
5. **Entities "escape" between consecutive bulk ops** → each op changes which
   archetypes match; use a single `Batch(...).Submit()`.
6. **Cross-world anything** → unsupported. Relations may not target another
   World's entity (throws at creation). Max 255 concurrent Worlds per process;
   up to 2³⁰ (~1.07 billion) entities per World.
7. **Compile error storing `EntityRef`** → it's a `ref struct`; store
   `entityRef.Entity` instead.
8. **`world.GC()` throws** → only callable in Immediate mode (not inside a
   runner or an open `world.Lock()` scope).

## Idioms

- Wrap primitives: `record struct Speed(float Value)` — never add bare
  `float`/`int` components; distinct types are what queries match on.
- Tags are empty structs; presence is the data.
- Reference-type components are shared per-instance: two entities `Add`ed the
  same object see each other's mutations — deliberate tool for shared state
  (and the basis of Object Links).
- No systems classes needed: a "system" is a cached Stream plus a method that
  calls a runner. Group them in plain classes; call them in your update loop
  in whatever order you need.
- Despawn during iteration: use the `EntityRef` overload of `For` and call
  `e.Despawn()`, or collect and despawn after, or use `stream.Despawn()` /
  `world.DespawnAllWith<T>()` for whole categories.

## Going deeper (reference files in this skill)

- **[references/queries-and-streams.md](references/queries-and-streams.md)** —
  QueryBuilder details, stream filters (`Subset`/`Exclude`/`Where`), all
  runner/delegate signatures, SIMD, enumeration.
- **[references/relations-links-aspects.md](references/relations-links-aspects.md)** —
  Entity-Entity Relations, Object Links, wildcard semantics, and Aspects
  (0.7.0's storage universes for fighting fragmentation).
- **[references/world-and-lifecycle.md](references/world-and-lifecycle.md)** —
  World configuration, deferred mode/`Lock()`, GC behavior, EntitySpawner,
  Batch semantics, limits, liveness.

Full documentation: https://fennecs.net (cookbook, demos for Godot/Stride/
MonoGame/Raylib, API docs). Discord: https://discord.gg/3SF4gWhANS
