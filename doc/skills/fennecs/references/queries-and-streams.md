# Queries & Streams — full reference (fennecs 0.7.0)

## Building Queries

Two entry styles, both fluent, both starting from a `World` (or an `Aspect`):

```csharp
// Generic: stream types are baked in; .Stream() compiles and returns the view.
var stream = world.Query<Position, Velocity>()   // stream types match Plain by default
    .Has<Player>()                                // must have (Plain by default)
    .Not<Dead>()                                  // must not have
    .Any<Buff>().Any<PowerUp>()                   // at least one of the Any-group
    .Stream();

// Non-generic: arbitrary expressions, compile to a bare Query, stream later.
var query = world.Query().Has<Position>().Not<Boring>().Compile();
var positions = query.Stream<Position>();
var swapped   = query.Stream<Velocity, Position>();  // any view over the same query
```

Every `Has/Not/Any` accepts an optional `Match` (see SKILL.md table) or a
`Link<T>`:

```csharp
.Has<Following>(leader)          // relation to specific entity (implicit Match)
.Has<Damage>(Match.Entity)       // any entity relation
.Has(Link.With(sharedTexture))   // specific object link
.Not<Owes>(Entity.Any)           // no entity relation of this type at all
.Has<Animal>(Match.Family)       // plain Animal OR any type derived from it
```

`Entity.Any` ≡ `Match.Entity`; `Link.Any` ≡ `Match.Object`.

