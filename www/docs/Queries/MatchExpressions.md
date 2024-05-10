---
title: Match Expressions
layout: doc
outline: [2, 3]
---

# Match Expressions

From the start, a Query includes only entities that match all of its [Stream Types](Query.1-5.md).

In ECS, the presence of another component often carries a meaning in itself, and Queries expose powerful, performant matching of Entities based on such presence.

# `Query<>.Has<C>()`
# `Query<>.Has<C>(Identity)`
Query includes only Entities that have the given component or relation.

# `Query<>.Not<C>()`
# `Query<>.Not<C>(Identity)`
Query excludes all Entities that have the given component.

# `Query<>.Any<C>()`
# `Query<>.Any<C>(Identity)`
Query matches Entities that match at least one the Any statements.


::: details :neofox_magnify: Behind the Scenes
Technically, all of these are actually methods on `QueryBuilder<>` instances, but in practice you almost never type the word `QueryBuilder`. Instead, you acquire them via `World.Query<>`. The builders expose a fluent interface to configure and then compile the query right away.
:::

## Mix & Match!

When building a Query with Match Expressions, any number of Match Expressions can be passed to the QueryBuilder.

::: info ~~INVITE~~ QUERY YOUR FRIENDS!

```cs
// includes each entity with a Name component and a music PlayList
// (Name and PlayList become the output Stream Types of the query)
var partyGoers = world.Query<Name, PlayList>() 
//... who is also a fox
    .Has<Fox>()
//... and has a Friendship relation to entity "me"
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
// compile query to register it with the world (alternative: .Cache())
    .Compile();

partyGoers.For((ref name, ref playlist) =>
{
    DeeJay.Instruct($"{name} is coming, please play something from {playlist.entries}");
});
  ```
:::

::: info :neofox_magnify: BEHIND THE SCENES
Each Query object maintains a collection of all Archetypes it matches, and when iterating Components or enumerating Entities, the Query does so *for each Archetype*, in deterministic order.

Whenever a new Archetype materializes, the World will notify _all matching Queries_ of its existence.
:::


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
    .Build();
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
- pre-fetch a certain piece of data and then pass it in as a Uniform to a [Query Runner](Query.1-5.md#passing-workloads-to-stream-queries)

::: warning :neofox_think: Paws for Thought: SUBSETs and EXCLUSIONs
Exclusion criteria like those below are hard-baked into the query. This is *slightly more performant*, but semantically inflexible. 

```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)  // we care about Entity-Entity relations
    .Unchecked()    // because the next two conflict with Entity
    .Has<Owes>(bob) // subset - specifically anyone who owes bob
    .Not<Owes>(me)  // exclusion - but who does not owe myself
    .Build();

friendsInNeed.For(PayOffDebt);
```
What if bob despawns?  In that case, a whole new query needs to be compiled (this is code you need to write), and any systems that use the query just would need to be notified of bob's death or its implications!

:::

As a rule of paw, consider [Filters](Filters.md) first for these cases. A Filter has similar performance characteristics to a compiled (and thus immutable) query, and can be dynamically reconfigured!

::: tip :neofox_thumbsup: PROTIP: SUBSET and EXCLUSION via [Stream Filters](Filters.md)
Our Query is always valid as compiled, and we can dynamically narrow down its matched archetypes whenever our criteria change. 
```cs
var friendsInNeed = world.Query<Friend>()
    .Has<Owes>(Match.Entity)  // we care about Entity-Entity relations
    .Build();

friendsInNeed.Subset<Owes>(bob);
friendsInNeed.Exclude<Owes>(me);
// do our thing, pay their debt with bob
friendsInNeed.For(PayOffDebt);
// optional: restore or change when criteria change
friendsInNeed.ClearFilters();
```

If needed, the filter state can stick around forever (at a minimal cost per query whenever new relations create new archetypes).

:::

