---
title: Release Notes
order: 2
outline: [2,2]

head:
  - - meta
    - name: title
      content: fennecs — Release Notes
  - - meta
    - property: og:description
      content: Changelog for fennecs, the tiny, fast, modern C# Entity-Component System for games and simulations!
  - - meta
    - property: og:type
      content: website
  - - meta
    - property: og:url
      content: https://fennecs.tech/misc/Changelog.html
  - - meta
    - property: og:title
      content: fennecs — Release Notes
  - - meta
    - name: twitter:title
      content: fennecs — Release Notes
  - - meta
    - property: og:image
      content: https://fennecs.tech/img/fennecs-changelog-panel.png
  - - meta
    - property: twitter:image
      content: https://fennecs.tech/img/fennecs-changelog-panel.png
  - - meta
    - name: twitter:image
      content: https://fennecs.tech/img/fennecs-changelog-panel.png
  - - meta
    - name: twitter:description
      content: Changelog for fennecs, the tiny, fast, modern C# Entity-Component System for games and simulations!
---

![a stylized fox shattering red and green polygons surrounded by source code](https://fennecs.tech/img/fennec-changelog.png)

# Release Notes
Here, there be ~~dragons~~ more foxes. *What did you expect?*

> [!CAUTION] BETA NOTICE
> **fenn**ecs will remain in Beta until version 1.0.0, which is expected in Q4 2024. Breaking API changes as well as bugs are likely to occur without warning in these beta builds. 
> You are nonetheless encouraged to try **fenn**ecs out, play around and experiment with the package freely; our resident foxes aim to keep it it as useful and stable as possible! Please report issues and feedback on the [GitHub Issues](https://github.com/outfox/fennecs/issues) board.

## Upcoming Changes
### soon(tm)
- `Match.Object` becomes internal / deprecated, use `Link.Any` instead.
- `Stream` (a Stream View without any type parameters) will be added (so filtering without a component list feels less awkward)
- `Has(params Comp[])` will be added to `QueryBuilders` to check for multiple components at once. (as well as `Any(params Comp[])`and `Not(params Comp[])`). These will be much more performant and low-allocation starting with .NET 9.0, and will use `Span<Comp>` in the future.
- Breaking for `HasVirtual`, `GetVirtual`, adding `Match` expression support. (breaking means that the old methods will currently match Any, but the new versions will match Plain by default)
...


## Version 0.5.10-beta
- Added `bool Entity.HasVirtual(object)` extension method to `fennecs.reflection`
- Fixed [Issue #17](https://github.com/outfox/fennecs/issues/17) Entities that have self-referencing relations on themselves can now be despawned and bulk-despawned without crashing / potentially undefined behaviour.


## Version 0.5.9-beta
- Added new namespace for some use cases using reflection: `fennecs.reflection`
- Added Extension methods for `Entity`: 
```cs
namespace fennecs.reflection;

/// <summary>
/// Extension Methods that use some sort of Reflection under the hood.
/// </summary>
/// <summary>
/// These are generally against fennecs design principles, but they do have their use cases, for instance when you have to work with contravariant and covariant component types, such as Lists.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Experimental method to add a specific Component identified via RTTI 
    /// (dynamically retrieved at runtime).
    /// This helps with contravariant and covariant component types, such as Lists.
    /// Only this call uses the dynamic logic, the component itself is as any normal 
    /// Component type.
    /// </summary>
    /// <remarks>
    /// This will attempt to create a component type of exactly the object's 
    /// <see cref="object.GetType"/> returned <c>System.Type</c>.
    /// Note that <c>QueryBuilders</c> will need to use the specific type to 
    /// match the Component! (e.g. <c>Query<List<int>></c>)
    /// </remarks>
    public static Entity AddVirtual(this Entity entity, object value, Match match = default);


    /// <summary>
    /// Returns all Components on the Entity that are 
    /// <see cref="Type.IsAssignableTo"/> to the Type Parameter <c>T</c>,
    /// statically cast to this specific type.
    /// </summary>
    /// <remarks>
    /// The array is empty if there are no matching components.
    /// </remarks>
    public static T[] GetVirtual<T>(this Entity entity);
}
```


## Version 0.5.8-beta
- `Component` factory class has most of its members deprecated. It is now a storage for a Boxed Component. ([updated documentation](/docs/Components/Expressions.md))
- `Comp<T>` is a new factory class for Component Expressions. ([updated documentation](/docs/Components/Expressions.md))
- get (read) a specific component using `entity.Get<T>(Match match)`, e.g. `entity.Get<MyLinkType>(Link.Any)` to get all the Links

### Upgrading
::: code-group
```csharp  [old api] 🕸️
var thanosStream = population.Stream<Alive>() with
{
    Subset = [Component.PlainComponent<Unlucky>()],    
    Exclude = [Component.PlainComponent<Lucky>()],
};
```
```csharp [new api] ✨
var thanosStream = population.Stream<Alive>() with
{
    Subset = [Comp<Unlucky>.Plain],    
    Exclude = [Comp<Lucky>.Plain],
};
```
:::

## Version 0.5.7-beta
- `bugfix` - Stream Filters (Subset/Exclude) now affect the `Count` property of the Stream.
- `bugfix` - `Stream<>.Despawn` respects current filters instead of despawning the entire underyling Query
- reinstated the Thanos appetizer's functionality! OH SNAP!

## Version 0.5.6-beta
- `Link.Any` is a Match Target that can be used to match any Link target in a Query. It's value-identical to `Match.Object`, but makes the code more readable and reads in line with `Entity.Any`. 
- lots of documentation updates and fixes

## Version 0.5.5-beta
- `/www/misc/Changelog.md` added 🦊
- `IBatch` renamed to `IBatchBegin`, since it is not the "Batch" itself, just the ability to create (begin) batches.
-  `IBatchBegin` now has all the overloads with AddConflict and RemoveConflict parameters formerly only available in `Query`, and thus are now available in `Stream<>`.
```csharp
public interface IBatchBegin
{
    public Batch Batch(Batch.AddConflict add, Batch.RemoveConflict remove);
    public Batch Batch();   
    public Batch Batch(Batch.AddConflict add);
    public Batch Batch(Batch.RemoveConflict remove);    
}
```
- submission must still be done by calling `Batch.Submit()`, which is not on this Interface.
- `World.GCBehaviour` is now `init` only.

### Upgrade Steps
* You no longer need to call `Stream<>.Query.Batch(...)`, just use `Stream<>.Batch(...)` to access the overloads with `AddConflict` and `RemoveConflict` parameters. 

### Breaking Changes
* `Entity.Ref<C>` no longer automatically adds the component to the Entity if it does not exist. The syntax was too muddled, and certain degenerate types, such as `string`, could not match any overloads and could no longer be used with `Ref<C>`.

### New Features
* `Stream<>` can be cloned with Subset and Exclude filters:
```csharp
 var filtered = myStream with 
    { 
        Subset = [Component.PlainComponent<ComponentA>()], 
        Exclude = [Component.AnyAny<ComponentB>(), Component.SpecificEntity<ComponentC>(notYou)] 
    };
```

- a new `Component` helper class exists to express strictly typed Match expressions, for these and other filters
```csharp
public readonly record struct Component
{
    public static Component AnyAny<T>();
    public static Component AnyRelation<T>();
    public static Component AnyEntity<T>();
    public static Component AnyObject<T>();
    public static Component PlainComponent<T>();
    public static Component SpecificEntity<T>(Entity target);
    public static Component SpecificLink<T>(T target) where T : class;
} 
```

### Performance Improvements
* Several accidental allocation leaks plugged.

### Other Changes
* Temporary Restriction: Cannot run Jobs on Queries with Wildcards. (an exception will be thrown)
* `default(Match)` is `Match.Plain`, not `Match.Any` (otherwise it would be annoying to write Queries/Streams and run Jobs on them)

### Known Issues
* Entity-Entity Relations with an Entity that resides in the same Archetype (i.e. the relation is PART of the Archetype's signature) crashes when bulk Despawning entities.
* Entity-Entity Relations with an Entity itself are a special case of the above, that can additionally face crash problems when despawning the entity itself.
* Streams can no longer be warmed up (`Stream.Warmup()`) (like queries used to - this is an oversight). This results in one or several one-time 40 byte allocations to show up in BenchmarkDotNet output.


## Version 0.5.4-beta
- `Stream<>` is a lightweight View that can be created for any Query, and is what wraps zip_view-like enumeration and iteration over the Query (especiall `For`, `Job`, and `Raw`)
- `Stream<...>` is `IEnumerable<ValueTuple<Entity, ...>>`, which is great for Unit Testing and simple, read-only enumeration of Queries.
- `Stream<C1, C2, ...>` expose all the runner functions from `Stream<C1, C2>` and `Stream<C1>`.
- `Entity.Ref<C>` creates the component if it is not present on an entity.
- 
### Breaking Changes
- `Query` does no longer expose Runners; and no longer has intrinsic type parameters. Instead, `Stream<>` is used to access the same functionality.
- `Query` enumerates ONLY to Entities, and no longer has an `IEnumerator` of component types.

### Upgrade Steps
- instead of `World.Query<...>().Compile()`, you can use the shorthand `World.Query<...>().Stream()` or `World.Stream<...>()` to get a `Stream<>` instance to use.

### Known Issues
- the old StreamFilters on `Queries` have not been correctly ported to the `Stream<>` API, and won't wok.
- `Entity.Ref<C>` in these versions is impossible to invoke with certain type parameters.



## Legacy Releases
#### Version 0.5.3-beta
#### Version 0.5.2-beta
#### Version 0.5.0-beta
#### Version 0.4.6-beta
#### Version 0.4.5-beta
#### Version 0.4.2-beta
#### Version 0.4.0-beta
#### Version 0.3.5-beta
#### Version 0.2.0-beta
#### Version 0.1.1-beta
#### Version 0.1.0-beta
#### Version 0.0.3-pre
#### Version 0.0.1-pre

