---
title: Roadmap
---

![a stylized fennec fox looking at an endless horizon](https://fennecs.tech/img/fennec-roadmap.png)

# The future of **fenn**ecs

Here's a list of planned and speculative features coming in the future.

Feedback and questions are always appreciated, please submit them on GitHub:
| What? | Where? |
| ------- | --------- |
| Issues | https://github.com/outfox/fennecs/issues |
| Discussions | https://github.com/outfox/fennecs/discussions |

Pull Requests especially welcome (please open an issue first to discuss the feature or bugfix you're planning to work on so your work gets the appreciation and attention it deserves).

### Q4 2024
::: details `1.0.0` Stable Release 
- 🎉 End of Beta
- *(maybe)* Code-Signed NuGet Package
- Documentation "feature complete" (every API feature explained)
- Short-Term Roadmap (until 1.5.0)
- Long-Term Roadmap (for 2.0.0)
:::

::: details `0.8.0+` Demos & Publicity (🦊help wanted)
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

::: details `0.7.0+` Benchmark Suite (🦊help wanted)
- internal set of benchmarks to ensure performance and memory usage are in line with expectations, and to prove good practices
- comparisons of **fenn**ecs with some other ECS libraries, likely as extended PR to [Doraku's Ecs.CSharp.Benchmark](https://github.com/Doraku/Ecs.CSharp.Benchmark)
:::


### Q3 2024
::: details `0.6.0+` Customizable Query & Archetype Garbage Collection
A set of specific or user-defined Interfaces that Component Types can "implement" to allow for nouanced garbage collection strategies.

**Concepts & Ideas**
- `Ephemeral` Interface for Components whose Archetypes should compact after each structural change.
- `Rare` Interface for Components that are only very rarely added, allowing a more conservative memory allocation strategy (instead of doubling).
- `World.SetGarbageCollectionStrategy<I>` to set the strategy for a given Interface.
:::

::: details `0.5.0+` Dog Fooding Phase + Enhanced Beta
- systematic internal and external beta testing
- using the library in a [real-world project](https://jupiter.blue)
- additional 1st party demos
- iteration on examples and tutorials
- strict examination of API surface for usability, elegance, and consistency
- consulting with subject matter experts for feedback
:::



### Q2 2024
::: details `0.1.x` ... `0.4.x` Fundamental Beta & Feedback Phase
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