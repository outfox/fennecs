---
title: Job
order: 2

---
# Parallel Query Workloads
#### `Stream<>.Job(ComponentAction<>)`
#### `Stream<>.Job<U>(U, UniformComponentAction<>)`

::: info ENTITY BY ENTITY, ONE BY ONE (IN PARALLEL!)
One work item at a time, multi-threaded. Super-fast, and with few synchronization caveats.
![three fennecs eating pizza together](https://fennecs.tech/img/fennec-job.png)
Takes a [`ComponentAction`](Delegates.md#ComponentAction-and-UniformComponentAction) or [`UniformComponentAction`](Delegates.md#ComponentAction-and-UniformComponentAction) delegate and instantly schedules and executes the workload split into chunks, calling it many times in parallel across CPU cores.  
:::

Sometimes, mommy and daddy foxes want to be on separate CPU cores. That doesn't mean they don't love each other anymore! It only means that if you ~~can keep a secret~~ have **lots and lots** of entities in large Archetypes, you might get ~~a new action figure~~ performance gains tomorrow!

### Basic Syntax
The nice part is, you can easily swap out `Stream.Job` for `Stream.For` and vice versa. There are optional parameters to optimize how the work is split up that you can use later to fine-tune your runtime performance.

::: code-group

```cs [Job(...) plain]
myStream.Job((ref Vector3 velocity) => 
{
    velocity += 9.81f * Vector3.DOWN * Time.deltaTime;
});
```

```cs [Job&lt;U&gt;(...) with uniform]
myStream.Job((ref Vector3 velocity, (Vector3 gravity, float dt) uniform) => 
{
    velocity += uniform.gravity * uniform.dt;
}, 
(9.81f * Vector3.DOWN, Time.deltaTime); 
```

```cs [Job&lt;U&gt;(...) with uniform + chunksize]
myStream.Job((ref Vector3 velocity, (Vector3 gravity, float dt) uniform) => 
{
    velocity += uniform.gravity * uniform.dt;
}, 
(9.81f * Vector3.DOWN, Time.deltaTime), chunkSize: 4096); 
```
:::

## Chunk Size
Choosing the right way to spread your workload across CPU cores can yield significant performance gains.

By Default, **fenn**ecs will parallelize workloads only per entire Archetype. The `chunkSize` optional parameter passed into `Stream.Job` affords fine-grained control over how the work is split up within each Archetype being processed.

:::warning :neofox_glare_sob: A GOOD TRADE-OFF LEAVES EVERYONE MAD!
Overhead for thread scheduling is real; as are context switches between threads. Experiment finding the right workload Chunk Size (start big - try 69,420, they say it's nice) and always consider giving [`Stream.For`](Stream.For.md) another look if you realize there's too much overhead or ==fragmentation==.

You can also set the [Filter State](Filters.md) of your Query to only include the Archetypes you want to process as a `Stream.Job`, and use `Stream.For` to do the rest. Or make it even easier: **Create Two Queries.**
:::

Scheduling Jobs has a certain overhead, so just splitting work across as many CPUs as possible sometimes slows down processing speeds.

If you have Archetypes that contain only a few dozen entities, but these have truly huge workloads (such as, entire Physics Worlds stored as a Component for regular updating through a Query), then a hypothetical Chunk Size as low as 1 could reserve 1 Job / 1 Core per Entity.
