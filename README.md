## ... the tiny, tiny, high-energy Entity Component System!
<table style="border: none; border-collapse: collapse; width: 80%">
    <tr>
        <td style="width: fit-content">
            <img src="Documentation/Logos/fennecs.png" alt="a box of fennecs, 8-color pixel art" style="min-width: 320px"/>
        </td>
        <td>
            <h1>What the fox!? Another ECS?</h1>
            <p>We know... oh, <em>we know.</em> 😩️</p>  
            <h3>But in a nutshell, <a href="https://fennecs.tech"><span style="font-size: larger">fennecs</span></a> is...</h3>
            <ul style="list-style-type: '🐾 ';">
                <li>zero codegen</li>
                <li>minimal boilerplate</li>
                <li>archetype-based</li>
                <li>intuitively relational</li>
                <li>lithe and fast</li>
            </ul>
            <p><b>fennecs</b> is a re-imagining of <a href="https://github.com/Byteron/HypEcs">RelEcs/HypEcs</a> 
            which <em>feels just right<a href="#quickstart-lets-go">*</a></em> for high performance game development in any modern C# engine. Including, of course, the fantastic <a href="https://godotengine.org">Godot</a>.</p>
        </td>
    </tr>
<tr><td><i>☝️ 9 out of 10 fennecs<br>recommend: fennecs!</i></td><td><img alt="GitHub top language" src="https://img.shields.io/github/languages/top/thygrrr/fennECS">
<a href="https://github.com/thygrrr/fennECS?tab=MIT-1-ov-file#readme"><img alt="License: MIT" src="https://img.shields.io/github/license/thygrrr/fennECS?color=blue"></a>
<a href="https://github.com/thygrrr/fennECS/issues"><img alt="Open issues" src="https://img.shields.io/github/issues-raw/thygrrr/fennECS"></a>
<a href="https://github.com/thygrrr/fennECS/actions"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/thygrrr/fennECS/xUnit.yml"></a>
</td></tr>
</table>

## Quickstart: Let's go!
📦`>` `dotnet add package fennecs`

At the basic level, all you need is a 🧩**component type**, a number of ~~small foxes~~ 🦊**entities**, and a query to ⚙️**iterate and modify** components, occasionally passing in some uniform 💾**data**.

```csharp
// Declare your own component types. (you can also use most existing value or reference types)
using Position = System.Numerics.Vector3;

// Create a world. (fyi, World implements IDisposable)
var world = new ECS.World();

// Spawn an entity into the world with a choice of components. (or add/remove them later)
var entity = world.Spawn().Add<Position>().Id();

// Queries are cached, just build them right where you want to use them.
var query = world.Query<Position>().Build();

// Run code on all entities in the query. (omit chunksize to parallelize only by archetype)
query.RunParallel((ref Position position, in float dt) => {
    position.Y -= 9.81f * dt;
}, uniform: Time.Delta, chunkSize: 2048);
```

### 💢... when we said minimal boilerplate, <em>we foxing meant it.</em>

Even using the strictest judgment, that's no more than 2 lines of boilerplate! Merely instantiating the world and building the query aren't directly moving parts of the actor/gravity feature we just built, and should be seen as "enablers" or "infrastructure".

The 💫*real magic*💫 is that none of this brevity compromises on performance.

## Features: What's in the box?

**fennECS** is a tiny, tiny ECS with a focus on performance and simplicity. And it cares enough to provide a few things you might not expect. Our competition sure didn't.

## Pile it on: Comparison Matrix

<!--<img src="Documentation/Logos/fennecs-group.png" width="768px" alt="Multiple colorful anthro fennecs in pixel art" />-->

<details>

<summary>🥇🥈🥉ECS Comparison Matrix<br/><b>Foxes are soft, choices are hard</b> - Unity dumb; .NET 8 really sharp.</summary>

Here are some of the key properties where fennECS might be a better or worse choice than its peers. Our resident fennecs have worked with all of these ECSs, and we're happy to answer any questions you might have.


