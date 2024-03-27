---
title: Per-Query CRUD
---

# Bulk Create, Read, Update, Delete
`fennecs.Query` let you Add or Remove components, much like `fennecs.Entity`, to allow easy operation in bulk on all entities matched by the query!


### TLDR; (sneak peek)
::: code-group
```cs [Damage System]
// Query for all entities with Health that also took Damage
var whoTookDamage = world.Query<Health, Damage, Identity>().Build();

// Do something for each Entity in Query
whoTookDamage.For((ref Health health, ref Damage damage, ref Identity id) => 
{
    health.hp -= damage.amount;
    if (health.hp < 0) world.On(id).Add<Exploding>();
});

//All damage values were accounted for! Bulk remove the values.
whoTookDamage.Remove<Damage>();
```

```cs [Death Explosion System]
// Query for all Entities that are about to Explode (and have a Position)
var whosExplodingWhere = world.Query<Position>().Has<Exploding>().Build();

// Do something for each Entity in Query
whosExplodingWhere.For((ref Position position) => Game.SpawnExplosion(position));

// Despawn all Entities matched by Query
whosExplodingWhere.Despawn();
```

```cs [Multiple Bulk Operations]
var loadedGuns = world.Query()
    .Has<Gun>()
    .Not<Loaded>().Not<Cooldown>().Not<RequestProjectileSpawn>()
    .Build();

// multiple bulk operations need to be batched, because
// entities wouldn't match our query after each change
// (there's a way to allow this, see in the documentation)
loadedGuns.Batch()
    .Add<Cooldown>(2.0f); //for our cooldown system
    .Add<RequestProjectileSpaw7n>(); //for another system
    .Remove<Loaded>(); 
    .Submit();
```

:::

## Adding & Removing Components in Bulk
`Query.Add<C>` adds a Component to each Entity in the Query. This throws if the Query already matches that Component - either ALL entities in the query would already have the Component, or the query would be empty anyway.

`Query.Remove<C>` removes a Component from each Entity in the Query. This throws if the Query doesn't match that Component - NO entities in the query would have the Component on them, anyway.


## Adding & Removing Links in Bulk
(TODO) object links can also be added. (coming beta 1.2)

## Adding & Removing Relations in Bulk
(TODO) entity relations can also be added. (coming beta 1.2)


## Batching Operations
Structural changes to all the Entities in a Query often mean that the Query no longer contains these entities. For this purpose, the `Query.Batch(...)` method and its overloads exist.

They return a `Batch` IDisposable builder pattern that allows you to queue up multiple structural changes.

Call `Submit()` on the `Batch` to defer or immediately execute all the Operations (depending on whether the World is locked or not).

Once you `Submit()` a `Batch`, you pass ownership and responsibility to `Dispose()` it to the World. You only need to dispose the Batch if you decide not to submit it. *(not a realistic use case)*

## Batch Conflicts
Batch Operations can also be used to overwrite and add or remove components at the same time. This means some semantic conflicts may occur, typically:
* `Remove<T>` called on a Batch for a Query where not every Entity has that component
* `Add<T>` called on a Batch for a Query where some or all Entities already have that component.

These could be used to great effect, e.g. to set a new Component value for all Entities in the Query at once!

`Batch.AddConflict` and `Batch.RemoveConflict` are enums that can be passed to `Query.Batch(AddConflict, RemoveConflict)` to specify what the `Batch` behaviour for these conflict types should be:

### Additions
`Batch.AddConflict.Disallow = default`  
Throw an exception if attempting to add a component that is not explicitly excluded from the query.

`Batch.AddConflict.Skip`  
Skip over each Archetype (group of Entities) that already has the Component, not adding or changing the existing one.

`Batch.AddConflict.Skip`  
Skip each Archetype (group of Entities) that already has the Component, not adding or changing the existing one. CAUTION: This will affect all operations of the batch on that archetype, including removals.

`Batch.AddConflict.Preserve`  
Preserve the Values of already present Components, and adds the new ones where not present. *(currently not implemented)*

`Batch.AddConflict.Replace`  
Replaces any existing components in addition to adding the new ones where not present.

### Removals
`Batch.RemoveConflict.Disallow = default`  
Throw if attempting to remove a component unless it is expressly included in the query (and thus present on all entities.)

`Batch.RemoveConflict.Allow`  
Allow operating on Archetypes where the Component to be removed is not present. Removal operations are Idempotent on these archetypes, i.e. they don't change them (on their own) and have a near-zero cost.
