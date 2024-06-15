---
title: per-Entity CRUD
order: 2
---

# per-Entity Create, Read, Update, Delete

`fennecs.Entity` is a builder struct that combines its associated `fennecs.World` and `fennecs.Identity` to form an easily usable access pattern which exposes operations to add, remove, and read [Components](/docs/Components/) , [Links](/docs/Components/Link.md) , and [Relations](/docs/Components/Relation.md). You can also conveniently Despawn the Entity.


The component data is accessed and processed in bulk through [Queries](/docs/Queries/), a typical way that ECS provide composable functionality.

## Liveness
Entities are considered `Alive` if they are spawned in a World. Entities that are despawned are considered dead. Their debug string shows this, too.

:::code-group
```csharp [Code]
var entity = world.Spawn();
if (entity.Alive) Console.WriteLine(entity);
entity.Despawn();
if (!entity.Alive) Console.WriteLine(entity);
```
```plaintext [Output]
E-00000001:00001 <fennecs.Identity>
E-00000001:00001 -DEAD-
```
:::

::: warning Deferred Despawn
When despawned inside a Runner, operations are deferred until the end of the scope (or until the last World lock is released). An entity will be considered `Alive` until the World goes through `WorldMode.Catchup` to integrate these structural changes.

When despawning while iterating a `Stream<>` or `Query` directly, the Entity will immediately despawn and the enumerator will be invalidated. Take out a world lock if you want to keep iterating.
```csharp
var worldLock = world.Lock();
foreach (var entity in world) // world is a query
{ 
    if (Random.Shared.NextSingle() >= 0.5f) entity.Despawn();
    if (entity.Alive) Console.WriteLine("Dead Fox Walking!");
}
worldLock.Dispose(); // this will catch up the despawns
```
:::

## `World.Spawn()`
Entities are created through the World's `Spawn()` function, which returns a `fennecs.Entity` builder struct that operates directly on the Entity's ==Identity== in that World.

## `Entity.Despawn()`
Despawns the Entity from its World.

## `Entity.Add<T>(T component)` & <br/> `Entity.Remove<T>(T component)`
Adds or removes a component to the Entity. `T` can be of practically any value or reference type. If `T` is newable (`where T: new()`), a default instance can be created if none is provided as a parameter.

Use of `null` values is discouraged. *(currently disallowed - looking for feedback)*

## `Entity.AddRelation<T>(T component)` & <br/>`Entity.RemoveRelation<T>(T component)`
Adds or removes a relation (and its backing component data) to the Entity.

## `Entity.AddLink<L>(L object)` & <br/>`Entity.RemoveLink<L>(L object)`
Adds or removes an Object Link to the Entity. [Object Links](/docs/Components/Link.md) are objects who, in addition to be added as component data, serve the purpose of Archetype grouping entities.

::: info :neofox_book: USE CASES
Object Links are very powerful because they create a natural division of labor in your runner code by means of creating an [Archetype](/docs/Components/index.md#archetype) for each Link. Unless you have high Entity counts or a definitive multi-threaded use case, you will only get semantic gains from this type of relationship

* grouping Entities by physics worlds, and pumping these worlds in separate threads and copying data from and to each entity there
* grouping Entities by scene hierarchy roots, network connections, or other things that naturally constitute a rarely changing grouping criterion
:::

::: info SUB-OPTIMAL USES
If you only need to a way to make available multiple objects of type `L` to an Entity in a runner, consider adding a component of type `L[]` or `List<L>` to the Entity instead.

If you need a 'shared' component, consider just using a reference type Component instead of a Link (the same object can be used as a component on any number of Entities).
:::


## Caveats
::: warning :neofox_dizzy: BEWARE of ==FRAGMENTATION==
Use Object Links and Relations sparingly to group larger families of Entities. The difference to a reference type Component is that an entity can have any number of Object Links or Relations of the same type. This means that for `n` different linked Objects or target Entities, up to `n!` Archetypes could exist at runtime were you to attach all permutations of them to Entities at runtime.

Naturally, only Archetypes for actual constellations of Links and Relations on Entities will exist, so it is entirely up to your code. Great power, great responsibility.
:::
