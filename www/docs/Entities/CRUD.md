---
title: per-Entity CRUD
order: 4
outline: [1, 2]
---

# Create, Read, Update, Delete

`fennecs.Entity` is a builder struct that combines its associated `fennecs.World` and `fennecs.Identity` to form an easily usable access pattern which exposes operations to add, remove, and read [Components](/docs/Components/) , [Links](/docs/Components/Link.md) , and [Relations](/docs/Components/Relation.md). You can also conveniently Despawn the Entity.


The component data is accessed and processed in bulk through [Queries](/docs/Queries/), a typical way that ECS provide composable functionality.


## Interface `IAddRemoveComponent`

The `IAddRemoveComponent<SELF>` interface provides methods for adding and removing components from entities or sets of entities. It follows a fluent pattern, allowing chained method calls.

### :neofox_cute_reach: Adding Components :neofox_hug_haj_heart:

- `Add<C>()`: Adds a default, plain newable component of type `C` to the entity/entities.
- `Add<C>(C value)`: Adds a plain component with the specified value of type `C` to the entity/entities.
- `Add<T>(Entity relation)`: Adds a newable relation component backed by a default value of type `T` to the entity/entities.
- `Add<R>(R value, Entity relation)`: Adds a relation component backed by the specified value of type `R` to the entity/entities.
- `Add<L>(Link<L> link)`: Adds an object link component with an object of type `L` to the entity/entities.

### :neofox_hug_haj: Removing Components :neofox_floof_sad_reach:

- `Remove<C>()`: Removes a plain component of type `C` from the entity/entities.
- `Remove<R>(Entity relation)`: Removes a relation component of type `R` with the specified relation from the entity/entities.
- `Remove<L>(L linkedObject)`: Removes an object link component with the specified linked object from the entity/entities.
- `Remove<L>(Link<L> link)`: Removes an object link component with the specified link from the entity/entities.

All methods return the interface itself (`SELF`), allowing for fluent method chaining.

Note: The type parameters `C`, `T`, `R`, and `L` have constraints to ensure they are non-null and/or class types.


## Interface `IHasComponent`

The `IHasComponent` interface provides methods for checking the presence of components on an entity or set of entities. 

### Has Component :neofox_verified:

- `Has<C>()`: Checks if the entity/entities has a plain component of type `C`.
- `Has<R>(Entity relation)`: Checks if the entity/entities has a relation component of type `R` with the specified relation.
- `Has<L>(L linkedObject)`: Checks if the entity/entities has an object link component with the specified linked object.
- `Has<L>(Link<L> link)`: Checks if the entity/entities has an object link component with the specified link.

All `Has` methods return a boolean value indicating whether the entity/entities has the specified component. Therefore, they can only be at the end of a chain.

Note: The type parameters `C`, `R`, and `L` have constraints to ensure they are non-null and/or class types.


## `Entity.Despawn` :neofox_x_x:
- `Entity.Despawn()`: Despawns the Entity from its World.



## Caveats
::: warning :neofox_dizzy: BEWARE of ==FRAGMENTATION==
Use Object Links and Relations sparingly to group larger families of Entities. The difference to a reference type Component is that an entity can have any number of Object Links or Relations of the same type. This means that for `n` different linked Objects or target Entities, up to `n!` Archetypes could exist at runtime were you to attach all permutations of them to Entities at runtime.

Naturally, only Archetypes for actual constellations of Links and Relations on Entities will exist, so it is entirely up to your code. Great power, great responsibility.
:::
