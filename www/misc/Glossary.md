---
title: Glossary
---

# Glossary of Terms
These are terms that are often casually used, explaine a little more in depth.

## Contains
We contextually sometimes say:

- "the Query contains" a set of Entities, and the base `Query` class also exposes these "contents" via a `IEnumerable<Entity>` interface. This is used rarely, however.
- "the World contains", which can refer to both Entites and Archetypes.
- "the Archetype contains", which can refer both to the Entities that share this Archetype, but also the Types that constitute said Archetype.

## Structural Changes

Changes to the layout of Entities - meaning which Components, Links, or Relations they have - define which [Archetype](/docs/Archetype.md) they falls into. 

### Each of these constitutes a structural change:
 - Adding a Component, Link, or Relation
 - Removing a Component, Link, or Relation
 - Despawning an Entity

### The special case of Spawning

- Spawning instantly returns a fully usable Entity builder struct... however, its Identity and Components will only be written to the World later, so it is invisible to Queries and even to `World.GetEntity` and `World.IsAlive` while a ==World Lock== is in effect.

### Reasons for Locking
Structural Changes will invariably cause data to be moved around internally, and sometimes internal data structures need to resize. This may cause individual Entities or even entire memory regions to be moved within their affected Archetype.

Structural changes to the world are deferred while a ==World Lock== is taken out, until _ALL_ locks are disposed. Once that happens, they are all applied in order of submission.

### Batch versus Individual Operations
When removing components, it's often good practice to perform structural changes in bulk through the [Query CRUD](/docs/Queries/CRUD.md) functions where possible. 


::: details :neofox_magnify: BEHIND THE SCENES
When a Structural Change is applied to an Entity, it is moved out of its current Archetype's Component data storage structures and appended to a new Archetype. The "hole" that is now in the original Archetype is plugged by copying the last Entity of that Archetype into the newly vacated spot, ensuring that Memory is always contiguous.

No need to manually remove a component from each entity in a Query, enqueing the change in the Deferred Operations Queue, and having the World execute them all afterwards (and in sub-optimal order, leading to additional, entirely unnecessary data copies).
:::


## World Lock

An `IDisposable` that can be aquired from a World to set it to Deferred Mode, meaning all structural changes are queued and executed only after the last lock has been returned.

All the Query runners use this internally to defer structural changes until the Runner has completed.

::: code-group

```cs [how to use it]
var myWorld = new World();
// using statement will ensure the lock is disposed when it goes out of scope.
using var worldLock = myWorld.Lock;
```

<<< ../../fennecs/Query1.cs#Showcase{10} [fennecs internal usage]
:::