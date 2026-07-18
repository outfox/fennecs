---
title: Shareables
order: 9
outline: [1, 2]
description: 'Shareable components in fennecs use reference types so many entities point at one instance - ideal for shared state, configs, and heavyweight objects.'
---

# Shareable Components :neofox_hug_duck_heart:

![two fennecs happily holding a huge cardboard box together](/img/fennecs-shareable.png)

::: tip :neofox_thumbsup: Shared State Made Simple
Need the same data across multiple entities? Shareable components let you use **reference types** to share a single instance between entities. One update, everyone sees it!
:::

## What is a Shareable?

A Shareable component is simply a reference type (`class` or `record`) used as a component. Multiple entities can hold references to the same instance, enabling efficient shared state.

```cs
public class SharedData
{
    public int Value;
}

var sharedData = new SharedData { Value = 42 };

var entity1 = world.Spawn().Add(sharedData);
var entity2 = world.Spawn().Add(sharedData);

// Both entities reference the same instance!
sharedData.Value = 100;  // Both see Value = 100
```

## When to Use Shareables

| Scenario | Why Shareable? |
|----------|----------------|
| **Heavyweight objects** | Expensive to create or copy |
| **Global configuration** | Settings shared across entities |
| **External references** | Game engine nodes, textures, etc. |
| **Shared counters/state** | Synchronized values |
| **Large data structures** | Don't want copies everywhere |

## Usage Examples

### Basic Sharing

```cs
public class TeamConfig
{
    public string Name;
    public int Score;
}

var blueTeam = new TeamConfig 
{ 
    Name = "Blue", 
    Score = 0 
};

// All blue team members share the same config
foreach (var entity in blueTeamEntities)
{
    entity.Add(blueTeam);
}

// Update score once, all members see it
blueTeam.Score += 10;
```

### Processing with Streams

::: code-group
```cs [Component Setup]
// A mutable record (or class)
record SharedData(int Value)
{
    public int Value { get; set; } = Value;
}
```

```cs [Iterating]
using var world = new World();
var stream = world.Query<SharedData>().Stream();

var sharedData = new SharedData(42);
world.Template().Add(sharedData).Spawn(5); // 5 entities share this

stream.For((ref data) =>
{
    data.Value++;  // Increments once per entity!
    Console.WriteLine(data.ToString());
});

sharedData.Value++;  // Increment outside of runner
Console.WriteLine();

stream.For((ref data) =>
{
    Console.WriteLine(data.ToString());
});
```

```plaintext [Output]
SharedData { Value = 43 }
SharedData { Value = 44 }
SharedData { Value = 45 }
SharedData { Value = 46 }
SharedData { Value = 47 }

SharedData { Value = 48 }
SharedData { Value = 48 }
SharedData { Value = 48 }
SharedData { Value = 48 }
SharedData { Value = 48 }
```
:::

### With EntityTemplate

```cs
var sharedTexture = LoadTexture("enemy.png");

// Bulk spawn entities with shared reference
world.Template()
    .Add(new Position())
    .Add(new Health { Value = 100 })
    .Add(sharedTexture)  // All share this texture
    .Add<Enemy>()
    .Spawn(1000);
```

## Shareables vs Object Links

| Shareable | Object Link |
|-----------|-------------|
| Plain component | Relation to object |
| One per type per entity | Multiple of same type allowed |
| `entity.Add(obj)` | `entity.Add(Link.With(obj))` |
| Groups by archetype normally | Groups entities by linked object |

```cs
Bank chase = new("Chase");

// Shareable: bob has chase as his plain Bank component
bob.Add(chase);

// Object Link: bob has a Bank relation TO chase
bob.Add(Link.With(chase));
// bob can also have another Bank link!
bob.Add(Link.With(targo));
```

See [Object Links](/docs/Advanced/Keys/Link.md) for more on relation-style object references.

## Performance Considerations :neofox_think:

::: info :neofox_science: The Trade-offs
1. **Reduced memory** – One instance instead of N copies
2. **Instant updates** – Change once, affects all entities
3. **Indirection cost** – Each access requires a pointer dereference
4. **Cache implications** – Data may not be contiguous in memory
:::

::: warning :neofox_owo: Iteration Performance
Queries that include reference components in their Stream Types will have slightly slower iteration due to memory indirection. However, queries that don't include the shareable as a Stream Type won't be affected!

```cs
// Slower: iterating through shared references
stream.For((ref data) => { ... });

// Not affected: SharedData not in stream types
var stream = world.Query<Position>()
    .Has<SharedData>()  // Just filtering, not streaming
    .Stream();
```
:::

## Best Practices

1. **Use for truly shared state** – If each entity needs different values, use value types
2. **Be mindful of mutation** – Changes affect all entities immediately
3. **Consider lifecycle** – Ensure shared objects outlive the entities using them
4. **Don't over-share** – Not everything needs to be shared

## Quick Reference

| Aspect | Shareables |
|--------|------------|
| **Type** | Reference types (`class`, `record`) |
| **Storage** | Heap, referenced by entities |
| **Sharing** | Same instance across entities |
| **Memory** | One allocation, many references |
| **Best For** | Expensive objects, shared state |

## Constraints

- Must be a reference type (`class` or `record`, not `struct`)
- Must be `notnull`
- Entity can only have one plain component of each type
- For multiple of same type, use [Object Links](/docs/Advanced/Keys/Link.md)