---
title: Quickstart
---

```cs
var world = new fennecs.World();
var entity = world.Spawn().Add<Vector3>();
var query = world.Query<Vector3>().Build();

query.Job(static (ref Vector3 velocity, float dt) => {
    velocity.Y -= 9.81f * dt;
}, uniform: Time.Delta);
```
