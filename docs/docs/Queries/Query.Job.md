# `Query.Job`
# `Query.Job<U>`

::: info THE WORKHORSE
Many items, multi-threaded. Takes a [`RefAction`](Delegates.md#refaction-and-refactionu) delegate and instantly schedules and executes the workload split into chunks, calling many times in parallel across CPU cores.  
:neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle::neofox_waffle::neofox_nom_waffle:
:::

Sometimes, mommy and daddy foxes want to be on separate CPU cores. That doesn't mean they don't love you anymore! It only means that if you have **lots and lots** of entities in large Archetypes, you might get some performance gains for cheap.


## Chunk Size
Choosing the right way to spread your workload across CPU cores can yield significant performance gains.

By Default, **fenn**ecs parallelizes workloads only per entire Archetype. The `chunkSize` optional parameter passed into `Query.Job` affords fine-grained control over how the work is split up within each Archetype being processed.

:::warning :neofox_glare_sob: A GOOD TRADE-OFF LEAVES EVERYONE MAD!
Overhead for thread scheduling is real; as are context switches between threads. Experiment finding the right workload Chunk Size (start big - try 69,420, they say it's nice) and always consider giving [`Query.For`](Query.For.md) another look if you realize there's too much overhead or ==fragmentation==.

You can also set the [Filter State](FilterExpressions.md) of your Query to only include the Archetypes you want to process as a `Query.Job`, and use `Query.For` to do the rest. Or make it even easier: **Create Two Queries.**
:::

Scheduling Jobs has a certain overhead, so just splitting work across as many CPUs as possible sometimes slows down processing speeds.

If you have Archetypes that contain only a few dozen entities, but these have truly huge workloads (such as, entire Physics Worlds stored as a Component for regular updating through a Query), then a hypothetical Chunk Size as low as 1 could reserve 1 Job / 1 Core per Entity.
