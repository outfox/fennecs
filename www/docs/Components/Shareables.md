---
title: Shareables
order: 10
---

# Shareable Components

One word: `reference types`. No, wait...

You can easily share the same instance of a component among multiple entities using **reference types**. This allows you to efficiently share state and updates across entities.

![two fennecs happily holding a huge cardboard box together](https://fennecs.tech/img/fennecs-shareable.png)

This is especially useful for heavyweight objects that are expensive to create or update, such as large data structures.


::: info :neofox_thumbsup: SHARING MADE SIMPLE
To share a component, devlare it as a [reference type](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/reference-types) (e.g., a class or record, but not struct) and add the same instance to multiple entities. Each entity will hold a reference to the same instance of the component.
:::

You can, of course, still add reference type components only to a single entity, and not share them at all.

## Sharing Components

```csharp
public class SharedData
{
    public int Value;
}

var sharedData = new SharedData { Value = 42 };

var entity1 = world.Spawn().Add(sharedData);
var entity2 = world.Spawn().Add(sharedData);
```

Both `entity1` and `entity2` now reference the same `sharedData` instance. Modifying `sharedData` affects both entities.

## Querying Shared Components

Query shared components using the same syntax as regular components:

::: code-group
```csharp [component setup]
record SharedData(int Value) // a mutable record, could also be a class
{
    public int Value { get; set; } = Value;
}
```

```csharp [modifying / iterating]
using var world = new World();
var stream = world.Query<SharedData>().Stream();

var sharedData = new SharedData(42); // shared instance
world.Entity().Add(sharedData).Spawn(5); // add it to 5 fresh entities

stream.For((ref SharedData data) =>
{
    data.Value++; // increments value once for each entity in query!
    Console.WriteLine(data.ToString());
});

sharedData.Value++; // increment outside of runner
Console.WriteLine();

stream.For((ref SharedData data) =>
{
    Console.WriteLine(data.ToString());
});
```

```csharp [output]
SharedData { Value = 43 }
SharedData { Value = 44 }
SharedData { Value = 45 }
SharedData { Value = 46 }
SharedData { Value = 47 }

SharedData { Value = 48 }
SharedData { Value = 48 }
SharedData { Value = 48 }
SharedData { Value = 48 }
SharedData { Value = 48 }
```
:::

## Considerations

1. Shared components can positively impact performance by reducing memory usage and allowing trivial updates of shared state.

2. However, accessing shared components involves an indirection. The data on the heap isn't acked as tightly in memory. Iterating over many entities and components can be slower.

3. Fortunately, Queries that don't include reference Components in their Stream Types will not suffer from this indirection at all!

4. Shared components introduce some weak coupling between entities. Be cautious when modifying shared instances.

5. Be mindful of the lifecycle of shared components to avoid memory leaks.

## Conclusion

Shareable components in **fenn**ecs allow efficient sharing of state and updates among entities. Use them judiciously to balance convenience and performance.