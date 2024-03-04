---
title: Query<>.Raw
---
# Custom Query Workloads
# `Query<>.Raw(RefAction)`
# `Query<>.Raw<U>(RefAction,U)`

::: danger THE FREIGHT TRAIN
Work items as contiguous memory. Using a distinct signature, [`MemoryAction`](Delegates.md#memoryaction-and-memoryactionu), delivers the *entire workload* of each Archetype diriectly into your ~~fox~~ delegate via a single call.
:neofox_waffle_long_blurry::neofox_scream_stare:
:::

## Processing entire Memory Blocks

Your code controls how and where. Maximum power, maximum responsibility.  
_(in reality, `Memory<T>` is quite easy to use in C#, but can be more difficult to debug!)_

Especially for [blittable Component types](https://learn.microsoft.com/en-us/dotnet/framework/interop/default-marshalling-behavior#default-marshalling-for-value-types), being able to refer to all of them an a single memory region can be a powerful utility when interacting with drivers, networks, or game engines.

::: danger :neofox_solder_googly: DANGER! Memory marshaling and multithreading are POWER TOOLS!

Copying memory regions is hard, and **Multiprocessing is HARDER.** Currently, **fenn**ecs doesn't provide parallel write restrictions, so you need to be aware of when and how you're changing data that you're processing in bulk from other threads.
:::


### Basic Syntax
::: code-group

```cs [Raw(...) DIY]
// This is NOT how Raw is usually used, but you can, in the trivial
// case, use it to iterate your workload yourself. The main use for that
// variant is to perform some sort of early-out iteration, e.g. search)
myQuery.Raw((Memory<Vector3> velocities) => 
{
    foreach (ref var velocity in velocities.Span) 
    {
        velocity += 9.81f * Vector3.DOWN * Application.FrameRate;
    }
});
```

```cs [Raw&lt;U&gt;(...) with uniform]
// This is NOT how Raw is usually used, but you can, in the trivial
// case, use it to iterate your workload yourself. The main use for that
// variant is to perform some sort of early-out iteration, e.g. search)
myQuery.Raw((Memory<Vector3> velocities, (float dt, Vector3 g) uniform) => 
{
    foreach (ref var velocity in velocities.Span) 
    {
        velocity += uniform.g * uniform.dt;
    }
}, 
(Time.deltaTime, 9.81f * Vector3.DOWN); 
```

:::


## Differences to other Runners
`Query<>.Raw` differs from `Query<>.For` by providing a `Memory<T>` for each Stream Type instead, and instead of running the delegate for each Entity, it is instead run once for each Archetype, and the `Memory<T>` structs provided constitute the entirety of all Component data for the Stream Type `T`.

All the `Memory<T>` structs are the same length, because each Entity in the Archetype has exactly one of the Archetype's components.

[Match Expressions](MatchExpressions.md) work the same as for other Runners, meaning Entities will get enumerated for each matching Stream Type in the Archetype when applicable.



## When to use `Raw`
This Runner's primary use is to transfer Component data into another facility, such as your Game Engine, Render Buffer, or for Serialization. Each `Memory<T>` is guaranteed to be contiguously filled with component data, in the same index order (think rows) for each Entity.

Raw processing is synchronous, immediate, and happens on the calling thread.

### Examples
You can either access this memory as a `Span`, cast it to the desired type, etc. Depending on your use case and context, it may be necessary to [Pin the Memory](https://learn.microsoft.com/en-us/dotnet/api/system.memory-1.pin) to get a [MemoryHandle](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.memoryhandle).

::: code-group
```cs [🦋 use as span]
//TODO :)
```

```cs [☠️ cast to another type]
//TODO :)
```

```cs [☠️☠️ transfer as buffer]
//TODO :)
```

```cs [☠️☠️☠️ pin for GPU]
//TODO :)
```
:::


**fenn**ecs internals guarantee that the memory order and size remains the same for as long as no ==structural changes== are made to the Entities in the Query.


::: info :neofox_sign_yes: We have MEMORY, yes. What about ~~second memory~~ THREADS?
 Good news! You are free to parallelize all work yourself in your `MemoryAction<>` to your heart's content! 
Go and make network transfers, disk access, or pass calculations to a GPU and write the results back. Simply use any suitable threading methods for as long as the Runner is active.

But should you need to defer ==structural changes== for longer (i.e. your parallel processes need to go on after the Runner returns), be sure to take out an additional ==World Lock== (as the Runners will dispose theirs when done)

::: tip :neofox_think_anime: PAWS for THOUGHT
__You are the architect__. Perhaps you could structure your implementation ensuring that the Archetypes matched by the Query aren't changed elsewhere while your code performs async work on them?  
_(i.e. no ??structural changes?? to any of the Entities contained)_

Then you might avoid having to lock your World altogether!

**fenn**ecs internals guarantee that the memory order and size remains the same for as long as no structural changes are made.
:::

### async/await
In case you want to use the TPL (async/await), note that the Runner itself isn't async, but you could, _if you really wanted_, pass in a uniform with a list of tasks to populate, complete them using [`Task.WaitAll`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.waitall) or polling for completion, and finally dispose the ==World Lock==.


