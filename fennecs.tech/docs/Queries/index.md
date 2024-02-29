---
title: Queries
---

# Queries

A Query is a view into a World, containing a subset of all Entities. It is associated with this specific World, and Queries can not bridge multiple Worlds.



## Processing Data in Queries
See: [`Query<C0, ...>`](Query.1-5.md)

:neofox_what:


::: info Behind the Scenes
Each Query object maintains a collection of all Archetypes it matches, and when iterating Components or enumerating Entities, the Query does so for each Archetype. The order is roughly deterministic.

Whenever a new Archetype materializes, the World will notify all _matching Queries_ of its existence.
:::
