---
title: Bulk CRUD
---

# Bulk Create, Read, Update, Delete
`fennecs.Query` let you Add or Remove components, much like `fennecs.Entity`, to allow easy operation in bulk on all entities matched by the query!


### Example Sneak Peek
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
loadedGuns.Batch()
    .Add<Cooldown>(2.0f); //for our cooldown system
    .Add<RequestProjectileSpawn>(); //for another system
    .Remove<Loaded>(); 
    .Submit();
```

:::

### Adding & Removing Components in Bulk
`Query.Add<C>` adds a Component to each Entity in the Query. This throws if the Query already matches that Component - either ALL entities in the query would already have the Component, or the query would be empty anyway.

`Query.Remove<C>` removes a Component from each Entity in the Query. This throws if the Query doesn't match that Component - NO entities in the query would have the Component on them, anyway.


### Adding & Removing Links in Bulk
(TODO) object links can also be added. (coming beta 1.2)

### Adding & Removing Relations in Bulk
(TODO) entity relations can also be added. (coming beta 1.2)

