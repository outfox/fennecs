---
title: 3-Body Problem
outline: [2, 3]
---

# Trisolarians, Trishmolarians

**fenn**ecs's relation features allow us you to simulate the 3-Body Problem with ease.

::: code-group

```csharp [Components]
// This is both our relation component ("Attractor")
// as well as the "Body" component itself.
// We create it as a class so the same backing object
// is used for both the plain and relation components.
private class Body
{
    public Vector3 position;
    public float mass { init; get; }
}

// Velocity of our Bodies
// A simple Vector3 component
// Fox typing is optional, as always
private struct Velocity : Fox<Vector3>
{
    public Vector3 Value { get; set; }
}

// Cumulative Forces acting on Bodies
// A simple Vector3 component.
// Fox typing is optional, as always
private struct Force : Fox<Vector3>
{
    public Vector3 Value { get; set; }
}

// Position of our Bodies
// A simple Vector3 component.
// Fox typing is optional, as always
private struct Position : Fox<Vector3>
{
    public Vector3 Value { get; set; }
}
```

```csharp [System Setup]
var world = new World();
const int bodyCount = 3;

// Determine starting positions
var p1 = new Vector3(-10, -4, 0);
var p2 = new Vector3(0, 12, 0);
var p3 = new Vector3(7, 0, 4);

// Create our Body Components
// These will serve both as "Attractor" relations
// as well as the "Bodies" themselves
var body1 = new Body{position = p1, mass = 2.0f};
var body2 = new Body{position = p2, mass = 1.5f};
var body3 = new Body{position = p3, mass = 3.5f};

// Set up our three suns
var sun1 = world.Spawn();
sun1.Add<Force>();
sun1.Add(new Position {Value = body1.position});
sun1.Add<Velocity>();

var sun2 = world.Spawn();
sun2.Add<Force>();
sun2.Add(new Position {Value = body2.position});
sun2.Add<Velocity>();

var sun3 = world.Spawn();
sun3.Add<Force>();
sun3.Add(new Position {Value = body3.position});
sun3.Add<Velocity>();

// By adding all attractor relations to all bodies,
// they all end up in the same Archetype
// (not strictly necessary, but limits fragmentation)
sun1.Add(body1);
sun1.AddRelation(sun1, body1);
sun1.AddRelation(sun2, body2);
sun1.AddRelation(sun3, body3);

sun2.Add(body2);
sun2.AddRelation(sun1, body1);
sun2.AddRelation(sun2, body2);
sun2.AddRelation(sun3, body3);

sun3.Add(body3);
sun3.AddRelation(sun1, body1);
sun3.AddRelation(sun2, body2);
sun3.AddRelation(sun3, body3);
```

```csharp [Queries]
// Used to accumulate all forces acting on a body (from the other bodies)
var accumulator = world
    .Query<Force, Position, Body>(Match.Plain, Match.Plain, Match.Entity)
    .Compile();

// Used to calculate the the forces into the velocities and positions
var integrator = world
    .Query<Force, Velocity, Position>()
    .Compile();

// Used to copy the Position into the Body components of the same object
// (Match.plain = only the plain, non-relation components)
var consolidator = world
    .Query<Position, Body>(Match.Plain, Match.Plain)
    .Compile();        
```

```csharp [Simulation Loop]        
// Main "Loop", we pretend we run at 100 fps (dt = 0.01)

// Clear all forces
accumulator.Blit(new Force { Value = Vector3.Zero });

// Accumulate all forces through 1 Attractor stream:
// This means Query.For will enumerate each sun 3 times
// (once for each attractor relation we set up on it)
accumulator.For(static 
    (ref Force force, ref Position position, ref Body attractor) =>
    {
        var distanceSquared = Vector3.DistanceSquared(attractor.position, position.Value);
        if (distanceSquared < float.Epsilon) return; // Skip ourselves (anything that's too close)
        
        var direction = Vector3.Normalize(attractor.position - position.Value);
        force.Value += direction * attractor.mass / distanceSquared;
    });

// Integrate forces, velocities, and positions
integrator.For(static 
    (ref Force force, ref Velocity velocity, ref Position position, float dt) =>
    {
        velocity.Value += dt * forces.Value;
        position.Value += dt * velocity.Value;
    }, 0.01f);


// Copy the Position back to the Body components of the same object
// (the plain and relation components are backed by the same instances of Body!)
consolidator.For(static
    (ref Position position, ref Body body) =>
    {
        iterations3++;
        body.position = position.Value;
    });
```
:::
