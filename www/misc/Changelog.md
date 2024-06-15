---
title: Release Notes
order: 2
outline: [2,2]

---

![a stylized fox shattering red and green polygons surrounded by source code](https://fennecs.tech/img/fennec-changelog.png)

# Release Notes
Here, there be ~~dragons~~ more foxes. *What did you expect?*

> [!CAUTION] BETA NOTICE
> **fenn**ecs will remain in Beta until version 1.0.0, which is expected in Q4 2024. Breaking API changes as well as bugs are likely to occur without warning in these beta builds. 
> You are nonetheless encouraged to try **fenn**ecs out, play around and experiment with the package freely; our resident foxes aim to keep it it as useful and stable as possible! Please report issues and feedback on the [GitHub Issues](https://github.com/outfox/fennecs/issues) board.

## UPCOMING
- `Match.Object` may become internal / deprecated
...

## Release 0.5.6-beta
- `Link.Any` is a Match Target that can be used to match any Link target in a Query. It's value-identical to `Match.Object`, but makes the code more readable and reads in line with `Entity.Any`. 
- lots of documentation updates and fixes

## Release 0.5.5-beta
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


## Release 0.5.4-beta
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
#### Release 0.5.3-beta
#### Release 0.5.2-beta
#### Release 0.5.0-beta
#### Release 0.4.6-beta
#### Release 0.4.5-beta
#### Release 0.4.2-beta
#### Release 0.4.0-beta
#### Release 0.3.5-beta
#### Release 0.2.0-beta
#### Release 0.1.1-beta
#### Release 0.1.0-beta
#### Release 0.0.3-pre
#### Release 0.0.1-pre

