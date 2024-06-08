---
title: Worlds
order: 10
---

# Worlds

Each World contains Entities and their Components, as well as their structure and Relations.

![World Example: blue circle labeled world filled with fox emojis with many different traits](https://fennecs.tech/img/diagram-world.png)
*"A world, populated by Entities with different traits (Components)"*


## Instantiation
Imagine making a new universe was as easy as saying *"let there be fennecs"* - well, it is!
::: code-group
```csharp [just a plain world]
// fiat fox! (that's latin for "using fennecs;")
var world = new fennecs.World();
```

```csharp [initial capacity]
// sometimes you know a ballpark upper bound of entities to prepare for!
var world = new fennecs.World(initialCapacity: 1_000_000);
```

```csharp [optional initializers]
// specify some strategies and debug settings in the object initializer
var world = new fennecs.World(initialCapacity: 0)
{
    Name = "very smol foxes world",
    GCBehaviour = GCStrategy.InvokeOnWorldCatchup 
        | GCStrategy.CompactStagnantArchetypes 
        | GCStrategy.DisposeEmptyRelationArchetypes,
}
```

### You can have up to a billion Entities in a World.
Realistically **fenn**ecs can help you handle around 1 or 2 million, depending on your Component layouts, at a reasonable performance level for games.

::: details :neofox_cry_loud: I WANT MORE! TRES COMMAS!
Listen, Jeff Jr. - the difference between 1 million and 1 billion is pretty much exactly 1 billion. Can you even begin to fathom how much a billion is? ... *sigh* ... sure, we'll chip in a couple more! 

Easy to remember, too - the limit is now your mom's weight and/or phone number:
`1,073,741,824` 
:::

## What's a World, anyway?

A `fennecs.World` is the root object that contains all your Entities, their Components, Queries, and the data structures that group them. It's the central hub for all things **fenn**ecs in your game or simulation.

::: details :neofox_magnify: BEHIND THE SCENES - Multiple Worlds

Yes, you can have multiple of them. Instantiate to your heart's content!<br/>
You usually only need one World... now go ... ... shoo!

::: details DON'T SHOO ME!
Each World is a separate, isolated universe of Entities and Components, with its own set of Archetypes and Queries.

Entities know their World, but Worlds don't know about each other. They're like parallel dimensions, each with its own set of rules and inhabitants. 

Entities can't move between Worlds *(yet)*, but copying them over by hand is easy enough.  
(with some caveats in the [Random Tidbits](#random-tidbits-for-nerds) section)

Multiple Worlds become useful when you want strict separation, e.g. for a Server World and a Client World on the same machine. They might also be useful if you want to re-use the same Query creation code with different rules. *(relevant for library authors)*

In Worlds where lots of Archetypes or Queries are created and destroyed, it can also be beneficial to have separate worlds to keep the Query overhead of these operations low. *(we're talking many thousands or millions of operations here - don't prematurely optimize, profile your code and then make an informed decision!)*

:::


## Querying your World

To find and work with Entities, you'll use the World's [QueryBuilder](Queries/index.md) creation methods like `Query()`, `Query<C>()` and its variants. These let you express complex queries to match exactly the Entities you need, using powerful [Match Expressions](/docs/Queries/Matching.md).

```csharp
// Find all Entities with a Position and Velocity component
var query = world.Query<Position, Velocity>().Stream();
```


## Spawning Entities ...
Entities spring to life in a World when you `Spawn()` them:

```csharp
var entity = world.Spawn();
```

You can give them Components either one by one...

```csharp
entity.Add<Position>();
entity.Add<Velocity>();
```

...or using the `EntitySpawner` for a bit of extra ✨flair✨:

```csharp
var entities = world.Entity()
    .Add<Position>()
    .Add<Velocity>()
    .Spawn(count: 100); 
```

### ... and completing the Circle of Life! 🌄

```csharp
// ...Living its best Entity life...

// Suddenly - Despawn!
entity.Despawn();
```
*(various ways to despawn Entities are available, see [CRUD](Queries/CRUD.md) for more!*

## I am become Fox, the Disposer of Worlds

When you're done with a World, disposing of it will clean up all its Entities and Components, and free up most of the resources **fenn**ecs was using for it.  Worlds implement `IDisposable`, this is useful in Games and Tests alike:

::: code-group
```csharp [manually (e.g. game code)]
myWorld.Dispose();
```

```csharp [via using statement (e.g. test code)]
using (var world = new World()) // a whole new world!
{
    // do unit tests in the World  
}  // World is disposed at end of scope!
```
::: tip :neofox_nervous: DON'T PANIC
You do not have to dispose worlds unless you want a clean slate. **fenn**ecs (or rather, the .NET Runtime) will clean up after itself when your program exits, and there can actually be an advantage to keeping a World around for the duration of your game or simulation - all your Queries are already compiled, most Archetypes exist, the IdentityPool is pre-allocated, etc., etc.
:::


## Random Tidbits for Nerds

::: details :neofox_magnify: BEHIND THE SCENES - Archetypes

Under the hood, a World stores Entities with identical Component setups together in data structures called ==Archetypes==. This keeps **fenn**ecs fast as a fox, letting it blaze through hundreds of thousands of Entities in a flash.

You usually don't need to think about Archetypes yourself - **fenn**ecs handles them automagically based on the Components you give to each Entity.
:::

::: details :neofox_magnify: BEHIND THE SCENES - IdentityPool
Entities have an internal Id, their Identity, which they receive from - and return to - an internal pool. There, the Identities recycled when Entities despawn.

Identiy stores an internal Generation number that prevents an Entity's successor from being mistaken for the real McCoy.
:::


::: danger RELATIONS :neofox_astronaut: :neofox_astronaut_gun:
*"Wait, it's all Identities? - Always has been!"*

#### Entity-Entity relations currently **don't care or knw about other worlds.** 

- Essentially it means your secondary keys are not unique across world boundaries. This is practically never a problem (we challenge you to even construct a scenario!). Either way, there's around 16 free bits in `Identity` reserved for these and other kinds of shenanigans in the future.

#### Entity-Object links work as expected.
- And yay! This is another one of their main uses, for example a Network Socket, Asset Provider, or Sound System might be a good candidate for an Object Link used by Entities in multiple Worlds.
:::



------------------

### :neofox_pat_up: *"**fenn**ecs make the world go round"*
