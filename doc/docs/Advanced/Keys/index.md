---
title: Keys
order: 2
outline: [1, 2]
description: 'Secondary Keys in fennecs let component types target an Entity or Object, turning plain components into Relations and Object Links between Entities.'
---
# Secondary Keys

::: tip :neofox_thumbsup: UNLOCK NEW RELATIONSHIPS
Secondary Keys let components reference Entities or Objects, enabling powerful relationship patterns!
:::

![a fennec carrying a golden key](/img/fennec-key.png)

## What are Secondary Keys?

**fenn**ecs allows component types, which act as primary keys, to also reference an additional **secondary key**. This enables rich relationship modeling between entities and objects.

## Quick Reference

| Key Type | Description | Use Case |
|----------|-------------|----------|
| [Plain Component](/docs/Components/index.md) | No secondary key | Standard component data |
| [Relation](Relation.md) | Target is an Entity | Entity-to-Entity relationships |
| [Object Link](Link.md) | Target is an Object | Entity-to-Object associations |

## Key Types

Secondary keys may be:
- **Nothing** - a ***Plain Component*** with no secondary key
- **Entity** - designating the component as a [Relation](Relation.md) to the target Entity
- **Object** - constituting an [Object Link](Link.md) to a reference type
