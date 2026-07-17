---
title: Link (Object)
order: 12
outline: [1, 2]
description: 'Object Links in fennecs ECS attach a reference type as both component data and secondary key, grouping Entities by shared objects via Link.With.'
---
# Object Links

::: tip :neofox_thumbsup: BRIDGE ENTITIES TO OBJECTS
Object Links connect Entities to reference types - perfect for game engine nodes, shared resources, and external systems!
:::

## What is an Object Link?

A special type of Relation is the **Link**, which associates a non-entity Object as the `secondary key` in the Type Expression that forms the relationship.

As opposed to Entity-Entity Relations, Links use the **Link's target as the backing data**.

This allows us to group Entities by a non-Entity object, such as a string, a game engine's Node, or even an entire Physics Simulation they need to interact with.

Because the Link's target is the backing data, the Link resolves **bidirectionally** at enumeration time - the Entity that is linked to the object will have full access to the object (not just the "data") - because the object *is the data*.

## Quick Reference

| Operation | Method | Example |
|-----------|--------|--------|
| Add Link | `entity.Add(Link.With(obj))` | `bob.Add(Link.With(chase))` |
| Remove Link | `entity.Remove<T>(obj)` | `bob.Remove<Bank>(chase)` |
| Remove Link (inferred) | `entity.Remove(obj)` | `bob.Remove(chase)` |

## Creating Links

```cs
Bank chase = new("Chase"); // Bank is a class / reference type
Bank targo = new("Targo"); // Bank is a class / reference type

// Components can BE an object and be considered a Relation to that Object
// ... as opposed to just "having a bank: chase"
// bob has two Bank relations (each backed a reference to the object)
bob.Add(Link.With(chase)); // bob banks at chase (Type Bank->chase)
bob.Add(Link.With(targo)); // bob also banks at targo (Type Bank->targo)
```

## Querying Links

```cs
// Specific link, fixed query 
var customersOfChase = world.Query<Bank>(chase).Compile();

// Wildcard link, fixed query
var customersOfAnyBank = world.Query<Bank>(Link.Any).Compile();

// Wildcard target, specific exclusion, fixed query
var customersOfAnyBankExceptChase = world
    .Query<Bank>(Link.Any)
    .Not<Bank>(chase)
    .Compile();

// All Entities, specific exclusion, fixed query
var entitiesExceptCustomersOfChase = world
    .Query()
    .Not<Bank>(chase)
    .Compile();

// All Entities, wildcard exclusion, fixed query
var unbankedEntities = world
    .Query()
    .Not<Bank>(Link.Any)
    .Compile();
        
// Wildcard target, specific exclusion, stream filter
var entitiesExceptCustomersOfChase = world
    .Query<Bank>(Link.Any)
    .Stream() with // do this on-the-fly where needed
    {
        Exclude = [Comp<Bank>.Matching(chase)]
    };
```

## Removing Links

```cs
bob.Remove<Bank>(chase); // bob no longer banks at chase
bob.Remove(chase); // type inference works here, too
```

## Constraints

::: warning :neofox_dizzy: BEWARE of ==FRAGMENTATION==
Use Object Links and Relations sparingly to group larger families of Entities. The difference to a reference type Component is that an entity can have **any number** of Object Links or Relations of the same type. 

This means that for `n` different linked Objects or target Entities, up to `n!` Archetypes could exist at runtime were you to attach all permutations of them to Entities.

Naturally, only Archetypes for actual constellations of Links and Relations on Entities will exist, so it is entirely up to your code. Great power, great responsibility.
:::
