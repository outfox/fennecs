---
title: Bulk CRUD
order: 3
---

# Bulk Create, Read, Update, Delete
OMG! `fennecs.Query` lets you Add or Remove components *(and even Despawn!*), much like an individual `fennecs.Entity`!

It's an easy, powerful way to operate in bulk on all entities matched by the query!


### TLDR; (sneak peek)
::: code-group
```cs [Damage System]
// Query for all entities with Health that also took Damage
var whoTookDamage = world.Query<Health, Damage, Identity>().Stream();

// Do something for each Entity in Query
whoTookDamage.For((Entity entity, ref Health health, ref Damage damage) => 
{
    health.hp -= damage.amount;
    if (health.hp < 0) entity.Add<Exploding>();
});

// 👇 All damage values were accounted for! Bulk remove the values.
whoTookDamage.Remove<Damage>();
```

```cs [Death Explosion System]
// Query for all Entities that are about to Explode (and have a Position)
var whosExplodingWhere = world.Query<Position>().Has<Exploding>().Stream();

// Do something for each Entity in Query
whosExplodingWhere.For((ref Position position) => Game.SpawnExplosion(position));

// 👇 Despawn all Entities matched by Query
whosExplodingWhere.Despawn();
```

```cs [Multiple Bulk Operations]
var loadedGuns = world.Query()
    .Has<Gun>()
    .Not<Loaded>().Not<Cooldown>().Not<RequestProjectileSpawn>()
    .Stream();

// ⚠️ multiple bulk operations may require batching, because
// entities wouldn't necessarily match our query after each change
// and thus "escape" before subsequent operations would be applied.
// (there's a way to customize this, see in the documentation)
loadedGuns.Batch()
    .Add<Cooldown>(2.0f); //for our cooldown system
    .Add<RequestProjectileSpawn>(); //for another system
    .Remove<Loaded>(); 
    .Submit();
```

:::

## Adding & Removing Components in Bulk
`Query.Add<C>` adds a Component to each Entity in the Query. This throws if the Query already matches that Component - either ALL entities in the Query would already have the Component, or the query would be empty anyway.

`Query.Remove<C>` removes a Component from each Entity in the Query. This throws if the Query doesn't match that Component - NO entities in the Query would have the Component on them, anyway.


## Adding & Removing Links in Bulk
(TODO) object links can also be added. (coming 0.1.2-beta)

## Adding & Removing Relations in Bulk
(TODO) entity relations can also be added. (coming 0.1.2-beta)


## Batch Operations
Structural changes to all the Entities in a Query often mean that the Query no longer contains these entities. For this purpose, the `Query.Batch(...)` method and its overloads exist.

They return a `Batch` IDisposable builder pattern that allows you to queue up multiple structural changes.

Call `Submit()` on the `Batch` to defer or immediately execute all the Operations (depending on whether the World is locked or not).

Once you `Submit()` a `Batch`, you pass ownership and responsibility to `Dispose()` it to the World. You only need to dispose the Batch if you decide not to submit it. *(not a realistic use case)*

### Handling Semantic Conflicts
Batch Operations can be requested for queries that do not or will not contain all affected Entities. They can be used to overwrite and add or remove components at the same time. This means some semantic conflicts may occur, typically:
* `Remove<T>` called on a Batch for a Query where not every Entity has that component
* `Add<T>` called on a Batch for a Query where some or all Entities already have that component.

These could be used to great effect, e.g. to set a new Component value for all Entities in the Query at once!

`Batch.AddConflict` and `Batch.RemoveConflict` are enums that can be passed to `Query.Batch(AddConflict, RemoveConflict)` to specify what the `Batch` behaviour for these conflict types should be:

### Batch.AddConflict
`Disallow = default`  
Throw an exception if attempting to add a component that is not explicitly excluded from the query.

`Preserve`  
Preserve the Values of already present Components, and adds the new ones where not present.

`Replace`  
Replaces any existing components in addition to adding the new ones where not present.

### Batch.RemoveConflict
`Disallow = default`  
Throw if attempting to remove a component unless it is expressly included in the Query (and thus present on all entities.)

`Allow`  
Allow operating on Archetypes where the Component to be removed is not present. Removal operations are Idempotent on these archetypes, i.e. they don't change them (on their own) and have a near-zero cost.
