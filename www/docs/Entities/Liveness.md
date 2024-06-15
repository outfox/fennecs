---
title: Liveness
layout: doc
order: 2
---


# Liveness
Entities (and their Ids) are recycled by an internal structure that resembles a [Sparse Set](https://www.codeproject.com/Articles/859324/Fast-Implementations-of-Sparse-Sets-in-Cplusplus). 

To this end, Identities track a generation number that is incremented when the Entity's lifetime ends. This means their Ids are always unique, but not necessarily sequential. To the user, this is kept relatively opaque, as Identities are value types. 

You can rely on the Entity's Id to be unique, and whether or not this specific Entity exists in a world is exposed through the `Alive` property.

## `Entity.Alive`
Entities are considered `Alive` if they are spawned in a World. Entities that are despawned are considered dead. Their debug string shows this, too (see the output).

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

## Future Features
We're working on a way to make pending spawns and despawns apparent, as well.