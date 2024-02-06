### ... the tiny, tiny, high-energy Entity Component System!

# What the fox!? Another ECS?

We know... oh, *we know.* 😩️

### But in a nutshell, **[fennecs](https://fennecs.tech)** is...

 🐾 zero codegen
 🐾 minimal boilerplate
 🐾 archetype-based 
 🐾 intuitively relational
 🐾 lithe and fast

 
**fennecs** is a re-imagining of [RelEcs/HypEcs](https://github.com/Byteron/HypEcs), extended and compacted until it *feels just right* for high performance game development in any modern C# engine. Including, of course, the fantastic [Godot 4.x](https://godotengine.org)!

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

### 💢... when we said minimal boilerplate, *we foxing meant it.*

Even using the strictest judgment, that's no more than 2 lines of boilerplate! Merely instantiating the world and building the query aren't directly moving parts of the actor/gravity feature we just built, and should be seen as "enablers" or "infrastructure".

The 💫*real magic*💫 is that none of this brevity compromises on performance.

## Features: What's in the box?

**fennECS** is a tiny, tiny ECS with a focus on performance and simplicity. And it cares enough to provide a few things you might not expect. Our competition sure didn't.

## Pile it on: Comparison Matrix

🥇🥈🥉ECS Comparison Matrix<br/><b>Foxes are soft, choices are hard</b> - Unity dumb; .NET 8 really sharp.

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

## Components

```csharp
// Components are simple structs.
struct Position { public int X, Y; }
struct Velocity { public int X, Y; }
```

## Systems

```csharp
// Systems add all the functionality to the Entity Component System.
// Usually, you would run them from within your game loop.
public class MoveSystem : ISystem
{
    public void Run(World world)
    {
        // iterate sets of components.
        var query = world.Query<Position, Velocity>().Build();
        query.Run((count, positions, velocities) => {
            for (var i = 0; i < count; i++)
            {
                positions[i].X += velocities[i].X;
                positions[i].Y += velocities[i].Y;
            }
        });
    }
}
```

### Spawning / De-Spawning Entities

```csharp
public void Run(World world)
{
    // Spawn a new entity into the world and store the id for later use
    Entity entity = world.Spawn().Id();
    
    // Despawn an entity.
    world.Despawn(entity);
}
```

### Adding / Removing Components

```csharp
public void Run(World world)
{
    // Spawn an entity with components
    Entity entity = world.Spawn()
        .Add(new Position())
        .Add(new Velocity { X = 5 })
        .Add<Tag>()
        .Id();
    
    // Change an Entities Components
    world.On(entity).Add(new Name { Value = "Bob" }).Remove<Tag>();
}
```

### Relations

```csharp
// Like components, relations are structs.
struct Apples { }
struct Likes { }
struct Owes { public int Amount; }
```

```csharp
public void Run(World world)
{
    var bob = world.Spawn().Id();
    var frank = world.Spawn().Id();
    
    // Relations consist of components, associated with a "target".
    // The target can either be another component, or an entity.
    world.On(bob).Add<Likes>(typeof(Apples));
    //   Component           ^^^^^^^^^^^^^^
    
    world.On(frank).Add(new Owes { Amount = 100 }, bob);
    //                                      Entity ^^^
    
    // if you want to know if an entity has a component
    bool doesBobHaveApples = world.HasComponent<Apples>(bob);
    // if you want to know if an entity has a relation
    bool doesBobLikeApples = world.HasComponent<Likes>(bob, typeof(Apples));
    
    // Or get it directly.
    // In this case, we retrieve the amount that Frank owes Bob.
    var owes = this.GetComponent<Owes>(frank, bob);
    Console.WriteLine($"Frank owes Bob {owes.Amount} dollars");
}
```

### Queries

```csharp
public void Run(World world)
{
    // With queries, we can get a list of components that we can iterate through.
    // A simple query looks like this
    var query = world.Query<Position, Velocity>().Build();
    
    // Now we can loop through these components
    query.Run((count, positions, velocities) => 
    {
        for (var i = 0; i < count; i++)
        {
            positions[i].X += velocities[i].X;
            positions[i].Y += velocities[i].Y;
        }
    });
    
    // we can also iterate through them using multithreading!
    // for that, we simply replace `Run` with `RunParallel`
    // note that HypEcs is an arche type based ECS.
    // when running iterations multithreaded, that means we parallelise each *Table* in the ecs,
    // not each component iteration. This means MultiThreading benefits from archetype fragmentation,
    // but does not bring any benefits when there is only one archetype existing in the ecs that is iterated.
    query.RunParallel((count, positions, velocities) => 
    {
        for (var i = 0; i < count; i++)
        {
            positions[i].X += velocities[i].X;
            positions[i].Y += velocities[i].Y;
        }
    });
    
    // You can create more complex, expressive queries through the QueryBuilder.
    // Here, we request every entity that has a Name component, owes money to Bob and does not have the Dead tag.
    var appleLovers = world.QueryBuilder<Entity, Name>().Has<Owes>(bob).Not<Dead>().Build();
    
    // Note that we only get the components inside Query<>.
    // Has<T>, Not<T> and Any<T> only filter, but we don't actually get T in the loop.
    appleLovers.Run((count, entities, names) => 
    {
        for (var i = 0; i < count; i++)
        {
            Console.WriteLine($"Entity {entities[i]} with name {names[i].Value} owes bob money and is still alive.")
        }
    });
}
```

## Creating a World

```csharp
// A world is a container for different kinds of data like entities & components.
World world = new World();
```

## Running a System

```csharp
// Create an instance of your system.
var moveSystem = new MoveSystem();

// Run the system.
// The system will match all entities of the world you enter as the parameter.
moveSystem.Run(world);

// You can run a system as many times as you like.
moveSystem.Run(world);
moveSystem.Run(world);
moveSystem.Run(world);

// Usually, systems are run once a frame, inside your game loop.
```

## SystemGroups

```csharp
// You can create system groups, which bundle together multiple systems.
SystemGroup group = new SystemGroup();

// Add any amount of systems to the group.
group.Add(new SomeSystem())
     .Add(new SomeOtherSystem())
     .Add(new AThirdSystem());

// Running a system group will run all of its systems in the order they were added.
group.Run(world);
```

## Example of a Game Loop

```csharp
// In this example, we are using the Godot Engine.
using Godot;
using HypEcs;
using World = HypEcs.World; // Godot also has a World class, so we need to specify this.

public class GameLoop : Node
{
    World world = new World();

    SystemGroup initSystems = new SystemGroup();
    SystemGroup runSystems = new SystemGroup();
    SystemGroup cleanupSystems = new SystemGroup();

    // Called once on node construction.
    public GameLoop()
    {
        // Add your initialization systems.
        initSystem.Add(new SomeSpawnSystem());

        // Add systems that should run every frame.
        runSystems.Add(new PhysicsSystem())
            .Add(new AnimationSystem())
            .Add(new PlayerControlSystem());
        
        // Add systems that are called once when the Node is removed.
        cleanupSystems.Add(new DespawnSystem());
    }

    // Called every time the node is added to the scene.
    public override void _Ready()
    {
        // Run the init systems.
        initSystems.Run(world);   
    }

    // Called every frame. Delta is time since the last frame.
    public override void _Process(float delta)
    {
        // Run the run systems.
        runSystems.Run(world);

        // IMPORTANT: For HypEcs to work properly, we need to tell the world when a frame is done.
        // For that, we call Tick() on the world, at the end of the function.
        world.Tick();
    }

    // Called when the node is removed from the SceneTree.
    public override void _ExitTree()
    {
        // Run the cleanup systems.
        cleanupSystems.Run(world);
    }
}
```


# Acknowledgements
Many thanks to [Byteron (Aaron Winter)](https://github.com/Byteron) for creating [HypEcs](https://github.com/Byteron/HypEcs) and [RelEcs](https://github.com/Byteron/RelEcs), the inspiring libraries that fennECS evolved from.