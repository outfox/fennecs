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

Even using the strictest judgment, that's no more than 2 lines of boilerplate! Merely instantiating the world and building the query aren't directly moving parts of the actor/gravity feature we just built, and should be seen as "enablers" or "infrastructure".

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

| Method                                       | entityCount   | Mean         | StdDev     | Ratio |
|--------------------------------------------- |---------------|-------------:|-----------:|------:|
| CrossProduct_Single_ECS_Lambda               | 1_000         |     2.004 us |  0.0978 us |  1.43 |
| CrossProduct_Parallel_ECS_Lambda             | 1_000         |     2.211 us |  0.0255 us |  1.58 |
| CrossProduct_Single_Span_Delegate            | 1_000         |     1.397 us |  0.0081 us |  1.00 |
| CrossProduct_Single_ECS_Delegate             | 1_000         |     2.085 us |  0.1131 us |  1.49 |
| CrossProduct_Single_ECS_Raw                  | 1_000         |     1.402 us |  0.0047 us |  1.00 |
| CrossProduct_Parallel_ECS_Raw                | 1_000         |     3.135 us |  0.0791 us |  2.24 |
| CrossProduct_Parallel_ECS_Delegate_Archetype | 1_000         |     2.211 us |  0.0163 us |  1.58 |
| CrossProduct_Parallel_ECS_Delegate_Chunk1k   | 1_000         |     2.195 us |  0.0013 us |  1.57 |
|                                              |               |              |            |       |
| CrossProduct_Single_ECS_Lambda               | 10_000        |    21.225 us |  1.4498 us |  1.73 |
| CrossProduct_Parallel_ECS_Lambda             | 10_000        |    24.437 us |  4.3404 us |  1.99 |
| CrossProduct_Single_Span_Delegate            | 10_000        |    12.288 us |  0.0282 us |  1.00 |
| CrossProduct_Single_ECS_Delegate             | 10_000        |    23.880 us |  1.9409 us |  1.94 |
| CrossProduct_Single_ECS_Raw                  | 10_000        |    12.388 us |  0.2673 us |  1.01 |
| CrossProduct_Parallel_ECS_Raw                | 10_000        |     8.111 us |  0.2773 us |  0.66 |
| CrossProduct_Parallel_ECS_Delegate_Archetype | 10_000        |    19.933 us |  0.0618 us |  1.62 |
| CrossProduct_Parallel_ECS_Delegate_Chunk1k   | 10_000        |    27.770 us |  0.2301 us |  2.26 |
|                                              |               |              |            |       |
| CrossProduct_Single_ECS_Lambda               | 100_000       |   173.340 us |  0.1528 us |  1.43 |
| CrossProduct_Parallel_ECS_Lambda             | 100_000       |   198.162 us |  1.7237 us |  1.64 |
| CrossProduct_Single_Span_Delegate            | 100_000       |   120.979 us |  0.8806 us |  1.00 |
| CrossProduct_Single_ECS_Delegate             | 100_000       |   195.004 us | 30.5909 us |  1.61 |
| CrossProduct_Single_ECS_Raw                  | 100_000       |   120.062 us |  0.2062 us |  0.99 |
| CrossProduct_Parallel_ECS_Raw                | 100_000       |    53.235 us |  1.2900 us |  0.44 |
| CrossProduct_Parallel_ECS_Delegate_Archetype | 100_000       |   197.735 us |  1.1834 us |  1.63 |
| CrossProduct_Parallel_ECS_Delegate_Chunk1k   | 100_000       |    67.614 us |  1.4787 us |  0.56 |
|                                              |               |              |            |       |
| CrossProduct_Single_ECS_Lambda               | 1_000_000     | 1,789.284 us | 71.5104 us |  1.49 |
| CrossProduct_Parallel_ECS_Lambda             | 1_000_000     | 1,978.499 us |  9.4791 us |  1.65 |
| CrossProduct_Single_Span_Delegate            | 1_000_000     | 1,197.915 us |  2.9327 us |  1.00 |
| CrossProduct_Single_ECS_Delegate             | 1_000_000     | 1,734.629 us |  2.4107 us |  1.45 |
| CrossProduct_Single_ECS_Raw                  | 1_000_000     | 1,208.246 us |  4.2537 us |  1.01 |
| CrossProduct_Parallel_ECS_Raw                | 1_000_000     |   363.921 us |  5.6343 us |  0.30 |
| CrossProduct_Parallel_ECS_Delegate_Archetype | 1_000_000     | 1,980.063 us | 18.7070 us |  1.65 |
| CrossProduct_Parallel_ECS_Delegate_Chunk1k   | 1_000_000     |   305.559 us |  1.2544 us |  0.26 |


------------------------

## 📅 Future Roadmap

- 🟦 Unity Support: Planned for when Unity is on .NET 8 or later, and C# 12 or later. Or when we can't wait any longer.
- ✅ ~~_**fenn**ecs_ as a NuGet package~~ (done!)
- 🟦 _**fenn**ecs_ as a Godot addon

------------------------

# 🧡 Acknowledgements

Many thanks to [Byteron (Aaron Winter)](https://github.com/Byteron) for creating [HypEcs](https://github.com/Byteron/HypEcs) and [RelEcs](https://github.com/Byteron/RelEcs), the inspiring libraries that _**fenn**ecs_ evolved from.

