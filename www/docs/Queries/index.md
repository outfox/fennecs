---
title: Queries
layout: doc
---

# Queries

A Query is a view into a World, representing a subset of its Entities. It remains associated with this specific World, and Queries can not bridge multiple Worlds.

Queries use [Filter Expressions](FilterExpressions.md) to define this subset.

::: info :neofox_magnify: Behind the Scenes
Each Query object maintains a collection of all Archetypes it matches, and when iterating Components or enumerating Entities, the Query does so *for each Archetype*, in deterministic order.

Whenever a new Archetype materializes, the World will notify _all matching Queries_ of its existence.
:::

## CRUD - Create, Read, Update, Delete
Queries expose methods to operate quickly and with clear intent on all the entities matched by the query.

### Adding & Removing Components in Bulk
`Query.Add<C>` adds a Component to each Entity in the Query. This throws if the Query already matches that Component - either ALL entities in the query would already have the Component, or the query would be empty anyway.

`Query.Remove<C>` removes a Component from each Entity in the Query. This throws if the Query doesn't match that Component - NO entities in the query would have the Component on them, anyway.


### Adding & Removing Links in Bulk
(TODO) object links can also be added. (coming beta 1.2)

### Adding & Removing Relations in Bulk
(TODO) entity relations can also be added. (coming beta 1.2)


### Deleting Entities in Bulk
Use the method `Query.Clear()` to despawn all Entities in that Query.
Alternatively, use `Query.Truncate(int, TruncateMode)` to cut your Query down to a specific size.

## Cleaning up unused Queries
Queries have a modest memory footprint, but in Worlds with many fragmented Archetypes, the cost of Query update notifications can add up! Call `Query.Dispose()` to de-register a query from its World and free its resources.

