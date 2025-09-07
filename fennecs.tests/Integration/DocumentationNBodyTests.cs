using System.Numerics;

namespace fennecs.tests.Integration;

public class DocumentationNBodyTests
{
    private struct Velocity : Fox<Vector3>
    {
        public Vector3 Value { get; set; }
    }

    private struct Acceleration : Fox<Vector3>
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
        public float mass { init; get; }
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
        sun1.Add<Acceleration>();
        sun1.Add(new Position {Value = body1.position});
        sun1.Add<Velocity>();

        var sun2 = world.Spawn();
        sun2.Add<Acceleration>();
        sun2.Add(new Position {Value = body2.position});
        sun2.Add<Velocity>();

        var sun3 = world.Spawn();
        sun3.Add<Acceleration>();
        sun3.Add(new Position {Value = body3.position});
        sun3.Add<Velocity>();
        
        sun1.Add(body1);
        sun1.Add(body1, sun1);
        sun1.Add(body2, sun2);
        sun1.Add(body3, sun3);
        
        sun2.Add(body2);
        sun2.Add(body1, sun1);
        sun2.Add(body2, sun2);
        sun2.Add(body3, sun3);
        
        sun3.Add(body3);
        sun3.Add(body1, sun1);
        sun3.Add(body2, sun2);
        sun3.Add(body3, sun3);

        // The match specifiers can be omitted, as there are no "Position" and "Forces" relations, only "Body"
        // var accumulator = world.Query<Forces, Position, Body>().Stream();
        
        // Used to accumulate all forces acting on a body from the other bodies
        // (the plain and relation Body Stream Components are backed by the same object!)
        var accumulator = world
            .Query<Acceleration, Body, Body>(Match.Plain, Match.Plain, Match.Entity)
            .Stream();
        
        Assert.Equal(3, accumulator.Count);
        Assert.Contains(sun1, accumulator.Query);
        Assert.Contains(sun2, accumulator.Query);
        Assert.Contains(sun3, accumulator.Query);

        // Used to calculate the the forces into the velocities and positions
        var integrator = world.Query<Acceleration, Velocity, Position>().Stream();
        
        // Used to copy the Position into the Body components of the same object (plain = non-relation component)
        var consolidator = world.Query<Position, Body>(Match.Plain, Match.Plain).Stream();
        
        const int bodyCount = 3;
        
        // Main "Loop", we pretend we run at 100 fps (dt = 0.01)
        var iterations1 = 0;
        var iterations2 = 0;
        var iterations3 = 0;
        
        // Clear all forces
        accumulator.Blit(new Acceleration { Value = Vector3.Zero });
        
        // Accumulate all forces (we have only 1 attractor stream, this will enumerate each sun 3 times)
        accumulator.For((ref acc, ref self, ref attractor) =>
        {
            iterations1++;

            if (self == attractor) return; // (we are not attracted to ourselves) 
            
            var distanceSquared = Vector3.DistanceSquared(attractor.position, self.position);
            var direction = Vector3.Normalize(attractor.position - self.position);
            acc.Value += direction * attractor.mass / distanceSquared / self.mass;
        });
        
        // NB! 3 bodies x 3 valid attractors
        Assert.Equal(bodyCount * bodyCount, iterations1);

        // Integrate forces, velocities, and positions
        integrator.For(0.01f, (dt, ref accel, ref velocity, ref position) =>
        {
            iterations2++;
            velocity.Value += dt * accel.Value;
            position.Value += dt * velocity.Value;
        });
        
        Assert.Equal(bodyCount, iterations2);

        
        // Copy the Position back to the Body components of the same object
        // (the plain and relation components are backed by the same instances of Body!)
        consolidator.For((ref position, ref body) =>
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

        var force1 = Vector3.Normalize(p2 - p1) * body2.mass / Vector3.DistanceSquared(p2, p1) / body1.mass;
        force1 += Vector3.Normalize(p3 - p1) * body3.mass / Vector3.DistanceSquared(p3, p1) / body1.mass;
        Assert.Equal(force1, sun1.Ref<Acceleration>().Value);
        
        var force2 = Vector3.Normalize(p1 - p2) * body1.mass / Vector3.DistanceSquared(p1, p2) / body2.mass;
        force2 += Vector3.Normalize(p3 - p2) * body3.mass / Vector3.DistanceSquared(p3, p2) / body2.mass;
        Assert.Equal(force2, sun2.Ref<Acceleration>().Value);
        
        var force3 = Vector3.Normalize(p1 - p3) * body1.mass / Vector3.DistanceSquared(p1, p3) / body3.mass;
        force3 += Vector3.Normalize(p2 - p3) * body2.mass / Vector3.DistanceSquared(p2, p3) / body3.mass;
        Assert.Equal(force3, sun3.Ref<Acceleration>().Value);
    }
}
