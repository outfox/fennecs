[![fennecs logo](https://fennecs.tech/logos/fennecs-logo-darkmode.svg#gh-dark-mode-only)](https://fennecs.tech#gh-dark-mode-only)
[![fennecs logo](https://fennecs.tech/logos/fennecs-logo-lightmode.svg#gh-light-mode-only)](https://fennecs.tech#gh-light-mode-only)

# _... the tiny, tiny, high-energy Entity-Component System!_
[![dotnet add package fennecs](https://fennecs.tech/video/animation-dotnet-add-package-fennecs.svg)](https://fennecs.tech/cookbook/)

<table style="width: 100%">
   <tr>
   </tr>
   <tr>
      <td>
         <img src="https://fennecs.tech/logos/fennecs.png" alt="a box of fennecs, 8-color pixel art" style="min-width: 320px; max-width: 320px"/>
      </td>
      <td style="width: fit-content">
         <h1>Okay, what the fox!<br/><em>Another ECS?!</em></h1>
         <p>We know... oh, <em>we know.</em> 😩</p>
         <p>But in a nutshell, <a href="https://fennecs.tech"><b>fenn</b>ecs</a> is...</p>
         <p>
            🐾 zero codegen<br/>
            🐾 minimal boilerplate<br/>
            🐾 archetype-based<br/>
            🐾 intuitively relational<br/>
            🐾 lithe and fast<br/>
         </p>
      </td>
   </tr>
   <tr>
      <th colspan="2">
         <a href="https://discord.gg/3SF4gWhANS"><img alt="Discord" src="https://img.shields.io/badge/discord-_%E2%A4%9Coutfox%E2%A4%8F-blue?logo=discord&logoColor=f5f5f5"/></a>
         <a href="https://www.nuget.org/packages/fennecs/"><img alt="Nuget" src="https://img.shields.io/nuget/v/fennecs?color=blue"/></a>
         <a href="https://www.nuget.org/packages/fennecs/"><img alt="Downloads" src="https://img.shields.io/nuget/dt/fennecs"/></a>
         <a href="https://github.com/outfox/fennecs/actions"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/outfox/fennecs/xUnit.yml"/></a>
      </th>
   </tr>
</table>

## Quickstart
> *Brand new? Try the [cookbook](https://fennecs.tech/cookbook/) for a quick & tasty intro, or [dive into the docs](https://fennecs.tech/docs/)!*</br>
> *Familiar with ECS architectures? Get an [overview](https://fennecs.tech/docs/Concepts.html) of new & unique concepts!*

At the basic level, all you need is a 🧩**component type**, a number of ~~small foxes~~ 🦊**entities**, and a query to ⚙️**iterate and modify** components, occasionally passing in some uniform 💾**data**.
```cs
// Declare a component record. (we can also use most existing value & reference types)
record struct Velocity(Vector3 Value);

// Create a world. (fyi, World implements IDisposable)
var world = new fennecs.World();

// Spawn an entity into the world with a choice of components. (or add/remove them later)
var entity = world.Spawn().Add<Velocity>();

// Queries are cached & we use ultra-lightweight Stream Views to feed data to our code!
var stream = world.Query<Velocity>().Stream();

// Run code on all entities in the query. (exchange 'For' with 'Job' for parallel processing)
stream.For(
    uniform: DeltaTime * 9.81f * Vector3.UnitZ,
    action: (Vector3 uniform, ref Velocity velocity) =>
    {
        velocity.Value -= uniform;
    }
);
```

#### 💢... when we said minimal boilerplate, <em>we meant it.</em>

By any measure, we're talking just a couple of lines to get this gravity feature up and running. Creating the world and query is the only setup – the real slam dunk is how cleanly we built the full actor/gravity logic with barely any ceremonial code in sight.

And there's more: all that simplicity doesn't force any performance trade-offs! You get to have your cake and eat it too with zero confusion or fluff!

------------------------

## 💡Highlights / Design Goals

- [x] Modern C# 12 codebase, targeting .NET 8.
- [x] Full Unit Test coverage.
- [x] Powerfully intuitive ways to access data... _fast!_
- [x] Workloads can be easily parallelized across *and within* Archetypes
- [x] Expressive, queryable relations among Entities themselves & between Entities and Objects
- [x] No code generation and no reflection required.

------------------------

## 📕 DOCUMENTATION: [fennecs.tech](https://fennecs.tech) (official website)
Grab a cup of coffee to [get started](https://fennecs.tech), try [the Cookbook](https://fennecs.tech/cookbook/), view [the Demos](https://fennecs.tech/examples/) , and more!  
![coffee cup](https://fennecs.tech/emoji/neofox_cofe.png)

------------------------
## ⏩ Nimble: _**fenn**ecs_ benchmarks

Preliminary (WIP) benchmarks suggest you can expect to process over 2 million components per millisecond on a 2020 CPU without even customizing your logic.

Using Doraku's synthetic [Ecs.CSharp.Benchmark](https://github.com/Doraku/Ecs.CSharp.Benchmark/pull/36), fennecs scores among the faster ECS in the benchmark suite.  
*(link goes to PR #36 to reproduce)*

> [!WARNING]
> These are synthetic benchmarks, using a **BETA BUILD** of **fenn**ecs. Real-world performance will vary wildly.
> If you need a production-ready ECS *today*, 9 out of 10 foxes endorse [Friflo.Engine.ECS](https://github.com/friflo/Friflo.Json.Fliox/tree/main/Engine)👍 and [Flecs.NET](https://github.com/BeanCheeseBurrito/Flecs.NET)👍

Another optimization pass for **fenn**ecs is [on the Roadmap](https://fennecs.tech/misc/Roadmap.html).


### Benchmark: CreateEntityWithThreeComponents
```
// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT VectorSize=256
// Job: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
// [EntityCount=100_000]
```
| ECS & Method |	Duration<br/>**(less=better)**   |
| -------| -------:|
| 🦊 fennecs |	1.458 ms |  
| FrifloEngineEcs |	1.926 ms |  
| LeopotamEcs |	4.991 ms |  
| LeopotamEcsLite |	4.994 ms |  
| Arch |	7.811 ms |  
| FlecsNet |	17.838 ms |  
| DefaultEcs |	19.818 ms |  
| TinyEcs |	24.458 ms |  
| HypEcs |	25.215 ms |  
| MonoGameExtended |	27.562 ms |  
| Myriad |	28.249 ms |  
| SveltoECS |	52.311 ms |  
| Morpeh_Stash |	64.930 ms |  
| RelEcs |	65.023 ms |  
| Morpeh_Direct |	131.363 ms |  

### Benchmark: SystemWithThreeComponents
```
// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT VectorSize=256
// Job: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
// [EntityCount=100_000, EntityPadding=10]
```

| ECS & Method | Duration<br/>**(less=better)** | Comment |
| ---------- | ----------:| --------- |
| 🦊 fennecs(AVX2) | 10.43 µs | optimized Stream<>.Raw using AVX2 Intrinsics  |
| 🦊 fennecs(SSE2) | 11.41 µs | optimized Stream<>.Raw using SSE2 Intrinsics |
| FrifloEngineEcs_MultiThread | 13.45 µs |    |
| FrifloEngineEcs_SIMD_MonoThread | 16.92 µs |    |
| TinyEcs_EachJob | 20.51 µs |    |
| Myriad_MultiThreadChunk | 20.73 µs |    |
| TinyEcs_Each | 40.84 µs |    |
| FrifloEngineEcs_MonoThread | 43.41 µs |    |
| HypEcs_MonoThread | 43.86 µs |    |
| 🦊 fennecs(Raw) | 46.36 µs | straightforward loop over Stream<>.Raw |
| HypEcs_MultiThread | 46.80 µs |    |
| Myriad_SingleThreadChunk | 48.56 µs |    |
| Arch_MonoThread | 51.08 µs |    |
| Myriad_SingleThread | 55.65 µs |    |
| 🦊 fennecs(For) | 56.32 µs | your typical bread & butter **fenn**ecs workload  |
| Arch_MultiThread | 59.84 µs |    |
| FlecsNet_Iter | 77.47 µs |    |
| 🦊 fennecs(Job) | 97.70 µs | unoptimized in beta, ineffective <1M entities |
| DefaultEcs_MultiThread | 102.37 µs |    |
| Myriad_Delegate | 109.31 µs |    |
| Arch_MonoThread_SourceGenerated | 134.12 µs |    |
| DefaultEcs_MonoThread | 142.35 µs |    |
| LeopotamEcs | 181.76 µs |    |
| FlecsNet_Each | 212.61 µs |    |
| LeopotamEcsLite | 230.50 µs |    |
| Myriad_Enumerable | 245.76 µs |    |
| RelEcs | 250.93 µs |    |
| SveltoECS | 322.30 µs | EntityPadding=0, skips benchmark with 10   |
| MonoGameExtended | 387.12 µs |    |
| Morpeh_Stash | 992.62 µs |    |
| Myriad_MultiThread | 1115.44 µs |    |
| Morpeh_Direct | 2465.25 µs |    |


------------------------

## 🥊 Comparisons: Punching above our weight?
So how does _**fenn**ecs_ compare to other ECSs? 

This library is a tiny, tiny ECS with a focus on good performance and great simplicity. But it *cares enough* to provide a few things you might not expect.

> [!IMPORTANT]
> The idea of _**fenn**ecs_ was to fill the gaps that the author felt working with various established Entity-Component Systems. This is why this matrix is clearly imbalanced, it's a shopping list of things that _**fenn**ecs_ does well and was made to do
well; and things it may aspire to do but compromised on in order to be able to achieve the others.
>
> <em>(TL;DR - Foxes are soft, choices are hard - Unity dumb, .NET 8 really sharp.)</em>


<details>

<summary>🥇🥈🥉 (click to expand) ECS Comparison Matrix<br/><b></b></summary>

> Here are some of the key properties where _**fenn**ecs_ might be a better or worse choice than its peers. Our resident fennecs have worked with all of these ECSs, and we're happy to answer any questions you might have.

|                                                                           |           _**fenn**ecs_            |                HypEcs                | Entitas |            Unity DOTS            |            DefaultECS            |
|:--------------------------------------------------------------------------|:----------------------------------:|:------------------------------------:|:-------:|:--------------------------------:|:--------------------------------:|
| Boilerplate-to-Feature Ratio                                              |               3-to-1               |                5-to-1                | 12-to-1 |            27-to-1 😱            |              7-to-1              |
| Entity-Component Queries                                                  |                 ✅                  |                  ✅                   |    ✅    |                ✅                 |                ✅                 |
| Entity-Entity Relations                                                   |                 ✅                  |                  ✅                   |    ❌    |                ❌                 | ✅<br/><sup>(Map/MultiMap)</sup> |
| Entity-Object-Relations                                                   |                 ✅                  | 🟨</br><sup>(System.Type only)</sup> |    ❌    |                ❌                 |                ❌                 |
| Target Querying<br/>*<sup>(find all targets of specific relations)</sup>* |                 ✅                  |                  ❌                   |    ❌    |                ❌                 |                ✅                 |
| Wildcard Semantics<br/>*<sup>(match multiple relations in 1 query)</sup>* |                 ✅                  |                  ❌                   |    ❌    |                ❌                 |                ❌                 |
| Journaling                                                                |                 ❌                  |                  ❌                   |   🟨    |                ✅                 |                ❌                 |
| Shared Components                                                         | ✅<br/><sup>(ref types only)</sup>  |                  ❌                   |    ❌    |                🟨<br/><sup>(restrictive)</sup>                |                ✅                 | 
| Mutable Shared Components                                                 |                 ✅                  |                  ❌                   |    ❌    |                ❌                 |                ✅                 | 
| Reference Component Types                                                 |                 ✅                  |                  ❌                   |    ❌    |                ❌                 |                ❌                 |
| Arbitrary Component Types                                                 |                 ✅                  | ✅<br/><sup>(value types only)</sup>  |    ❌    |                ❌                 |                ✅                 |
| Structural Change Events                                                  |    🟨<br/><sup>(planned)</sup>     |                  ❌                   |    ✅    |  ☠️<br/><sup>(unreliable)</sup>  |                ❌                 |
| Workload Scheduling                                                       |                 ❌                  |                  ❌                   |      ❌  | ✅<br/><sup>(highly static)</sup> |                ✅                 |
| No Code Generation Required                                               |                 ✅                  |                  ✅                   |    ❌    |                ❌                 | 🟨<br/><sup>(roslyn addon)</sup> |
| Enqueue Structural Changes at Any Time                                    |                 ✅                  |                  ✅                   |    ✅    | 🟨<br/><sup>(restrictive)</sup>  |                🟨                |
| Apply Structural Changes at Any Time                                      |                 ❌                  |                  ❌                   |    ✅    |                ❌                 |                ❌                 |
| Parallel Processing                                                       |                 ⭐⭐                 |                  ⭐                   |    ❌    |               ⭐⭐⭐                |                ⭐⭐                |
| Singleton / Unique Components                                             | 🟨<br/><sup>(ref types only)</sup> |                  ❌                   |    ✅    |  🟨<br/><sup>(per system)</sup>  |                ✅                 |

</details>

------------------------

# 🧡 Acknowledgements
Many thanks to [Byteron (Aaron Winter)](https://github.com/Byteron) for creating [HypEcs](https://github.com/Byteron/HypEcs) and [RelEcs](https://github.com/Byteron/RelEcs), the inspiring libraries that _**fenn**ecs_ evolved from.

Neofox was created by [Volpeon](https://volpeon.ink/emojis/) and is in the Creative Commons [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0), the same license applies to all Neofox-derived works made for this documentation.
