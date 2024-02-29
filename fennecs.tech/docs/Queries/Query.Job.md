# `Query.Job`
# `Query.Job<U>`

For when many, many similar things need doing - fast!

## Chunk Size
Choosing the right way to spread your workload across CPU cores can yield significant performance gains.

By Default, fennecs parallelizes workloads only per whole Archetype. The `chunkSize` optional parameter passed into `Query.Job` affords fine-grained control over how the work is split up within each Archetype being processed.

Scheduling Jobs has a certain overhead, so just splitting work across as many CPUs as possible sometimes slows down processing speeds.

If you have many Archetypes that contain ~100,000 Entities, choosing a `ChunkSize` around `10,000` might be a good start to begin measuring and optimizing when planning to run on systems with 10 or more logical CPU cores.

If you have Archetypes that contain only a few dozen entities, but these have truly huge workloads (such as, entire Physics Worlds stored as a Component for regular updating through a Query), then a hypothetical Chunk Size as low as 1 could reserve 1 Job / 1 Core per Entity.
