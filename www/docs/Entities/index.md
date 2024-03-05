---
layout: doc
title: Entities
---

## Entities

An Entity is a specific type of [Identity](Identity.md) (a 64-bit number) associated with a specific [World](World.md).

Entities can have any number of [Components](Component.md) attached to them. This is how the **fenn**ecs Entity Component Systems provides composable, structured data semantics. 

Entities with the identical combinations of Component ==Type Expressions== share the same [[Archetype]].



## CRUD - Create, Read, Update, Delete

`fennecs.Entity` is a builder struct that combines its associated `fennecs.World` and `fennecs.Identity` to form an easily usable access pattern which exposes operations to add, remove, and read [Components](Component.md) , [Links](Link.md) , and [Relations](Relation.md). You can also conveniently Despawn the Entity.


The component data is accessed and processed in bulk through [Queries](Queries/index.md), a typical way that ECS provide composable functionality.
