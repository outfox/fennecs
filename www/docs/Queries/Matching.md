---
title: Matching
layout: doc
outline: [1, 3]
order: 1
---

# Matching

Perk up your ears! Match Expressions ware what makes **fenn**ecs so cool and powerful, and it is a complex topic, shaped by two intertwined ideas: 

1. the `Match Type`, a commonplace concept in ECS
2. the `Match Target` (or `Match Identity`), a feature unique to **fenn**ecs

Together, they enable Entity interactions natively in **fenn**ecs in elegant, expressive ways that in other ECS might seem like pure science fiction. 

![a fennec wearing a futuristic VR headset](https://fennecs.tech/img/fennec-3body.png)

The [3-Body-Problem](/cookbook/staples/3Body.md) recipe and [N-Body-Problem](/examples/NBody.md) demo illustrate how to use Match Expressions to simulate complex systems of mutually interacting Entities.

## Sneak Dive / Deep Peek
A match expression is split into a `Match Type` and a `Target` (or `Identity`), and they can be combined in any way to create complex Queries that match (and enumerate!) exactly the Entities and Components you need.

When building a Query with Match Expressions, any number of Match Expressions can be passed to the QueryBuilder to specify what this Query should include and exclude. Here's a quick example to give an overview over what's available


::: details :neofox_peek_owo: DARE TO PEEK: What an expressive smörgåsbord !
#### ~~INVITE~~ QUERY YOUR FRIENDS!
This query is a bit of a party invitation! It includes each entity with a Name component and a music PlayList (Name and PlayList become the output Stream Types of the query)

 ... but then it gets picky!
```cs
var partyGoers = world.Query<Name, PlayList>() // "()" means Match.Any
//... who is also a fox
    .Has<Fox>()
//... and has a Friendship entity relation to both "you" & "me"
    .Has<Friendship>(you)
    .Has<Friendship>(me)
//... and who's not asleep
    .Not<Sleeping>()
//... and has -either- a pet or a plush component
    .Any<Pet>()
    .Any<Plush>()
//... and has pizza
    .Has<Pizza>()
//... but only if they specifically don't like (the string) pineapple
    .Not<Likes>(Identity.Of("pineapple"));
// compile query to register it with the world
    .Compile();


// WAIT! It's the 2020s. We need two last minute additions!
partyGoers.Subset<Vaccinated>(Match.Plain); // party is a safe space!
partyGoers.Exclude<Sick>(); // if that even needs saying


partyGoers.For((ref name, ref playlist) =>
{
    DeeJay.Instruct($"{name} is coming, please play something from {playlist.entries}");
});
```
:::

## Match Types

From the start, a Query includes only Entities that match all of its [Stream Types](Stream.1-5.md).
It does so regardless of whether it's a Plain Component, a Entity-Entity Relation, or an Object Link - unless expressly specified for each stream type in the QueryBuilder factory method. (see: [Match Targets](#match-targets)

::: details :neofox_magnify: BEHIND THE SCENES: What does a Query even DO?
Each compiled Query object maintains a collection of all Archetypes it matches (and a [filtered subset](Filters.md)), and when iterating Components or enumerating Entities, the Query does so *for each Archetype*, in deterministic order.

Whenever a new Archetype materializes, the World will notify _all matching Queries_ of its existence.

The Query feeds the Storages corresponding to its Stream Types of each matched Archetype to its [Runner Delegates](Delegates.md).
:::

-----

### The three main `Match Type` Expressions are:

> ### `Query<>.Has<C>()` and `Query<>.Has<C>(Identity)`
> `includes only` Entities that have the given component or relation. Multiple `Has` statements can be compared to a logical `A AND B AND C`.

> ### `Query<>.Not<C>()` and `Query<>.Not<C>(Identity)`
> `excludes` any Entities that have the given component. Multiple `Not` statements can be compared to a logical `NOT (A OR B OR C)`, aka. `(NOT A) AND (NOT B) AND (NOT C)`.

> ### `Query<>.Any<C>()` and `Query<>.Any<C>(Identity)`
> matches Entities that match `at least one` of the Any statements. Multiple `Any` statements can be compared to a logical `A OR B OR C`.

 
A Query being built is then further refined through Match Expressions passed to the builder's methods. These expressions are the building blocks of the query, and they can be combined in any way to create complex queries that match exactly the Entities you need.

::: details :neofox_magnify: BEHIND THE SCENES: Huh, Builders, Queries, confused yet?!
Technically, all of these are actually methods on `QueryBuilder<>` instances, but in practice you almost never type the word `QueryBuilder`. Instead, you acquire them via `World.Query<>`. The builders expose a fluent interface to configure and then compile the query right away.
:::

## Match Targets

Not only the Stream Types can match specific or wildcarded targets. `world.Query<ST1, ST2>()` is the same as `world.Query<ST1, ST2>(Match.Any, Match.Any)`, but these targets can be other Wildcards, or the IdEntities of specific Objects and Entities.

The same is true for any other `Match Type` expression, like `Has`, `Not`, or `Any`.

::: tip :neofox_solder: THIS IS OUR MAIN TOOL
In ECS, the presence of another component often carries a meaning in itself, and Queries expose powerful, performant matching of Entities based on such presence.
:::

The `default` Identity, also known as `Match.Plain`, matches only components that have no targets, i.e. that are just pure data without any relational meaning.

In addition to specific idEntities, there are five virtual wildcard IdEntities:
- `Match.Any` matches any target, including the default. 
  - *this is the silent default for all Stream Types, unless otherwise specified*
- `Match.Target` matches any actual targets (both Object and Entity, but not Plain)
- `Match.Object` matches only Object targets (so called Object Links)
- `Match.Entity` matches only Entity targets (so called Entity-Entity Relations)
- `Match.Plain` matches only components without a targets. This is great for [Tags](../Components/Tags.md) (like `Dead` or `Invisible`)



## Cleaning up unused Queries

Queries have a modest memory footprint, but in Worlds with many fragmented Archetypes, the cost of Query update notifications can add up! Call `Query.Dispose()` to de-register a query from its World and free its resources.


## Conflicting Match Expressions
QueryBuilders start in checked mode: An internal safety flag tells them to throw exceptions whenever duplicate or conflicting Match Expressions are incorporated.

::: info Examples of Conflicts

```cs
var enemies = world.Query<Enemy>()
// already covered by stream type
    .Has<Enemy>()
// duplicate
    .Has<Velocity>().Has<Velocity>()
// always empty, never possible to match
    .Has<Position>().Not<Position>()` 
// Match.Target already includes Object, but Object excludes Entity.  
    .Has<Objective>(Match.Object).Has<Objective>(Match.Target) 
    .Compile();

  ```
:::

This helps remediate minor developer oversights that can happen especially during refactors or merges. However, these can add up as the number of queries and components grows. Semantically inconsistent queries may become the cause of subtle, insidious bugs (or lead to hard-to-debug refactors or seemingly fine SCM diffs later in the project life cycle, where it is easy to miss a bug in plain sight).

However, this safety means QueryBuilders that are programmatically composed or set up late during runtime may throw as they are being configured.

### Unchecked Queries

Calling the `Unchecked()` builder method disables the safety on QueryBuilder's mask, allowing, for all subsequent operations:
- repeat and overlapping types
- conflicting types (even if that would cause the query to always be empty)

Unchecked mode will also allow building a Query that matches groups of relations while excluding a small subset / single target.

### Unchecked Query vs. Filter
Sometimes, it is useful to narrow down a Query a little (or a lot).

For example:
- perform an action on all followers of a specific character
- pre-fetch a certain piece of data and then pass it in as a Uniform to a [Query Runner](Stream.1-5.md#passing-workloads-to-stream-queries)

::: warning :neofox_think: PAWS FOR THOUGHT: When does the matching / filtering happen?
Exclusion criteria like those below are hard-baked into the query. This is *slightly more performant*, but semantically inflexible. 

```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)  // we care about Entity-Entity relations
    .Unchecked()    // because the next two conflict with Entity
    .Has<Owes>(bob) // subset - specifically anyone who owes bob
    .Not<Owes>(me)  // exclusion - but who does not owe myself
    .Stream();