|                                                               |            fennECS            | HypEcs | Entitas |    Unity DOTS    | DefaultECS |
|:--------------------------------------------------------------|:-----------------------------:|:------:|:-------:|:----------------:|:----------:|
| Boilerplate-to-Feature Ratio                                  |            3-to-1             | 5-to-1 | 12-to-1 |    27-to-1 😱    |   7-to-1   |
| Entity-Target Relations                                       |               ✅               |   ✅    |    ❌    |        ❌         |     ❌      |
| Target Querying<br/>*(find all targets of relations of type)* |               ✅               |   ❌    |    ❌    |        ❌         |     ❌      |
| Entity-Component Queries                                      |               ✅               |   ✅    |    ✅    |        ✅         |     ✅      |
| Add Shared Components                                         |               ✅               |   ❌   |    ❌    |        🟨        |     ✅      | 
| Change Shared Components                                      |               ✅               |   ❌   |    ❌    |        ❌         |     ✅      | 
| Entity-Type-Relations                                         |               ❌               |   ✅    |    ❌    |        ❌         |     ❌      |
| Entity-Target-Querying                                        |               ✅               |   ❌    |    ❌    |        ❌         |     ❌      |
| Arbitrary Component Types                                     |               ✅               |   ✅    |    ❌    |        ❌         |     ✅      |
| Structural Change Responders                                  |     🟨<br/>(coming soon)      |   ❌    |    ✅    |        ❌         |     ❌      |
| Automatic Thread Scheduling                                   |  🟨<br/>(coming soon)  |   ❌    |      ❌  | ✅<br/>(highly static) |     ✅      |
| No Code Generation Required                                   |               ✅               |   ✅    |    ❌    |        ❌         |     🟨     |
| Enqueue Structural Changes at Any Time                        |               ✅               |   ✅    |    ✅    |        🟨        |     🟨     |
| Apply Structural Changes at Any Time                          |               ❌               |   ❌    |    ✅    |        ❌         |     ❌      |
| C# 12 support                                                 |               ✅               |   ❌    |    ❌    |        ❌         |     ❌      |
| Parallel Processing                                           |              ⭐⭐               |   ⭐    |    ❌    |       ⭐⭐⭐        |     ⭐⭐     |
| Singleton / Unique Components                                 |    🟨<br/>(ref types only)    |   ❌    |    ✅    |  🟨<br/>(per system)  |     ✅      |
| Journaling                                                    |               ❌               |   ❌    |   🟨    |        ✅         |     ❌      |


</details>

## Highlights / Design Goals

- Entity-Entity-Relations with O(1) lookup time complexity.
- Entity-Component Queries with O(1) lookup time complexity.
- Entity Spawning and De-Spawning with O(1) time complexity.
- Entity Structural Changes with O(1) time complexity (per individual change).

- Workloads can be parallelized across Archetypes (old) and within Archetypes (new).

- Unit Test coverage.
- Benchmarking suite.
- Modern C# 12 codebase, targeting .NET 8.
- Godot 4.x Sample Integrations.

## Future Roadmap

- Unity Support: Planned for when Unity is on .NET 7 or later, and C# 12 or later.
- fennECS as a NuGet package
- fennECS as a Godot addon

## Already plays well with Godot 4.x!

<img src="Documentation/Logos/godot-icon.svg" width="128px" alt="Godot Engine Logo, Copyright (c) 2017 Andrea Calabró" />

# Legacy Documentation

The old HypEcs [documentation can be found here](Documentation/legacy.md) while our fennecs are frantically writing up new docs for the new APIs.

# Acknowledgements
Many thanks to [Byteron (Aaron Winter)](https://github.com/Byteron) for creating [HypEcs](https://github.com/Byteron/HypEcs) and [RelEcs](https://github.com/Byteron/RelEcs), the inspiring libraries that fennECS evolved from.