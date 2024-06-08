---
title: Components
order: 4
outline: [1, 3]
---

# Components

Components are data attached to Entities!

### Entities can have ...
1. zero or one of each component type
1. any number of unique components

### Components can ...
1. be added/removed to an Entity
1. have any C# language type
1. have an optional [Target](/docs/Queries/Matching.md#match-targets) (secondary key) associated with them
1. be shared between Entities (by reference)
1. be [Queried](/docs/Queries/index.md) for and processed by [Streams](/docs/Streams/index.md)


![a cartoon fennec with a hand truck moving a large stack of colored boxes](https://fennecs.tech/img/fennec-components.png)
*(a cute fennec moving into its new Archetype!)

# Archetypes

An Archetype is a collection of Entities that share the same set of Components. Such collections can be processed most efficiently by the ECS, so in large projects, there's a performance incentive to make frequently processed Archetypes either rare (e.g. aim for just a few dozen small ones), or chunky and large (e.g. 10k entities for a start, or even more).