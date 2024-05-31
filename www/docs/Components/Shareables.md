---
title: Shareables
order: 2
---

# Shareable Components

In **fenn**ecs, components are typically value types unique to each entity. However, you can easily share the same instance of a component among multiple entities using reference types.

This is especially useful for heavyweight objects that are expensive to create or update, such as large data structures.

![two fennecs happily holding a huge cardboard box together](https://fennecs.tech/img/fennec-shareable.png)


::: info :neofox_thumbsup: SHARING MADE SIMPLE
To share a component, define it as a [reference type](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/reference-types) (e.g., a class) and add the same instance to multiple entities. Each entity will hold a reference to the shared component instance.
:::

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

```csharp
var query = world.Query<SharedData>().Compile();

query.For((ref SharedData data) =>
{
    data.Value++; // increments value once for each entity in query!
});
```

## Considerations

1. Shared components can positively impact performance by reducing memory usage and allowing trivial updates of shared state.

2. However, accessing shared components involves an indirection. The data on the heap isn't acked as tightly in memory. Iterating over many entities and components can be slower.

3. Fortunately, Queries that don't include reference Components in their Stream Types will not suffer from this indirection at all!

4. Shared components introduce some weak coupling between entities. Be cautious when modifying shared instances.

5. Be mindful of the lifecycle of shared components to avoid memory leaks.

## Conclusion

Shareable components in **fenn**ecs allow efficient sharing of state and updates among entities. Use them judiciously to balance convenience and performance.