---
title: Components
order: 4
---

# Components

Components are data attached to Entities!

### Entities can have ...
0. no components at all
1. zero or one of each component type
2. any number of unique components

### Components can ...
0. be added/removed to an entity
1. be shared between entities (by reference)
2. be queried for in systems


![a cartoon fennec with a hand truck moving a large stack of colored boxes](https://fennecs.tech/img/fennec-components.png)


# Archetype

An Archetype is a collection of Entities that share the same set of Components. Thes entities can be processed efficiently with an ECS, so in large projects, there's a performance incentive to make frequently processed Archetypes either rare (a few dozen samll ones), or large (10_000 entities or more).