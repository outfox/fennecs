---
title: WIP Filter Expressions
layout: doc
---

# Filter Expressions

## Creating a Query using Filter Expressions

::: info :neofox_magnify: Behind the Scenes
Each Query object maintains a collection of all Archetypes it matches, and when iterating Components or enumerating Entities, the Query does so *for each Archetype*, in deterministic order.

Whenever a new Archetype materializes, the World will notify _all matching Queries_ of its existence.
:::


## Cleaning up unused Queries

Queries have a modest memory footprint, but in Worlds with many fragmented Archetypes, the cost of Query update notifications can add up! Call `Query.Dispose()` to de-register a query from its World and free its resources.

