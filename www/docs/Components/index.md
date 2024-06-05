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


# Archetype

An Archetype is a collection of Entities that share the same set of Components. Thes entities can be processed efficiently with an ECS, so in large projects, there's a performance incentive to make frequently processed Archetypes either rare (a few dozen samll ones), or large (10_000 entities or more).