`Match.Family` is inheritance-aware: it matches plain components of the named
type or any type derived from it (self included; base-class chains only,
interfaces don't participate; structs degrade to self-only). Legal in
`Has/Not/Any` and stream filters (`Comp<T>.Matching(Match.Family)`), and as a
*stream type* with **read-only** data access: derived components arrive viewed
as their base type via the `ForRead` runners (`in` parameters — mutate class
components through their members; the slot can't be reassigned) or enumeration.
The writable surfaces (`ref`-based `For`, `Job`, `Raw`, `Blit`, and
`Has/Not/Where` filter views) throw on Family streams, because a writable
`ref Base` over a `Storage<Derived>` could store a non-derived instance:

```csharp
var animals = world.Query<Animal>(Match.Family).Stream();
animals.ForRead((in Animal a) => a.Tick());          // Fox, Fennec arrive as Animal
animals.ForRead(dt, (float dt, in Animal a) => …);   // uniform variant
animals.ForRead((in EntityRef e, in Animal a) => …); // entity variant (4 overloads total)
```

- Queries are cached per world: compiling an identical expression returns the
  cached instance. Hold streams/queries in fields; don't rebuild per frame
  (rebuilding works, it's just pointless allocation).
- `query.Warmup()` pre-allocates internals if you want to avoid first-use cost.
- Conflicting expressions (e.g. `Has<T>().Not<T>()`) are legal and simply
  match nothing.

## Query members

`Query : IReadOnlySet<Entity>, IDisposable`

| Member | Notes |
|---|---|
| `Count`, `IsEmpty` | matched entity count |
| `Contains(entity)`, `Contains<T>(match)` | membership tests |
| `this[int index]`, `Random()` | positional / random access |
| `GetEnumerator()` | enumerate matched Entities; full `IReadOnlySet` ops too |
| `Add<T>()/Add<T>(value)`, `Remove<T>()` | bulk structural ops (Strict — throw on conflict) |
| `Batch(…)` | see world-and-lifecycle.md |
| `Despawn()`, `Truncate(n, mode)` | bulk despawn; `TruncateMode.{Proportional, PerArchetype}` |
| `Stream<C…>(match…)` | create stream views (arity 1–5) |

## Stream views

`Stream<C0..C4>` is a `readonly record struct` — cheap to copy, safe to store.
`stream.Query` exposes the underlying Query. `stream.Count` = matched entities.

### Runner & delegate signatures (arity pattern; 1–5 stream types)

```csharp
// For — single-threaded
void For(ComponentAction<C0,…> action);                       // (ref C0, …)
void For<U>(U uniform, UniformComponentAction<U,C0,…>);       // (U, ref C0, …)
void For(EntityComponentAction<C0,…>);                        // (in EntityRef, ref C0, …)
void For<U>(U uniform, UniformEntityComponentAction<U,C0,…>); // (U, in EntityRef, ref C0, …)

// Job — same shapes as the first two, parallelized across cores
void Job(ComponentAction<C0,…>);
void Job<U>(U uniform, UniformComponentAction<U,C0,…>);

// Raw — whole archetype storages as contiguous memory
void Raw(MemoryAction<C0,…>);                                 // (Memory<C0>, …)
void Raw<U>(U uniform, MemoryUniformAction<U,C0,…>);          // (U, Memory<C0>, …)
```

Components arrive as `ref` — write through them directly. Uniforms are passed
by value (contravariant `in U`, so a delegate taking a base type accepts a
derived uniform). A mutable reference-type uniform is the idiom for
accumulating results ("varyings at home") — make it thread-safe for `Job`
(`System.Collections.Concurrent`).

### Job specifics

- Synchronous: returns when all chunks are done. No scheduler, no handles.
- Splits work per archetype into chunks across cores; worth it for large
  entity counts and meaty per-entity work, otherwise `For` wins.
- Rejects wildcard stream types (asserts) — `Plain` or concrete targets only.
- Unsupported on browser/WASM (`[UnsupportedOSPlatform("browser")]`).
- No `EntityRef` overloads — structural changes belong in `For` or after.

### Raw & SIMD

`Raw` hands you one `Memory<C>` per stream type **per archetype** (called once
per archetype). Use for vectorization (AVX/SSE via `Span`), interop, or bulk
`memcpy`-style work. Storage order is stable within a call but changes across
structural changes — never cache the memory.

`stream.Blit(value, match = default)` broadcast-writes one value to the
component on all matched entities (no wildcards).

**Fox typing** (`Fox<T>`, `Fox128<T>`, `Fox256<T>`): interfaces for wrapper
components exposing `.Value` — give semantically different data distinct
component types (e.g. `Health : Fox<float>`, `Shield : Fox<float>`) while
keeping SIMD-friendly layouts.

### Enumeration (LINQ)

`Stream<C…>` is `IEnumerable<(Entity, C0, …)>`; `Query` and `World` enumerate
`Entity`. Copies, not refs — great for tests, debugging, prototyping; use
runners for hot paths.

```csharp
foreach (var (entity, pos, vel) in stream) { … }
Assert.All(stream, t => Assert.True(t.Item2.Value > 0));
```

### Stream filters — narrow without recompiling

Queries bake their criteria immutably. Streams add cheap, reconfigurable
narrowing — `Has(...)` (must have ALL), `Not(...)` (must have NONE), and
`Where(...)` each return a `FilteredStream<C…>` view; chaining `Has`/`Not`
accumulates expressions:

```csharp
var filtered = stream
    .Has(Comp<Unlucky>.Plain)
    .Not(Comp<Lucky>.Plain, Comp<Owes>.Matching(eve));
```

`Comp<T>` expressions: `Comp<T>.Plain`, `Comp<T>.Matching(match)` (an `Entity`
converts implicitly), `Comp<T>.Matching(linkedObject)`.

Prefer a broad query + stream filters when the narrowing target is dynamic
(e.g. a specific entity that may despawn); prefer baked query expressions for
static criteria.

Per-value predicate filtering — `Where` (LINQ-style, evaluated per entity):

```csharp
var moving = stream
    .Where((in Velocity v) => v.Value.LengthSquared() > 0.01f)
    .Where((in Health h) => h.Current < h.Max);   // different component: ANDed
```

`Where` takes a `ComponentFilter<C>` (`bool (in C c)`) for one of the stream's
type parameters and returns a `FilteredStream` — spell out the lambda's
parameter type; it selects the overload. One predicate slot per stream type:
chaining across different components ANDs them, but a second `Where` on the
same component type *replaces* that slot (combine conditions in one lambda
instead). A `FilteredStream` honors ALL its filters in everything it offers:
`For`, `Job` (predicates must be thread-safe), enumeration, `Count` (O(n) with
predicates), and `Despawn`. `Raw`/`Blit` are not offered — drop down via the
`filtered.Stream` property for block operations.

Two-delegate runners visit the complement too: every `For`/`Job` variant has an
`(included, excluded)` overload — `included` runs on entities passing all
filters, `excluded` on every other entity the Query matches (pruned archetypes
AND predicate rejects); together they visit each entity exactly once. Not
available on wildcard streams (and `Despawn` with predicates set also asserts
no wildcards):

```csharp
filtered.For(
    included: (ref Pos p, ref Health h) => Advance(ref p),
    excluded: (ref Pos p, ref Health h) => h.Regen());
```

### Wildcard cross-join semantics

A wildcard *stream type* (`Match.Any/Target/Entity/Object`) yields the entity
once per matching component. Two wildcard stream types = cartesian product per
entity. This is deliberate — it's how the N-Body demo pairs every body with
each of its attractors in one `For`:

```csharp
// Attractor is a relation; each body is visited once per attractor.
var accumulate = world.Query<Attractor, Body, Acceleration>(
        Match.Entity, Match.Plain, Match.Plain).Stream();
```

Wildcards in `Has/Not/Any` filter expressions are free — no cross-join; only
stream types multiply iteration.
