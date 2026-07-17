---
title: Relation (Entity)
order: 1
outline: [1, 2]
description: 'Entity-Entity Relations in fennecs are components with an Entity secondary key - added via Add<T>(data, target) and queried with wildcards like Entity.Any.'
---
# :neofox_heart: Relations between Entities

::: tip :neofox_thumbsup: CONNECT YOUR ENTITIES
Relations link Entities together, perfect for hierarchies, social connections, and game mechanics!
:::

## What is a Relation?

When a component type references another Entity as its secondary key, it is called a ***Relation***.

**fenn**ecs allows component attachment to entities to reference an additional `Entity`, the relation target. This relation becomes part of the Key that is used to group Entities into an Archetype.

The relation is said to be ***backed by its Component type***, and any data can be stored in it. The relation is a `secondary key` on top of the normal matching logic that **fenn**ecs performs.

## Quick Reference

| Operation | Method | Example |
|-----------|--------|--------|
| Add Relation | `entity.Add<T>(data, target)` | `bob.Add<Owes>(new(10M), alice)` |
| Remove Relation | `entity.Remove<T>(target)` | `bob.Remove<Owes>(alice)` |
| Check Relation | `entity.Has<T>(target)` | `bob.Has<Owes>(eve)` |
| Get Relation Data | `entity.Ref<T>(target)` | `bob.Ref<Owes>(eve)` |

::: info 🦋 IS THIS BI-ERASURE?
Relations are **unidirectional**, so the Target doesn't "know" it is being related to.

But despawning the Target of one or more Relations <u>will remove</u> these Relation Components from any Entities that reference it. See the ending of the [Kill Bill](/cookbook/appetizers/KillBill.md) appetizer for a demonstration.
:::

Relations can be backed by any type (reference or value types). The backing data is only shared if the component is a [shared component](/docs/Components/Shareables.md) itself.

The target of a relation must be [Alive](/docs/Entities/Liveness.md).

## Creating & Removing Relations

Any Component type can back a Relation.

```cs
record struct Owes(decimal Amount);

Entity bob, alice, eve;
```

::: code-group

```cs [Adding a Relation]
bob.Add<Owes>(new(10M), alice); // he owes alice $10 (Relation Owes->alice)
bob.Add<Owes>(new(23M), eve); // and owes eve $23 (Relation Owes->eve)
```

```cs [Removing a Relation]
bob.Remove<Owes>(alice); // bob no longer owes alice
```

```cs [Modifying Backing Data]
if (!bob.Has<Owes>(eve)) { // the relation must be present to access it,
    bob.Add<Owes>(eve);    // exactly like any other component
}

bob.Ref<Owes>(eve).Amount += 7M;
```
:::

## Querying Relations

Relations can be queried like any other component. The `Query` method can be used to find all entities that have a relation to a specific target.

We can also query for all entities that have a relation to **any** target. Here's a variety of ways, ordered from most common to least common / performant:

```cs
// Specific target, fixed query 
var entitiesOwingEve = world.Query<Owes>(eve).Compile();

// Wildcard target, fixed query
var entitiesOwingAnyone = world.Query<Owes>(Entity.Any).Compile();

// Wildcard target, specific exclusion, fixed query
var entitiesOwingAnyoneExceptEve = world
    .Query<Owes>(Entity.Any)
    .Not<Owes>(eve)
    .Compile();

// All Entities, specific exclusion, fixed query
var entitiesExceptAnyOwingEve = world
    .Query()
    .Not<Owes>(eve)
    .Compile();

// Any target, wildcard exclusion, fixed query
var entitiesThatAreDebtFree = world
    .Query()
    .Not<Owes>(Entity.Any)
    .Compile();

// Wildcard target, specific exclusion, stream filter
var entitiesOwingAnyoneExceptEve = world
    .Query<Owes>(Entity.Any)
    .Stream() with // do this on-the-fly where needed
    {
        Exclude = [Comp<Owes>.Matching(eve)]
    };
```
::: danger :neofox_googly_reverse: DUDE, where's my ENTITY?
To query for Relations, you must either specify a concrete target Entity or use a wildcard:
- `Entity.Any` - matches any Entity relation
- `Match.Target` - matches Links and Relations
- `Match.Any` - matches anything, including plain components

**The following won't match Relations:**
- ~~`world.Query<Owes>().Compile();`~~
- ~~`world.Query<Owes>(Match.Plain).Compile();`~~
- ~~`world.Query<Owes>(Match.Object).Compile();`~~
:::


## Multi-Enumeration (Cross Join)

When a wildcard like `Entity.Any` is used as the Match expression, iterating a `Stream` view of the Query will yield **multiple instances** of the same entity if it has multiple relations.

This is used to great effect in the [3-Body Problem](/cookbook/staples/3Body.md) staple.

::: code-group

```cs [Code Example]
var world = new World();
var debtors = world.Query<Owes>(Entity.Any).Stream();

var tom = world.Spawn();
var eve = world.Spawn();
var bob = world.Spawn(); //E-00000003:00001

bob.Add<Owes>(new(10M), eve);
bob.Add<Owes>(new(23M), tom);

// We use IEnumerable here, but it's the same for Stream.For/Job/Raw
foreach (var (Entity entity, Owes owes) in debtors) 
{
    Console.WriteLine($"{entity} owes {owes.Amount} to someone!");
}
```

```plaintext [Output]
E-00000003:00001 owes 10 to someone!
E-00000003:00001 owes 23 to someone!
```
:::

## Back Links

::: tip :neofox_what: Wait, I owe money to "someone" ?!?
If it's important to know the target of a relation as it is being enumerated (bi-directional relation), you can add an `Entity` field in the backing component and store it there. This is a loosely coupled backlink.

If it's only the Entity and Relation you care about, you can just make some records:

```cs
record struct Child(Entity ValueAndTarget);
record struct Parent(Entity ValueAndTarget);
record struct Uncle(Entity ValueAndTarget);

var me = world.Spawn();
var monkey = world.Spawn().Add<Uncle>(new(me), me); //this is how memes are born
```
:::

## Constraints

::: warning :neofox_dizzy: BEWARE of ==FRAGMENTATION==
Use Object Links and Relations sparingly to group larger families of Entities. The difference to a reference type Component is that an entity can have **any number** of Object Links or Relations of the same type. 

This means that for `n` different linked Objects or target Entities, up to `n!` Archetypes could exist at runtime were you to attach all permutations of them to Entities.

Naturally, only Archetypes for actual constellations of Links and Relations on Entities will exist, so it is entirely up to your code. Great power, great responsibility.
:::
