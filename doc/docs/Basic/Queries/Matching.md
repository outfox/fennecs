---
title: Matching
layout: doc
outline: [1, 2]
order: 1
description: 'Match Expressions define what a fennecs Query contains - Has, Not, Any clauses and target wildcards like Match.Plain (the default) and Match.Entity.'
---

# Query Matching

::: tip :neofox_hug_duck_heart: THE HEART OF ECS!
Match Expressions define which Entities your Query contains  –  by the Components they **must have**, **may have**, or **must not have**.
:::

## Quick Reference

| Expression | Description |
|------------|-------------|
| `Has<C>(Match = default)` | Include only Entities that have component C |
| `Not<C>(Match = default)` | Exclude Entities that have component C |
| `Any<C>(Match = default)` | Match Entities with at least one of the Any components |

::: details :neofox_science: Matching (Intermediate, see [Keys](/docs/Intermediate/Keys/))
### Concrete Match Expressions
| Expression | Description |
|------------|-------------|
| `Match.Plain` | `default` - Match only plain components (no relation)|
| `Entity e` | Relation Key matching specific Entity |
| `Link.With()` | Relation Key matching specific Link |

### Wildcard Match Expressions
| Expression | Description |
|------------|-------------|
| `Match.Any` | Match any component, with or without target |
| `Match.Target` | Match any actual target (Object or Entity) |
| `Match.Object` | Match only Object Links |
| `Match.Entity` | Match only Entity Relations |
| `Match.Family` | Match plain components of the type *or any type derived from it* |
:::

## Two Intertwined Concepts

::: info :neofox_book: MATCH TYPE
Matching groups/selects Entities by the Components they **must have, may have, or must not have**.

Queries let us find and access any set of Entities & Components **extremely quickly**.
:::

::: info :neofox_heart: MATCH TARGET
Targets further group entities by an optional **secondary key**, like an Object or another Entity.

Match Expressions with targets constitute `many-to-1` Relations (Entity-Entity, or Entity-Object).

Relations can be backed by **any Component** type  –  even **their Targets themselves** or **Shareable Components on them**!
:::

Both have low, usually zero cost  –  at runtime, as well as compile and design times. This is foundational for how Archetype-based ECS can be so fast, intuitive, and efficient.



## Teaser

Together, Types and Targets unlock Entity relations and interactions in elegant, expressive ways that in many other ECS might seem like *pure science fiction*.

![a fennec wearing a futuristic VR headset](/img/fennec-3body.png)

The [3-Body-Problem](/cookbook/staples/3Body.md) recipe and [N-Body-Problem](/examples/NBody.md) demo illustrate how to use Match Expressions to simulate complex systems of mutually interacting Entities.

## Building a Query

A match expression combines a `Match Type` and a `Target` (or `Identity`). Any number of expressions can be passed to the QueryBuilder to specify what to include and exclude.

::: details :neofox_peek_owo: DARE TO PEEK: What an expressive smörgåsbord!
#### Query Your Friends!
This query is a party invitation! It includes each entity with a Name and PlayList component, but then gets picky:

```cs
var partyGoers = world.Query<Name, PlayList>() // "()" means Match.Plain
    .Has<Fox>()                    // must be a fox
    .Has<Friendship>(you)          // has friendship relation to "you"
    .Has<Friendship>(me)           // and to "me"
    .Not<Sleeping>()               // not asleep
    .Any<Pet>()                    // has either a pet...
    .Any<Plush>()                  // ...or a plush
    .Has<Pizza>()                  // has pizza
    .Not<Likes>(pineapple)         // no Likes relation to the pineapple Entity
    .Stream();

// Last minute additions via stream filters!
var checkedGoers = partyGoers
    .Has(Comp<Vaccinated>.Plain)
    .Not(Comp<Sick>.Plain);

checkedGoers.For((ref name, ref playlist) =>
{
    DeeJay.Instruct($"{name} is coming, play something from {playlist.entries}");
});
```
:::

## Match Types

From the start, a Query includes only Entities that match all of its [Stream Types](../Streams/). This applies regardless of whether it's a Plain Component, Entity-Entity Relation, or Object Link  –  unless expressly specified in the QueryBuilder.

::: details :neofox_magnify: BEHIND THE SCENES: What does a Query even DO?
Each compiled Query maintains a collection of all Archetypes it matches (and a [filtered subset](/docs/Intermediate/Filters/Archetypes.md)). When iterating, the Query processes each Archetype in deterministic order.

Whenever a new Archetype materializes, the World notifies _all matching Queries_ of its existence.
:::

### The Three Main Match Type Expressions

| Expression | Logic | Description |
|------------|-------|-------------|
| `Has<C>()` | `A AND B AND C` | Include only Entities that have the component |
| `Not<C>()` | `NOT A AND NOT B` | Exclude Entities that have the component |
| `Any<C>()` | `A OR B OR C` | Match Entities with at least one of the Any components |

```cs
var query = world.Query<Position, Velocity>()
    .Has<Player>()        // must have Player
    .Not<Dead>()          // must not have Dead
    .Any<Buff>()          // must have either Buff...
    .Any<PowerUp>()       // ...or PowerUp (or both)
    .Stream();
```

::: details :neofox_magnify: BEHIND THE SCENES: QueryBuilder?
Technically, these are methods on `QueryBuilder<>` instances. In practice, you acquire them via `World.Query<>()` and chain the fluent interface to configure and compile immediately.
:::

