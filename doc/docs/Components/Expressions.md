---
title: Expressions
order: 12
outline: [1, 2]
---

# Component Expressions :neofox_magnify:

::: tip :neofox_thumbsup: Meta-Level Component References
Component Expressions let you refer to components in a strongly typed way at a meta-level – perfect for building queries, filtering, and runtime inspection!
:::

## What are Component Expressions?

Component Expressions (`Comp<C>`) are a way to reference components by their type and optional relation target. They're used primarily for:

- Building Queries dynamically
- Filtering Streams
- Runtime inspection and serialization

## Strongly Typed Expressions

### `Comp<C>.Plain`

Creates an expression for a "plain" component – one with no relation target.

```cs
// Reference a plain Position component
var posExpr = Comp<Position>.Plain;

// Use in stream filters
stream.Subset = [Comp<Position>.Plain];
```

### `Comp<R>.Matching(Entity relation)`

Creates an expression for a relation component pointing to a specific entity.

```cs
var parent = world.Spawn();

// Reference the ChildOf relation to 'parent'
var childOfParent = Comp<ChildOf>.Matching(parent);
```

### `Comp<L>.Matching(L link)` 

Creates an expression for an object link to a specific object.

```cs
var gameObject = new GameObject("Player");

// Reference the link to this specific object
var linkExpr = Comp<GameObject>.Matching(gameObject);
```

### `Comp<T>.Matching(Match match)`

Creates an expression with wildcard matching.

```cs
// Match any component of type Health (plain or relation)
var anyHealth = Comp<Health>.Matching(Match.Any);

// Match only entity relations of type Owes
var owesRelation = Comp<Owes>.Matching(Match.Entity);

// Match only object links of type Texture
var textureLinks = Comp<Texture>.Matching(Match.Object);
```

## Match Wildcards

| Match | Description |
|-------|-------------|
| `Match.Any` | Any target (includes plain) |
| `Match.Plain` | No target (plain components only) |
| `Match.Entity` | Entity-to-entity relations |
| `Match.Object` | Object links |
| `Match.Target` | Any relation (Entity or Object, not Plain) |

## Boxed Components :neofox_box:

For runtime inspection and serialization, **fenn**ecs provides boxed component access through `Entity.Components`.

### `Entity.Components`

Returns all components on an entity as `IReadOnlyList<Component>`:

```cs
using var world = new World();
var entity = world.Spawn();
entity.Add(123);
entity.Add(69.420);
entity.Add(new Position { X = 10, Y = 20 });
entity.Add(Link.With("hello world"));

var components = entity.Components;
Console.WriteLine($"Entity has {components.Count} components");

foreach (var component in components)
{
    Console.WriteLine($"  {component.Type.Name}: {component.Box.Value}");
}
```

### The `Component` Struct

`Component` is a managed struct containing metadata about a component:

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `System.Type` | The backing type of the component |
| `Box` | `IStrongBox` | The boxed value |
| `isRelation` | `bool` | Is this a relation to another Entity? |
| `isLink` | `bool` | Is this an Object Link? |
| `targetEntity` | `Entity` | The relation target (if `isRelation`) |

### Usage Example

```cs
foreach (var component in entity.Components)
{
    if (component.isRelation)
    {
        Console.WriteLine($"Relation to {component.targetEntity}");
    }
    else if (component.isLink)
    {
        Console.WriteLine($"Link to {component.Box.Value}");
    }
    else
    {
        Console.WriteLine($"Plain: {component.Type.Name} = {component.Box.Value}");
    }
}
```

::: warning :neofox_owo: Boxed Components are Copies
The `Component.Box.Value` is a **copy** for value type components, but the **same reference** for shareables and object links.

Don't modify boxed components – use the Entity CRUD API (`Add`, `Remove`, `Ref`) instead.
:::

## Quick Reference

| Expression | Creates Reference To |
|------------|---------------------|
| `Comp<C>.Plain` | Plain component (no target) |
| `Comp<C>.Matching(entity)` | Relation to entity |
| `Comp<C>.Matching(obj)` | Object link |
| `Comp<C>.Matching(Match.Any)` | Any matching component |

| Property | Purpose |
|----------|---------|
| `Entity.Components` | All components (boxed) |
| `Component.Type` | Backing type |
| `Component.Box` | Boxed value |
| `Component.isRelation` | Is entity relation? |
| `Component.isLink` | Is object link? |
