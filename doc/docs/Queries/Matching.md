---
title: Matching
layout: doc
outline: [1, 2]
order: 1
---

# :neofox_snug_glare: Matching

::: tip :neofox_thumbsup: THE HEART OF ECS!
Match Expressions define which Entities your Query contains  –  by the Components they **must have**, **may have**, or **must not have**.
:::

## Quick Reference

| Expression | Description |
|------------|-------------|
| `Has<C>()` | Include only Entities that have component C |
| `Not<C>()` | Exclude Entities that have component C |
| `Any<C>()` | Match Entities with at least one of the Any components |
| `Match.Plain` | Match only plain components (no relations) |
| `Match.Any` | Match any target (default for Stream Types) |
| `Match.Target` | Match any actual target (Object or Entity) |
| `Match.Object` | Match only Object Links |
| `Match.Entity` | Match only Entity Relations |

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
var partyGoers = world.Query<Name, PlayList>() // "()" means Match.Any
    .Has<Fox>()                    // must be a fox
    .Has<Friendship>(you)          // has friendship relation to "you"
    .Has<Friendship>(me)           // and to "me"
    .Not<Sleeping>()               // not asleep
    .Any<Pet>()                    // has either a pet...
    .Any<Plush>()                  // ...or a plush
    .Has<Pizza>()                  // has pizza
    .Not<Likes>(Identity.Of("pineapple"))  // doesn't like pineapple
    .Stream();

// Last minute additions via filters!
partyGoers.Subset<Vaccinated>(Match.Plain);
partyGoers.Exclude<Sick>();

partyGoers.For((ref name, ref playlist) =>
{
    DeeJay.Instruct($"{name} is coming, play something from {playlist.entries}");
});
```
:::

## Match Types

From the start, a Query includes only Entities that match all of its [Stream Types](../Streams/). This applies regardless of whether it's a Plain Component, Entity-Entity Relation, or Object Link  –  unless expressly specified in the QueryBuilder.

::: details :neofox_magnify: BEHIND THE SCENES: What does a Query even DO?
Each compiled Query maintains a collection of all Archetypes it matches (and a [filtered subset](/docs/Streams/Filters.md)). When iterating, the Query processes each Archetype in deterministic order.

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

Stream Types and Match Expressions can both specify targets. `world.Query<ST1, ST2>()` is equivalent to `world.Query<ST1, ST2>(Match.Any, Match.Any)`.

::: tip :neofox_solder: THIS IS OUR MAIN TOOL
In ECS, the presence of a component often carries meaning in itself. Queries expose powerful, performant matching based on such presence.
:::

### Wildcards

| Wildcard | Matches |
|----------|--------|
| `Match.Any` | Any target, including plain *(default for Stream Types)* |
| `Match.Plain` | Only plain components (no relations) |
| `Match.Target` | Any actual target (Object or Entity, not Plain) |
| `Match.Object` | Only Object Links |
| `Match.Entity` | Only Entity Relations |

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
Filters have similar performance but can be dynamically reconfigured:

```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)
    .Stream();

// Dynamic filtering!
friendsInNeed.Subset<Owes>(bob);
friendsInNeed.Exclude<Owes>(me);
friendsInNeed.For(PayOffDebt);

// Reconfigure when needed
friendsInNeed.ClearFilters();
```
:::

### When to Use Filters

| Use Case | Recommendation |
|----------|----------------|
| Target entity may despawn | Use Filters |
| Many entities share same relation | Use Filters (faster iteration) |
| Relations change frequently | Use broader Query + Filters |
| Static, unchanging criteria | Hard-baked Query is fine |