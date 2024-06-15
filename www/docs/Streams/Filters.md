---
title: Filtering
layout: doc
outline: [2, 3]
order: 8
---

# Filters

Sometimes, a dynamic on-the-fly filter is needed to process only a subset of Entities; this enables us to quickly adjust our ECS logic to do different subsets of work without requiring a growing amount of queries to be defined.

To do so, we simply clone our lightweight `Stream<>` view and change some fields.

## Creating a Stream Filter
Each `Stream<>` has two filter fields, `Subset` and `Exclude`, which are used to filter the Entities that are processed by the Stream.

::: tip :neofox_floof_mug: Can I interest you in a Grape-Colored Example
There's an appetizer on this topic! Check out [Thanos](/cookbook/appetizers/Thanos.md) for an, ahem, "practical" example of using filters.
:::

You can specify them using the `with` syntax to create a new Stream with the filters applied. This gives you a new view that applies the filter for all its operations. It doesn't mutate the original Stream, nor the underlying Query.

```csharp
var stream = world.Stream<Position, Velocity>();

var filteredStream = stream with 
{
    Subset = [ Component.PlainComponent<Alive>() ], // collection initializer
    Exclude = [ Link.With(TheOneRing) ] // (the collections are immutable sets)
};
```

## Subset Clause
This works much like an additional `Has<>` clause.

> `includes only` Entities that have the given component or relation. Multiple `Has` statements can be compared to a logical `A AND B AND C`.

## Exclude Clause
This works much like an additional `Not<>` clause.

> `excludes` any Entities that have the given component. Multiple `Not` statements can be compared to a logical `NOT (A OR B OR C)`, aka. `(NOT A) AND (NOT B) AND (NOT C)`.

## Combining Filters
Subset and Exclude are `ImmutableSets`, so they can be combined to build or merge some filters together when specifying a new Stream using the `with` keyword.

```csharp
var stream = world.Stream<Position, Velocity>();

var filteredStream = stream with 
{
    Subset = otherFilter.Subset
        .Add(Component.PlainComponent<Alive>())
        .Remove(Component.PlainComponent<Dead>()),
    Exclude = otherFilter.Subset.Union([Component.AnyRelation<Owes>()]),
};
```

## Future Features
The `Component` utility class to create the necessary filter expressions is likely to have its API reviewed and tightened, to make the syntax more readable and easier to use. It might get unified into a Mask-like system as used internally to power QueryBuilders.