friendsInNeed.For(PayOffDebt);
```
What if bob despawns?  In that case, a whole new query needs to be compiled (this is code you need to write), and any systems that use the query just would need to be notified of bob's death or its implications!

:::

As a rule of paw, consider [Filters](Filters.md) first for these cases. A Filter has similar performance characteristics to a compiled (and thus immutable) query, and can be dynamically reconfigured!

::: tip :neofox_thumbsup: PROTIP: SUBSET and EXCLUDE via [Stream Filters](Filters.md)
Our Query is always valid as compiled, and we can dynamically narrow down its matched archetypes whenever our criteria change. 
```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)  // we care about Entity-Entity relations
    .Stream();

friendsInNeed.Subset<Owes>(bob);
friendsInNeed.Exclude<Owes>(me);
// do our thing, pay their debt with bob
friendsInNeed.For(PayOffDebt);
// optional: restore or change when criteria change
friendsInNeed.ClearFilters();
```

If needed, the filter state can stick around forever (at a minimal cost per query whenever new relations create new archetypes).

:::

This mechanism is great when the number of different relations stays relatively constant during the lifetime of the query; if each frame or tick several different `Owes => Entity` relations are added or removed, then the broader query will get updated under the hood each time that happens.

It's also great if there are many Entities that have the same relation, because it makes iterating the query faster than having a query that spans various fragmented, tiny archetypes. However, if there are only very few Entities (let's say in the dozens), then the performance impact would be negligible.