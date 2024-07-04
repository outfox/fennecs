---
title: Expressions
order: 12

---

# Comp Expressions

Sometimes, you have to address the ~~queen's soldiers~~ components as what they are - just ~~playing cards~~ annotated types.

::: tip :neofox_science: The Essence of Components
Comp Expressions are a way to refer to components in a strongly typed way, on a meta-level.

Boxed Components are a way to refer to components in a weakly typed way, with their values, for at-runtime inspection and serialization.
:::

## Strongly Typed Expressions

These are used most commonly to tell a Query or Stream what to filter for. 

::: details FUTURE FEATURE
These will also be used to create certain interactions at runtime, such as building Queries from a Parameter List. (this will come with C# 13)
:::

### `Comp<C>.Plain`
Creates a Comp Expression that represents a "plain" component of the given type, i.e. one that has no relation target. It just holds data. Most components are likely to be plain components.

### `Comp<R>.Matching(Entity relation)`
Creates a Comp Expression that represents a component of the given type that represents a relation to the given Entity.

### `Comp<L>.Matching(L link) where L : class`
Creates a Comp Expression that represents a component of the given type that represents an Object Link to the target.

### `Comp<T>.Matching(Match match)`
Creates a Comp Expression that represents a component of the given type that matches the given Match Expression (Wildcards).
- `Match.Any` matches any component of the given type (includes Plain Components).
- `Match.Entity` matches any component that represents a relation to an Entity.
- `Match.Object` matches any component that represents an Object Link.
- `Match.Target` matches any component that represents a relation to an Object or Entity


## Boxed Components

These are encountered when serializing or inspecting components, and in operations when you can't know the types of all component at compile time.

### `Entity.Components` (IReadonlyList&lt;Component$gt;)
```csharp
using var world = new World();
var entity = world.Spawn();
entity.Add(123);
entity.Add(69.420);
entity.Add(new TypeA());
entity.Add(Link.With("hello world"));

var components = entity.Components;
Assert.Equal(4, components.Length);

List<IStrongBox> expected  = [
    new StrongBox<int>(123), 
    new StrongBox<double>(69.420), 
    new StrongBox<TypeA>(new()), 
    new StrongBox<string>("hello world")
    ];

foreach (var component in components)
{
    var found = expected.Aggregate(
        false, 
        (current, box) => current | box.Value!.Equals(component.Box.Value)
    );
    Assert.True(found);
}
```

Getting all Components from an Entity via `Entity.Components` returns an `IEnumerable<Component>`. This is a collection of boxed components in the `Component.Box` properties, which can be unboxed to their original type with the help of the `Component.Type` property.

### `Component`
Component is a managed struct that contains a boxed value and some metadata about the component, making it easy to reason about it, for example when serializing or debugging.

#### `isRelation` (bool)
Returns whether the component is a relation to another Entity.

#### `isLink` (bool)
Returns whether the component is an ObjectLink. In this case, the `Box.Value` is also the target object.


#### `targetEntity` (Entity)
The Entity target of this Component, if it is a Relation.


#### `Type` (System.Type)
The backing type of this Component. 


#### `Box` (IStrongBox)
The boxed value of this Component. 

::: info :neofox_snug_glare: NOT SURE IF COPY OR REFERENCE
You can't change the value of a Component directly. You have to use the Entity CRUD API to do that. Component contains copies of everything. It also doesn't know its own Entity. (but fortunately, you usually know, because your code just called `Entity.Components`). 

The boxed value is a copy of a Value component, but is the same reference for Shareable Components, as well as for Object Links.

Best practice is to just not modify a Component.
:::
