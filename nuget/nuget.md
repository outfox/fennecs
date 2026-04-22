![fennecs logo](https://raw.githubusercontent.com/outfox/fennecs/main/nuget/fennecs-logo-nuget.svg)

[**fenn**ecs](https://fennecs.net) is a lightweight, performant, and expressive ECS library for game & simulations written in modern C#. 

It's designed to be easy to use, with minimal boilerplate and no code generation or reflection required.

## 🚀 Quickstart

📦`>` `dotnet add package fennecs`

Here's a simple example to get you started:

```cs
// Declare a Component record. (we can also use most existing value & reference types)
record struct Velocity(Vector3 Value);

// Create a world. (fyi, World implements IDisposable)
var world = new fennecs.World();

// Spawn an entity into the world with a choice of Components. (or add/remove them later)
var entity = world.Spawn().Add<Velocity>();

// Queries are cached & we use ultra-lightweight Stream Views to feed data to our code!
var stream = world.Query<Velocity>().Stream();

// Run code on all entities in the query. (exchange 'For' with 'Job' for parallel processing)
stream.For(
    uniform: DeltaTime * 9.81f * Vector3.UnitZ,
    static (Vector3 uniform, ref Velocity velocity) =>
    {
        velocity.Value -= uniform;
    }
);
```

## 🌟 Key Features

- Modern C# codebase targeting .NET 8, 9, and 10
- Archetype-based storage for cache-friendly iteration
- Expressive, queryable relations between entities and Components
- Easy parallelization of workloads across and within archetypes
- Zero codegen and minimal boilerplate

## 📚 Learn More

Check out the [fennecs website](https://fennecs.net) for:

- 📖 API documentation
- 🍳 Cookbook with tasty recipes
- 🎮 Demo projects to get inspired

Stay Foxy! 🦊❤️