## Match Targets

Stream Types and Match Expressions can both specify targets. `world.Query<ST1, ST2>()` is equivalent to `world.Query<ST1, ST2>(Match.Plain, Match.Plain)` — to match Relations or Object Links, say so explicitly.

::: tip :neofox_solder: THIS IS OUR MAIN TOOL
In ECS, the presence of a component often carries meaning in itself. Queries expose powerful, performant matching based on such presence.
:::

### Wildcards

| Wildcard | Matches |
|----------|--------|
| `Match.Plain` | Only plain components (no relations) *(default for Stream Types)* |
| `Match.Any` | Any target, including plain |
| `Match.Target` | Any actual target (Object or Entity, not Plain) |
| `Match.Object` | Only Object Links |
| `Match.Entity` | Only Entity Relations |
| `Match.Family` | Plain components of the type *or any derived type* (see below) |

```cs
// Match entities with any Damage relation (to any entity)
var damaged = world.Query<Health>()
    .Has<Damage>(Match.Entity)
    .Stream();

// Match entities with a specific relation target
var followersOfBob = world.Query<Position>()
    .Has<Following>(bob)
    .Stream();
```

::: info :neofox_knives: WILDCARDS CUT BOTH WAYS
Wildcards aren't just for matching – [`Remove<C>(Match)`](/docs/Basic/Entities/ComponentRemove.md#removing-with-wildcards) accepts them on Entities, EntityRefs (inside runners), Batches, and Templates to strip all matching components at once.
:::

### Inheritance-Aware Matching (Family)

`Match.Family` matches plain components of the given type **or any type derived from it** — the type
itself included. Base classes are discovered automatically from each component type's inheritance
chain (base classes only; interfaces do not participate), without consuming component TypeIDs.

```cs
class Animal { public int Age; }
class Fox : Animal { }
class Fennec : Fox { }
record struct Active;

// Matches entities carrying Animal, Fox, or Fennec as a plain component.
var animals = world.Query()
    .Has<Animal>(Match.Family)
    .Compile();

// Works in Not/Any clauses and stream filters, too.
var noAnimals = world.Query<Active>().Stream()
    .Not(Comp<Animal>.Matching(Match.Family));
```

As a **Stream Type**, `Match.Family` delivers derived components *viewed as their base type* —
read-only. Use the `ForRead` runners (components arrive as `in` parameters) or enumeration:

```cs
var stream = world.Query<Animal>(Match.Family).Stream();

// Fox and Fennec components arrive as Animal references; mutate them through their members.
stream.ForRead((in Animal animal) => animal.Age++);

foreach (var (entity, animal) in stream) { /* animal may be a Fennec */ }
```

::: warning :neofox_think: READ-ONLY REFERENCES
A `Storage<Fennec>` holds only Fennecs, so a Family Stream Type can never hand out a *writable*
`ref Animal` — writing a plain `Animal` into that slot would corrupt the storage. Family streams
therefore offer only `ForRead` (mutate class components in place through `in` references; the
component slot itself cannot be reassigned) and enumeration. The `ref`-based `For`, `Job`, `Raw`,
`Blit`, and `FilteredStream` views throw on Family streams.

`Match.Family` also matches **plain components only** — never Entity Relations or Object Links. On
structs (which have no user base classes) it degrades to matching the type itself.
:::

## Conflicting Match Expressions

Some expressions can cause a Query to always be empty:

::: warning :neofox_think: CONFLICT EXAMPLES
```cs
var enemies = world.Query<Enemy>()
    .Has<Position>().Not<Position>()  // Always empty!
    .Has<Objective>(Match.Target).Not<Objective>(Match.Entity)  // Only Object Links match
    .Stream();
```
:::

::: info :neofox_book: SAFETY NOTE
As of fennecs 0.5.1, the safety check that threw on conflicting expressions is removed (feedback welcome!).
:::

## Query vs. Filter

Sometimes you need to narrow down a Query dynamically:

- Perform an action on all followers of a specific character
- Pre-fetch data and pass it as a Uniform to a [Query Runner](../Streams/#passing-workloads-to-stream-queries)

::: warning :neofox_think: HARD-BAKED CRITERIA
Exclusion criteria in the QueryBuilder are immutable. What if `bob` despawns? You'd need a whole new Query!

```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)
    .Has<Owes>(bob)  // Hard-baked! Problematic if bob despawns
    .Not<Owes>(me)
    .Stream();
```
:::

::: tip :neofox_thumbsup: USE FILTERS INSTEAD!
Filters have similar performance but can be reconfigured on the fly. Streams
are lightweight value types  –  `Has`/`Not` create a filtered view, and the
original stream stays around, unfiltered:

```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)
    .Stream();

// Dynamic filtering!
var owingBobNotMe = friendsInNeed
    .Has(Comp<Owes>.Matching(bob))
    .Not(Comp<Owes>.Matching(me));
owingBobNotMe.For(PayOffDebt);

// Reconfigure when needed - just make another filtered view!
```
:::

### When to Use Filters

| Use Case | Recommendation |
|----------|----------------|
| Target entity may despawn | Use Filters |
| Many entities share same relation | Use Filters (faster iteration) |
| Relations change frequently | Use broader Query + Filters |
| Static, unchanging criteria | Hard-baked Query is fine |
