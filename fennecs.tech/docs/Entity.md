---
layout: doc
title: Entity
---

# Entity

An Entity is a specific type of [Identity](Identity.md) (a 64-bit number) associated with a specific [World](World.md).

Entities can have any number of [Components](Component.md) attached to them. 

Entities with the identical combinations of Component Types share the same [[Archetype]]

The `fennecs.Entity` struct exposes operations to add, remove, and read [Components](Component.md) , [Links](Link.md) , and [Relations](Relation.md).

This is how the **fenn**ecs Entity Component Systems provides composable, structured data semantics. 

The component data is accessed and processed in bulk through [Queries](Query.md), a typical way that ECS provide composable functionality.
