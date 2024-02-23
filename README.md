![fennecs logo](./docs/logos/fennecs-logo-darkmode.svg#gh-dark-mode-only) ![fennecs logo](./docs/logos/fennecs-logo-lightmode.svg#gh-light-mode-only)

<table style="width: 90%">
   <th colspan="9">
      <h2><em>... the tiny, tiny, high-energy Entity Component System!</em></h2>
   </th>
   <tr>
      <td colspan="4" style="width: fit-content">
         <img src="docs/logos/fennecs.png" alt="a box of fennecs, 8-color pixel art" style="min-width: 320px"/>
      </td>
      <td colspan="5">
         <h1>What the fox, another ECS?!</h1>
         <p>We know... oh, <em>we know.</em> 😩</p>
         <p>But in a nutshell, <a href="https://fennecs.tech"><span style="font-size: larger"><em><b>fenn</b>ecs</em></span></a> is...</p>
         <p>
            🐾 zero codegen<br/>
            🐾 minimal boilerplate<br/>
            🐾 archetype-based<br/>
            🐾 intuitively relational<br/>
            🐾 lithe and fast<br/>
         </p>
         <p><span style="font-size: larger"><em><b>fenn</b>ecs</em></span> is a re-imagining of <a href="https://github.com/Byteron/HypEcs">RelEcs/HypEcs</a> 
            which <em>feels just right<a href="#quickstart-lets-go">*</a></em> for high performance game development in any modern C# engine. Including, of course, the fantastic <a href="https://godotengine.org">Godot</a>.
         </p>
        <p></p>
      </td>
   </tr>
<th colspan="9">
     <a href="https://www.nuget.org/packages/fennecs/"><img alt="Nuget" src="https://img.shields.io/nuget/v/fennecs?color=blue"/></a>
     <a href="https://github.com/thygrrr/fennECS/actions"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/thygrrr/fennECS/xUnit.yml"/></a>
     <a href="https://github.com/thygrrr/fennECS/issues"><img alt="Open issues" src="https://img.shields.io/github/issues-raw/thygrrr/fennECS?color=green"/></a>
     <img alt="GitHub top language" src="https://img.shields.io/github/languages/top/thygrrr/fennECS"/>
     <a href="https://github.com/thygrrr/fennECS?tab=MIT-1-ov-file#readme"><img alt="License: MIT" src="https://img.shields.io/github/license/thygrrr/fennECS?color=blue"/></a>
</th>
   <tr>
      <td colspan="3"><h3>Code Samples</h3></td>
      <td colspan="7">
      </td>
   </tr>
   <tr>
      <td>MonoGame<br/><sup>(soon)</sup></td>
      <td>Stride<br/><sup>(soon)</sup></td>
      <td><a href="https://github.com/thygrrr/fennecs/tree/main/examples/example-godot">Godot<br/><sup>(WIP)</sup></a></td>
      <td>Flax<br/><sup>(soon)</sup></td>
      <td>Unity<br/><sup>(waiting)</sup></td>
      <td>Duality<br/><sup>(soon)</sup></td>
      <td>Evergine<br/><sup>(soon)</sup></td>
      <td>UNIGINE<br/><sup>(soon)</sup></td>
      <td>NeoAxis<br/><sup>(soon)</sup></td>
   </tr>
</table>

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

#### 💢... when we said minimal boilerplate, <em>we meant it.</em>
Even using the strictest judgment, that's no more than 2 lines of boilerplate! Merely instantiating the world and building the query aren't directly moving parts of the actor/gravity feature we just built, and should be seen as "enablers" or "infrastructure".  

The 💫*real magic*💫 is that none of this brevity compromises on performance.

------------------------

## 🥊 Comparisons: Punching above our weight?
So how does _**fenn**ecs_ compare to other ECSs? 

This library is a tiny, tiny ECS with a focus on good performance and great simplicity. But it *cares enough* to provide a few things you might not expect.

> [!IMPORTANT]
> The idea of _**fenn**ecs_ was to fill the gaps that the author felt working with various established Entity Component Systems. This is why this matrix is clearly imbalanced, it's a shopping list of things that _**fenn**ecs_ does well and was made to do
well; and things it may aspire to do but compromised on in order to be able to achieve the others.
>
> <em>(TL;DR - Foxes are soft, choices are hard - Unity dumb, .NET 8 really sharp.)</em>


<details>

<summary>🥇🥈🥉 (click to expand) ECS Comparison Matrix<br/><b></b></summary>

> Here are some of the key properties where _**fenn**ecs_ might be a better or worse choice than its peers. Our resident fennecs have worked with all of these ECSs, and we're happy to answer any questions you might have.

|                                                                           |            _**fenn**ecs_            |                HypEcs                | Entitas |            Unity DOTS            |            DefaultECS            |
|:--------------------------------------------------------------------------|:-----------------------------------:|:------------------------------------:|:-------:|:--------------------------------:|:--------------------------------:|
| Boilerplate-to-Feature Ratio                                              |               3-to-1                |                5-to-1                | 12-to-1 |            27-to-1 😱            |              7-to-1              |
| Entity-Component Queries                                                  |                  ✅                  |                  ✅                   |    ✅    |                ✅                 |                ✅                 |
| Entity-Target Relations                                                   |                  ✅                  |                  ✅                   |    ❌    |                ❌                 | ✅<br/><sup>(Map/MultiMap)</sup> |
| Entity-Object-Relations                                                   |                  ✅                  | 🟨</br><sup>(System.Type only)</sup> |    ❌    |                ❌                 |                ❌                 |
| Target Querying<br/>*<sup>(find all targets of specific relations)</sup>* |                  ✅                  |                  ❌                   |    ❌    |                ❌                 |                ✅                 |
| Wildcard Semantics<br/>*<sup>(match multiple relations in 1 query)</sup>* |                  ✅                  |                  ❌                   |    ❌    |                ❌                 |                ❌                 |
| Journaling                                                                |                  ❌                  |                  ❌                   |   🟨    |                ✅                 |                ❌                 |
| Shared Components                                                         | ✅<br/><sup>(ref types only)</sup>   |                  ❌                   |    ❌    |                🟨<br/><sup>(restrictive)</sup>                |                ✅                 | 
| Mutable Shared Components                                                 |                  ✅                  |                  ❌                   |    ❌    |                ❌                 |                ✅                 | 
| Reference Component Types                                                 |                  ✅                  |                  ❌                   |    ❌    |                ❌                 |                ❌                 |
| Arbitrary Component Types                                                 |                  ✅                  | ✅<br/><sup>(value types only)</sup>  |    ❌    |                ❌                 |                ✅                 |
| Structural Change Events                                                  |      🟨<br/><sup>(soon)</sup>       |                  ❌                   |    ✅    |  ☠️<br/><sup>(unreliable)</sup>  |                ❌                 |
| Workload Scheduling                                                       |      🟨<br/><sup>(soon)</sup>       |                  ❌                   |      ❌  | ✅<br/><sup>(highly static)</sup> |                ✅                 |
| No Code Generation Required                                               |                  ✅                  |                  ✅                   |    ❌    |                ❌                 | 🟨<br/><sup>(roslyn addon)</sup> |
| Enqueue Structural Changes at Any Time                                    |                  ✅                  |                  ✅                   |    ✅    | 🟨<br/><sup>(restrictive)</sup>  |                🟨                |
| Apply Structural Changes at Any Time                                      |                  ❌                  |                  ❌                   |    ✅    |                ❌                 |                ❌                 |
| Parallel Processing                                                       |                 ⭐⭐                  |                  ⭐                   |    ❌    |               ⭐⭐⭐                |                ⭐⭐                |
| Singleton / Unique Components                                             | 🟨<br/><sup>(ref types only)</sup>  |                  ❌                   |    ✅    |  🟨<br/><sup>(per system)</sup>  |                ✅                 |

</details>

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

<details>

<summary>📈 Click to Expand Benchmarks: </summary>
<pre>executing a System.Numerics.Vector3 cross product and writing the result back with various calling methods</pre>

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

</details>

------------------------

## 📅 Future Roadmap

- 🟦 Unity Support: Planned for when Unity is on .NET 8 or later, and C# 12 or later. Or when we can't wait any longer.
- ✅ ~~_**fenn**ecs_ as a NuGet package~~ (done!)
- 🟦 _**fenn**ecs_ as a Godot addon

------------------------

# 🧡 Acknowledgements
Many thanks to [Byteron (Aaron Winter)](https://github.com/Byteron) for creating [HypEcs](https://github.com/Byteron/HypEcs) and [RelEcs](https://github.com/Byteron/RelEcs), the inspiring libraries that _**fenn**ecs_ evolved from.


------------------------

# 🕸️ Legacy Documentation

The old HypEcs [documentation can be found here](docs/legacy.md) while our fennecs are frantically writing up new docs for the new APIs.

