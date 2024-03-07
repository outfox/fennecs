---
layout: doc
title: Entities
---

## Entities

An Entity is a specific type of ==Identity== (a 64-bit number) associated with a specific [World](/docs/World.md).

Entities can have any number of [Components](/docs/Component.md) attached to them. This is how the **fenn**ecs Entity Component Systems provides composable, structured data semantics. 

Entities with the identical combinations of Component ==Type Expressions== share the same [[Archetype]].



## CRUD - Create, Read, Update, Delete

`fennecs.Entity` is a builder struct that combines its associated `fennecs.World` and `fennecs.Identity` to form an easily usable access pattern which exposes operations to add, remove, and read [Components](/docs/Component.md) , [Links](/docs/Link.md) , and [Relations](/docs/Relation.md). You can also conveniently Despawn the Entity.


The component data is accessed and processed in bulk through [Queries](/docs/Queries/), a typical way that ECS provide composable functionality.
