---
title: Basics
---

# Basic Example
Assume we wanted to apply earth's gravity to any object with a weight and a velocity.

```cs
// Declare your own component types, or use any existing type.
using Velocity = System.Numerics.Vector3;
using Weight = float; 

// Create a world. (fyi, World implements IDisposable)
var world = new fennecs.World();

// Spawn an entity into the world with a choice of components.
var entity = world.Spawn().Add<Velocity>();

// Queries are cached, just build them right where you want to use them.
var query = world.Query<Velocity>().Build();

// Run code on all entities in the query. 
query.For(static (ref Velocity v, float dt) => {
    v.Y -= 9.81f * dt;
}, uniform: Time.Delta);
```
