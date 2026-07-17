# World, Lifecycle & Bulk Operations — full reference (fennecs 0.7.0)

## World

```csharp
using var world = new World(initialCapacity: 4096)
{
    Name = "simulation",                   // init-only, for debugging
    GCBehaviour = World.GCAction.DefaultBeta, // init-only, see GC below
    // StrictAspects = true,               // init-only, see Aspects reference
};
```

`World : IDisposable, IEnumerable<Entity>, IAspect` — implements the full
query/stream surface itself (`world.Query<…>()`, `world.Stream<…>()`,
`world.All`, `world.Count`, `foreach (var e in world)`).

### Limits

| Limit | Value |
|---|---|
| Entities per World | 2³⁰ = 1,073,741,824 (capacities grow in powers of two; 2³⁰ is the largest .NET array) |
| Concurrent Worlds per process | 255 — `Dispose()` worlds you're done with |
| Generations per entity slot | 16-bit, wraps |
| Stream types per query | 5 (more components per entity is fine; 5 is the per-view limit) |

Entities are world-bound: handles from a disposed World stop resolving
(`Alive == false`), and relations can never point across Worlds.

## Spawning & despawning

```csharp
var e = world.Spawn();                        // one entity, then .Add(...) fluently

using var spawner = world.Entity()            // EntitySpawner: preconfigure once
    .Add<Enemy>()
    .Add(new Health(50))
    .Add(new Owes(10M), creditor)             // relations & links work too
    .Add(Link.With(level));
spawner.Spawn(10_000);                        // spawn any number, reuse freely
spawner.Remove<Enemy>();                      // tweak config between spawns

var single = spawner.Spawn();                 // Spawn() returns the one spawned Entity
Span<Entity> wave = stackalloc Entity[64];
spawner.Spawn(wave);                          // spawns wave.Length entities, handles
                                              // written into the span (stays fluent)

e.Despawn();                // or world.Despawn(e)
world.DespawnAllWith<Projectile>();           // by component (optional Match)
query.Despawn();  stream.Despawn();           // bulk despawn matched
query.Truncate(1000);                         // cull down to a count
```

Liveness: `entity.Alive`, or just `if (entity)` (implicit bool). Handles are
generational — a stale handle to a reused slot is correctly dead. Despawning
an entity also removes all relations *targeting* it from other entities.

## Deferred structural changes

The World is normally in **Immediate** mode. Runners (`For`/`Job`/`Raw`) and
`world.Lock()` switch it to **Deferred**: structural changes (Add / Remove /
Despawn / Batch) are queued and applied when the last lock disposes. Reads and
component writes are never deferred.

```csharp
// Automatic: inside any runner, structural work through EntityRef is queued.
stream.For((in EntityRef e, ref Health hp) =>
{
    if (hp.Value <= 0) e.Despawn();          // applied after the For completes
});

// Manual: enumerating a query while modifying entities requires a lock.
using (var _ = world.Lock())
{
    foreach (var entity in query)
        if (Roll(entity)) entity.Add<Lucky>();
}   // changes flush here
```

Locks are reentrant (counted). Consequence of deferral: a component added
inside a runner is not visible (`Has` = false) until the scope closes.

## Batches — multiple structural changes on a query

A single bulk `query.Add<T>(…)`/`Remove<T>()` changes which archetypes match —
so consecutive calls can miss entities that "escaped" after the first change.
A `Batch` applies all operations to the *original* matched set atomically:

```csharp
query.Batch(Batch.AddConflict.Replace, Batch.RemoveConflict.Allow)
    .Add(new Cooldown(2f))
    .Add<RequestProjectileSpawn>()
    .Remove<Loaded>()
    .Submit();
```

Available from `Query.Batch(…)` and `Stream.Batch(…)`. After `Submit()` the
World owns the batch; call `Dispose()` only if you decide *not* to submit.

Conflict modes (both default `Strict`):

| `Batch.AddConflict` | Behavior when some matched entities already have the component |
|---|---|
| `Strict` | throw — unless the query's `Not<T>()` guarantees absence |
| `Preserve` | keep existing values, add where missing |
| `Replace` | overwrite everywhere, add where missing |

| `Batch.RemoveConflict` | Behavior when not all matched entities have the component |
|---|---|
| `Strict` | throw — unless the query's `Has<T>()` guarantees presence |
| `Allow` | remove where present, skip elsewhere (idempotent) |

## World GC & maintenance

Structural churn leaves empty/stagnant archetypes behind (especially despawned
relation targets). `world.GC()` compacts according to `GCBehaviour` flags
(`World.GCAction`: `CompactStagnantArchetypes`, `DisposeEmptyArchetypes`,
`DisposeEmptyRelationArchetypes`, `CompactMeta`, invoke-on triggers;
`DefaultBeta` is a sensible default). Call it at loading screens or level
transitions, in Immediate mode only (it throws inside a lock/runner scope).

`world.DebugString()` / `ToString()` report archetype counts and sizes —
the first thing to look at when diagnosing fragmentation.

## Disposal

`World` implements `IDisposable`. Dispose frees the world's registry slot (of
the 255) and invalidates its entities. In tests, `using var world = new
World();` per test keeps the registry clean. Note: 0.7.0 is in beta — object
links and queries are not exhaustively torn down on dispose yet; dispose
worlds at natural boundaries, not per-frame.

## Threading model

- Structural changes and query compilation: main/owning thread. fennecs has no
  internal synchronization for concurrent structural mutation.
- `Job` parallelizes *within* a runner call and joins before returning — your
  delegate runs concurrently, so uniforms/captured state must be thread-safe.
- There is no scheduler: you sequence systems yourself, which also means
  determinism is in your hands (archetype iteration order is deterministic).
