---
title: Roadmap
order: 3
---

![a stylized fennec fox looking at an endless horizon](/img/fennec-roadmap.png)

## The future of **fenn**ecs

Here's a list of planned and speculative developments coming in the future.

Feedback and questions are always appreciated, please submit them on GitHub / Discord:

| What? | Where? |
| ------- | --------- |
| Issues | <https://github.com/outfox/fennecs/issues> |
| Discussions | <https://discord.gg/eGNJaXRjPD> |

Pull Requests especially welcome (please open an issue first to discuss the feature or bugfix you're planning to work on so your work gets the appreciation and attention it deserves).

### "When it's Done"

::: details `1.0.0` Stable Release

- 🎉 End of Beta
- *(maybe)* Code-Signed NuGet Package
- Documentation "feature complete" (every API feature explained)
- Short-Term Roadmap (until 1.5.0)
- Long-Term Roadmap (for 2.0.0)
- 3rd party demos and examples
- 3rd party exposition & tutorials (e.g. YouTube, blog posts)
- 1st party exposition (e.g. YouTube shorts)
- Web support
  - Godot (?) (depends on Godot 4.4+)
  - others
- each Demo runs on at least two of these engines, and each engine has at least two demos:
  - Godot
  - Stride
  - Flax
  - MonoGame
  - Unigine
  - Web? (WebGL, WebAssembly)
  - (more? others?)
:::

### Q4 2026

::: details `0.9.0+` Benchmark Suite / 🦊**fenn**ecs Arena⚔️ (help wanted)

- internal set of benchmarks to ensure performance and memory usage are in line with expectations, and to prove good practices
- comparisons of **fenn**ecs with some other ECS libraries, likely as extended PR to (or fork of!) [Doraku's Ecs.CSharp.Benchmark](https://github.com/Doraku/Ecs.CSharp.Benchmark)
:::

### Q3 2026

::: details `0.8.0+` Easy SIMD

- A set of specific methods to allow for SIMD-accelerated arithmetic operations on Component data. Inspired by [TensorPrimitives](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.tensors.tensorprimitives?view=net-9.0).
:::

### Q2 2026

::: details `0.7.0` Aspects
Worlds gain [Aspects](/docs/Advanced/Aspects/index.md): self-contained collections of Archetypes with their own contiguous memory layout, sharing the World's Entities. Group hot data, fight fragmentation.

- ✅ first iteration: registration (`Owns<T>`), lazy membership, per-Aspect queries & streams, strict mode
- 🦊 still cooking: query bookkeeping optimizations, more docs and cookbook recipes
:::

::: details `0.7.x` Unified Entity
The entity struct needs a refactor for higher memory bandwidth and more consistency, freeing bit fields for new key types, such as hash keys. Previous attempts at this refactor weren't successful, so I'm moving this into its own milestone out of ~~fear~~ respect for its complexity
:::

### ~~Q3 2025~~

::: details `0.6.x` End of Beta: Stream Filters & API cleanups
After the dogfooding period, several feedbacks and experiences influence the following:

- `Stream` filters extended with the capability to filter by component values (in addition to component presence/absence). This is provided through a LINQ-like syntax: `Stream<...>.Where(lambda)`
- `Streams`become record structs, and as such are more lightweight and immutable *(their control structures, not their "contents")*.
:::

### ~~Q3 2024~~

::: details `0.5.0+` ~~Dog Fooding Phase + Enhanced Beta~~

- systematic internal and external beta testing
- using the library in a [real-world project](https://jupiter.blue)
- iteration on examples and tutorials
- strict examination of API surface for usability, elegance, and consistency
- consulting with subject matter experts for feedback
:::

### ~~Q2 2024~~

::: details `0.1.x` ... `0.4.x` ~~Fundamental Beta & Feedback Phase~~

- feedback acquisition & experimentation
- API stabilization
- submission to various game dev communities for feedback
- documentation and tutorials
- performance testing and optimization
- 3rd party benchmarking (e.g. [Doraku's Ecs.CSharp.Benchmark](https://github.com/Doraku/Ecs.CSharp.Benchmark))
:::

### ~~Q1 2024~~

::: details `0.1.0` ~~Prerelease and First Beta~~
**Done!**
:::
