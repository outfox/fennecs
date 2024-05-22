using System.Data.SqlTypes;
using System.Numerics;

namespace fennecs.tests.Integration;

public class DocumentationNBodyTests(ITestOutputHelper output)
{
    private struct Velocity : Fox<Vector3>
    {
        public Vector3 Value { get; set; }
    }

    private struct Force : Fox<Vector3>
    {
        public Vector3 Value { get; set; }
    }

    private struct Position : Fox<Vector3>
    {
        public Vector3 Value { get; set; }
    }

    private class Body
    {
        public Vector3 position;
        public float mass = 1f;
    }
    

    [Fact]
    public void ThreeBodyTest()
    {
        var world = new World();
        
        // By adding all attractor relations to all bodies,
        // they all end up in the same Archetype (not strictly necessary)
        var p1 = new Vector3(-10, -4, 0);
        var p2 = new Vector3(0, 12, 0);
        var p3 = new Vector3(7, 0, 4);
        
        var body1 = new Body{position = p1, mass = 2.0f};
        var body2 = new Body{position = p2, mass = 1.5f};
        var body3 = new Body{position = p3, mass = 3.5f};

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

        // The match specifiers can be omitted, as there are no "Position" and "Forces" relations, only "Body"
        // var accumulator = world.Query<Forces, Position, Body>().Compile();
        
        // Used to accumulate all forces acting on a body from the other bodies
        using var accumulator = world.Query<Force, Position, Body>(Match.Plain, Match.Plain, Match.Entity).Compile();
        
        Assert.Equal(3, accumulator.Count);
        Assert.Contains(sun1, accumulator);
        Assert.Contains(sun2, accumulator);
        Assert.Contains(sun3, accumulator);

        // Used to calculate the the forces into the velocities and positions
        using var integrator = world.Query<Force, Velocity, Position>().Compile();
        
        // Used to copy the Position into the Body components of the same object (plain = non-relation component)
        using var consolidator = world.Query<Position, Body>(Match.Plain, Match.Plain).Compile();
        
        const int bodyCount = 3;
        
        // Main "Loop", we pretend we run at 100 fps (dt = 0.01)
        var iterations1 = 0;
        var iterations2 = 0;
        var iterations3 = 0;
        
        // Clear all forces
        accumulator.Blit(new Force { Value = Vector3.Zero });
        
        // Accumulate all forces (we have only 1 attractor stream, this will enumerate each sun 3 times)
        accumulator.For((ref Force force, ref Position position, ref Body attractor) =>
        {
            iterations1++;

            var distanceSquared = Vector3.DistanceSquared(attractor.position, position.Value);
            if (distanceSquared < float.Epsilon) return; // Skip ourselves (anything that's too close)
            
            var direction = Vector3.Normalize(attractor.position - position.Value);
            force.Value += direction * attractor.mass / distanceSquared;
        });
        
        // NB! 3 bodies x 3 valid attractors
        Assert.Equal(bodyCount * bodyCount, iterations1);

        // Integrate forces, velocities, and positions
        integrator.For((ref Force forces, ref Velocity velocity, ref Position position, float dt) =>
        {
            iterations2++;
            velocity.Value += dt * forces.Value;
            position.Value += dt * velocity.Value;
        }, 0.01f);
        
        Assert.Equal(bodyCount, iterations2);

        
        // Copy the Position back to the Body components of the same object
        // (the plain and relation components are backed by the same instances of Body!)
        consolidator.For((ref Position position, ref Body body) =>
        {
            iterations3++;
            body.position = position.Value;
        });

        Assert.Equal(bodyCount, iterations3);
        
        var pos1 = sun1.Ref<Position>().Value;
        Assert.Equal(body1.position, pos1);
        Assert.NotEqual(p1, pos1);

        var pos2 = sun2.Ref<Position>().Value;
        Assert.Equal(body2.position, pos2);
        Assert.NotEqual(p2, pos2);
        
        var pos3 = sun3.Ref<Position>().Value;
        Assert.Equal(body3.position, pos3);
        Assert.NotEqual(p3, pos3);
    }
}
