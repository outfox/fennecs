---
title: Tags
order: 2
---

# Tag Components

Super useful: Tags are a components that don't store any data. They are often used to mark entities that have a common trait or behavior.

```csharp
public struct Pretty;
public struct Smart;
public struct Cool;

var you = world.Spawn()
            .Add<Pretty>()
            .Add<Smart>()
            .Add<Cool>();

var fennecsUsers = world.Query()
                    .Has<Pretty>()
                    .Has<Smart>()
                    .Has<Cool>()
                    .Compile();

Assert.Contains(you, fennecsUsers);
```	

They need practically no memory and are great to use in Match Expressions, e.g. `Query.Has<MyTag>()` (they're not so great as Stream Types, but they're not harmful either - they just take a slot you might want to use for actual data).