---
title: Links
order: 11
---
# Object Links

A special type of Relation is the Link, which associates a non-entity Object as the `secondary key` in the Type Expression that forms the relationship.

As opposed to Entity-Entity Relations, Links use the Link's target as the backing data.

This allows us to group Entities by a non-Entity object, such as a string, a game engine's Node, or even an entire Physics Simulation they need to interact with.

Because the Link's target is the backing data, the Link resolves **bidirectionally** at enumeration time - the Entity that is linked to the object will have full access to the object (not just the "data" instead) - because the object *is the data.*

## Creating & Removing Links

::: code-group
```csharp [Object Link Relation]
Bank chase = new("Chase"); // Bank is a class / reference
Bank targo = new("Targo"); // Bank is a class / reference

// Components can BE an object and be considered a Relation to that Object
// ... as opposed to just "having a bank: chase"
// bob has two Bank relations (each backed a reference to the object)
bob.Add(Link.With(chase)); // bob banks at chase (Type Bank->chase)
bob.Add(Link.With(targo)); // bob also banks at targo (Type Bank->targo)
```
:::

## Querying Links

```csharp  
// Specific link, fixed query 
var customersOfChase = world.Query<Bank>(chase).Compile();

// Wildcard link, fixed query
var customersOfAnyBank = world.Query<Bank>(Link.Any).Compile();

// Wildcard target, specific exclusion, fixed query
var customersOfAnyBankExceptChase = world
    .Query<Owes>(Link.Any)
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
var entitiseExceptCustomersOfChase = world
    .Query<Bank>(Link.Any)
    .Stream() with // do this on-the-fly where needed
    {
        Exclude = [Component.SpecificLink<Bank>(chase)]
    };
```

## Removing Links    
```csharp
bob.Remove<Bank>(chase); // bob no longer banks at chase
bob.Remove(chase); // type inference works here, too
```
