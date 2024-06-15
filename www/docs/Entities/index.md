---
layout: doc
title: Entities
order: 3
outline: [1, 2]
---
# Entities
### Entities are  lightweight, immutable objects
`readonly record structs`, technically

You can simply store them in variables, collections, or even in Component attached to another Entity.

### Components can be (and usually will be) attached to Entities
... unless you are very boring.


### Entities have a simple lifecycle
Each Entity knows if it is [alive](Liveness.md) inside a World, and an Entity can only live in up to one World at once, and it needs a World to be alive. (don't we all!)

Despawned Entities are recycled, so they are extremely cheap to spawn and process even in large waves without runaway memory consumption.


![fennecs in a box](https://fennecs.tech/img/fennecs-512.png)
*(cuddly, lively, come in litters of `1,073,741,824`)*

## Composition
Entities can have any number of [Components](/docs/Components/) attached to them. This is how the **fenn**ecs Entity-Component Systems provides composable, structured data semantics. 


They can also serve as the `secondary key` in a [Relation](/docs/Components/Relation.md)) between two Entities. 

Entities with the identical combinations of Component ==Type Expressions== share the same [Archetype](../Components/index.md#archetypes).

A dead Entity never has Components. 


## Internals
::: details Tidbits for the curious
The defining property of an entity is its `Identity` - a specific type of 64-bit number. Associated with a specific [World](/docs/World.md), this gives us an object to operate on.

A dead Entity doesn't exist in any World (it's just a scrap of data and a leftover `Identity` whose successor was already returned to to the internal IdentityPool).


Living Entities sit in a slot in a world's storage structure - both in a `Meta` in the world's Meta-Set, as well as their current Archetype's storage (`Storage<Identity>`).
:::
