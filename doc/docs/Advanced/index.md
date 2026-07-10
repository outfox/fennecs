---
title: Advanced
order: 11

head:
  - - meta
    - name: description
      content: Advanced fennecs topics - Aspects, Keys, SIMD, and other sharp tools for big simulations.
---

# Advanced Foxery

::: warning :neofox_glasses: FOR FOXES WHO READ THE MANUAL *TWICE*
Nothing in this section is required reading  –  **fenn**ecs works great without it. But when your simulation grows teeth (and a few hundred thousand Entities), these are the tools you'll be ~~hoarding~~ glad to have in the den.
:::

## In this Section

### [Aspects](Aspects/index.md) *(new in 0.7.0)*
Split a World into multiple contiguous storage universes to fight ==Fragmentation== and keep your hot data cozy and cache-friendly.

### [Keys](Keys/index.md)
Secondary Keys unlock [Relations](Keys/Relation.md) between Entities and [Object Links](Keys/Link.md) to shared data.

### [SIMD](SIMD.md)
Bulk mutations at blazing speeds with `Blit` and friends, courtesy of `System.Intrinsics`.

### [Expressions](Expressions.md)
Meta-level Component references (`Comp<C>`) for dynamic query building, filtering, and runtime inspection.

## Sharp Tools living elsewhere

Some advanced topics live with their families instead of in this section:

- [Stream.Raw](/docs/Streams/Stream.Raw.md)  –  get handed entire memory blocks and do your worst *(best)*.
- [Fox Typing](/docs/Components/FoxTyping.md)  –  **fenn**ecs's delightfully loose take on component typing.
- [Batch Operations](/docs/Queries/CRUD.md#batch-operations)  –  structural changes in bulk, straight from a Query.
