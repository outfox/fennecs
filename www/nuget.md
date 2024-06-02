![fennecs logo](https://raw.githubusercontent.com/thygrrr/fennecs/main/www/logos/fennecs-logo-nuget.svg)

[**fenn**ecs](https://fennecs.tech) is a lightweight, performant, and expressive ECS library for game & simulations written in modern C#. 

It's designed to be easy to use, with minimal boilerplate and no code generation or reflection required.

## 🚀 Quickstart

📦`>` `dotnet add package fennecs`

Here's a simple example to get you started:

```csharp
// Create a world
var world = new fennecs.World();

// Spawn an entity with a Position component
var entity = world.Spawn().Add<Vector3>();

// Create a query to update positions
var query = world.Query<Vector3>().Stream();

// Run the query
query.For(static (ref Vector3 position, float dt) => {
    pos.Y -= 9.81f * dt;
}, uniform: Time.deltaTime);
```

## 🌟 Key Features

- Modern C# codebase targeting .NET 8
- Archetype-based storage for cache-friendly iteration
- Expressive, queryable relations between entities and components
- Easy parallelization of workloads across and within archetypes
- Zero codegen and minimal boilerplate

## 📚 Learn More

Check out the [fennecs website](https://fennecs.tech) for:

- 📖 API documentation
- 🍳 Cookbook with tasty recipes
- 🎮 Demo projects to get inspired

Happy coding! 🦊❤️
