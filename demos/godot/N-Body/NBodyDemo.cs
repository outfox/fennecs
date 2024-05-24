using System.Collections.Generic;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace fennecs.demos.godot;

public partial class NBodyDemo : Node
{
	// We just use a shared singleton here for ease of use.
	private static World world => EntityNode3D.World;
	
	private Query<Acceleration, Body, Body> _accumulator;
	private Query<Acceleration, Velocity, Position> _integrator;
	private Query<Position, Body> _consolidator;

	public override void _Ready()
	{
		// Collect all bodies and set up the relationships
		using var bodies = world.Query<Body>().Unique();
		
		
		//TODO: Just getting all the components here would be amazing.
		
		
		// By adding all attractor relations to all bodies,
		// they all end up in the same Archetype (not strictly necessary)
		var p1 = new Vector2(-10, -4);
		var p2 = new Vector2(0, 12);
		var p3 = new Vector2(7,  4);

		var body1 = new Body { position = p1, mass = 2.0f };
		var body2 = new Body { position = p2, mass = 1.5f };
		var body3 = new Body { position = p3, mass = 3.5f };

		var sun1 = world.Spawn();
		sun1.Add<Acceleration>();
		sun1.Add(new Position { Value = body1.position });
		sun1.Add<Velocity>();

		var sun2 = world.Spawn();
		sun2.Add<Acceleration>();
		sun2.Add(new Position { Value = body2.position });
		sun2.Add<Velocity>();

		var sun3 = world.Spawn();
		sun3.Add<Acceleration>();
		sun3.Add(new Position { Value = body3.position });
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
		// (the plain and relation Body Stream Components are backed by the same object!)
		_accumulator = world
			.Query<Acceleration, Body, Body>(Match.Plain, Match.Plain, Match.Entity)
			.Compile();

		// Used to calculate the the forces into the velocities and positions
		_integrator = world.Query<Acceleration, Velocity, Position>().Compile();

		// Used to copy the Position into the Body components of the same object (plain = non-relation component)
		_consolidator = world.Query<Position, Body>(Match.Plain, Match.Plain).Compile();

	}

	// Main simulation "Loop"
	private void _Process(float delta)
	{
		// Clear all forces
		_accumulator.Blit(new Acceleration { Value = Vector2.Zero });

		// Accumulate all forces (we have only 1 attractor stream, this will enumerate each sun 3 times)
		_accumulator.For((ref Acceleration acc, ref Body self, ref Body attractor) =>
		{
			if (self == attractor) return; // (we are not attracted to ourselves)

			var distanceSquared = Vector2.DistanceSquared(attractor.position, self.position);
			var direction = Vector2.Normalize(attractor.position - self.position);
			acc.Value += direction * attractor.mass / distanceSquared / self.mass;
		});

		// Integrate accelerations, velocities, and positions
		_integrator.For((ref Acceleration accel, ref Velocity velocity, ref Position position, float dt) =>
		{
			velocity.Value += dt * accel.Value;
			position.Value += dt * velocity.Value;
		}, delta);

		// Copy the Position back to the Body components of the same object
		// (the plain and relation components are backed by the same instances of Body!)
		_consolidator.For((ref Position position, ref Body body) =>
		{
			body.position = position.Value;
		});
	}
}
