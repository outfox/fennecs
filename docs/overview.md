### ... the tiny, tiny, high-energy Entity Component System!

![fennecs logo](https://raw.githubusercontent.com/thygrrr/fennecs/main/docs/logos/fennecs-logo-nuget.svg)

# What the fox, another ECS?!

We know... oh, *we know.* 😩️

### But in a nutshell, **[fennecs](https://fennecs.tech)** is...

🐾 zero codegen  
🐾 minimal boilerplate  
🐾 archetype-based  
🐾 intuitively relational  
🐾 lithe and fast

**fennecs** is a re-imagining of [RelEcs/HypEcs](https://github.com/Byteron/HypEcs), extended and compacted until it *feels just right* for high performance game development in any modern C# engine.

## Quickstart: Let's go!

📦`>` `dotnet add package fennecs`

At the basic level, all you need is a 🧩**component type**, a number of ~~small foxes~~ 🦊**entities**, and a query to ⚙️**iterate and modify** components, occasionally passing in some uniform 💾**data**.

```csharp
// Declare your own component types. (you can also use most existing value or reference types)
using Position = System.Numerics.Vector3;

// Create a world. (fyi, World implements IDisposable)
var world = new fennecs.World();

// Spawn an entity into the world with a choice of components. (or add/remove them later)
var entity = world.Spawn().Add<Position>().Id();

// Queries are cached, just build them right where you want to use them.
var query = world.Query<Position>().Build();

// Run code on all entities in the query. (omit chunksize to parallelize only by archetype)
query.Job(static (ref Position position, float dt) => {
    position.Y -= 9.81f * dt;
}, uniform: Time.Delta, chunkSize: 2048);
```

### 💢... when we said minimal boilerplate, *we foxing meant it.*

Even using the strictest judgment, that's no more than 2 lines of boilerplate! Merely instantiating the world and building the query aren't directly moving parts of the actor/gravity feature we just built, and should be seen as "enablers" or "
infrastructure".

The 💫*real magic*💫 is that none of this brevity compromises on performance.

## Features: What's in the box?

**fennECS** is a tiny, tiny ECS with a focus on performance and simplicity. And it cares enough to provide a few things you might not expect. Our competition sure didn't.

------------------------

## 💡Highlights / Design Goals

- [x] Modern C# 12 codebase, targeting .NET 8.
- [x] Full Unit Test coverage.
- [ ] Benchmarking suite. (Work in Progress)

- [x] Workloads can be easily parallelized across *and within* Archetypes

- [x] Expressive, queryable relations between Entities and Objects
- [x] Entity Structural Changes with O(1) time complexity (per individual change).
- [x] Entity-Component Queries with O(1) runtime lookup time complexity.

- [x] No code generation and no reflection required.

------------------------

## ⏩ Nimble: _**fenn**ecs_ benchmarks

Preliminary (WIP) benchmarks suggest you can expect to process over 2 million components per millisecond on a 2020 CPU.
We worked hard to minimize allocations, though convenience, especially parallelization, has a tiny GC cost.

_**fenn**ecs_ provides a variety of ways to iterate over and modify components, to offer a good balance of control and elegance without compromising too much.

Here are some raw results from our benchmark suite, from the Vector3 operations parts, better ones soon.
(don't @ us)

### Executing a System.Numerics.Vector3 cross product and writing the result back with various calling methods

| Method     | entities  | chunk |       Mean |   StdDev | Jobs | Contention | Alloc |
|------------|-----------|------:|-----------:|---------:|-----:|-----------:|------:|
| Cross_JobU | 1_000_000 | 32768 |   349.9 us |  1.53 us |   32 |     0.0029 |     - |
| Cross_JobU | 1_000_000 | 16384 |   350.5 us |  5.82 us |   64 |     0.0005 |     - |
| Cross_JobU | 1_000_000 |  4096 |   356.1 us |  1.78 us |  248 |     0.0083 |     - |
| Cross_Job  | 1_000_000 |  4096 |   371.7 us | 15.36 us |  248 |     0.0103 |     - |
| Cross_Job  | 1_000_000 | 32768 |   381.6 us |  4.22 us |   32 |          - |     - |
| Cross_Job  | 1_000_000 | 16384 |   405.2 us |  4.56 us |   64 |     0.0039 |     - |
| Cross_RunU | 1_000_000 |     - | 1,268.4 us | 44.76 us |    - |          - |   1 B |
| Cross_Run  | 1_000_000 |     - | 1,827.0 us | 16.76 us |    - |          - |   1 B |

------------------------

# 🧡 Acknowledgements

Many thanks to [Byteron (Aaron Winter)](https://github.com/Byteron) for creating [HypEcs](https://github.com/Byteron/HypEcs) and [RelEcs](https://github.com/Byteron/RelEcs), the inspiring libraries that _**fenn**ecs_ evolved from.

