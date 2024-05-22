using System.Data.SqlTypes;
using System.Numerics;

namespace fennecs.tests.Integration;

public class DocumentationNBodyTests(ITestOutputHelper output)
{
    private struct Velocity : Fox<Vector3>
    {
        public Vector3 Value { get; set; }
    }

    private struct Forces : Fox<Vector3>
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


    private struct Mass : Fox<float>
    {
        public float Value { get; set; }
    }
    

    [Fact]
    public void ThreeBodyTest()
    {
        var world = new World();
        
        var sun1 = world.Spawn();
        sun1.Add<Forces>();
        sun1.Add(new Position {Value = new Vector3(-10, -4, 0)});
        sun1.Add<Velocity>();

        var sun2 = world.Spawn();
        sun2.Add<Forces>();
        sun2.Add(new Position {Value = new Vector3(0, 12, 0)});
        sun2.Add<Velocity>();

        var sun3 = world.Spawn();
        sun3.Add<Forces>();
        sun3.Add(new Position {Value = new Vector3(7, 0, 4)});
        sun3.Add<Velocity>();
        
        // By adding all attractor relations to all bodies,
        // they all end up in the same Archetype (not strictly necessary)
        var body1 = new Body();
        var body2 = new Body();
        var body3 = new Body();

        sun1.Add(body1);
        sun1.AddRelation(sun1, body1);
        sun1.AddRelation(sun2, body2);
        sun1.AddRelation(sun3, body3);
        
        sun2.Add(body2);
        sun2.AddRelation(sun1, body1);
        sun2.AddRelation(sun2, body2);
        sun2.AddRelation(sun3, body3);
        
        sun3.Add(body2);
        sun3.AddRelation(sun1, body1);
        sun3.AddRelation(sun2, body2);
        sun3.AddRelation(sun3, body3);

        // The match specifiers can be omitted, as there are no "Position" and "Forces" relations, only "Body"
        // var accumulator = world.Query<Forces, Position, Body>().Compile();
        
        // Used to accumulate all forces acting on a body from the other bodies
        using var accumulator = world.Query<Forces, Position, Body>(Match.Plain, Match.Plain, Match.Entity).Compile();
        
        Assert.Equal(3, accumulator.Count);
        Assert.Contains(sun1, accumulator);
        Assert.Contains(sun2, accumulator);
        Assert.Contains(sun3, accumulator);

        // Used to calculate the the forces into the velocities and positions
        using var integrator = world.Query<Forces, Velocity, Position>().Compile();
        
        // Used to copy the Position into the Body components of the same object (plain = non-relation component)
        using var consolidator = world.Query<Position, Body>(Match.Plain, Match.Plain).Compile();
        
        const int bodyCount = 3;
        
        // Main "Loop", we pretend we run at 100 fps (dt = 0.01)
        var iterations1 = 0;
        var iterations2 = 0;
        var iterations3 = 0;
        
        // Clear all forces
        accumulator.Blit(new Forces { Value = Vector3.Zero });
        
        // Accumulate all forces (we have only 1 attractor stream, this will enumerate each sun 3 times)
        accumulator.For((ref Forces forces, ref Position position, ref Body attractor, float dt) =>
        {
            iterations1++;
            forces.Value += dt * (attractor.position - position.Value) * attractor.mass / Vector3.DistanceSquared(attractor.position, position.Value);
        }, 0.01f);
        
        // NB! 3 bodies x 3 attractors
        Assert.Equal(bodyCount * bodyCount, iterations1);

        // Integrate forces, velocities, and positions
        integrator.For((ref Forces forces, ref Velocity velocity, ref Position position, float dt) =>
        {
            iterations2++;
            velocity.Value += dt * forces.Value;
            position.Value += dt * velocity.Value;
        }, 0.01f);
        
        Assert.Equal(bodyCount, iterations2);
        
        // Copy the Position into the Body components of the same object
        // (the plain and relation components are backed by the same instances of Body!)
        consolidator.For((ref Position position, ref Body body) =>
        {
            iterations3++;
            body.position = position.Value;
        });

        Assert.Equal(bodyCount, iterations3);
    }
}
