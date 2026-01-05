using System;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace fennecs.demos.godot;

[GlobalClass]
public partial class NBodyDemo : Node2D
{
	// We just use a shared singleton here for ease of use.
	private static World world => EntityNode2D.World;

	private Stream<Acceleration, Body, Body> _accumulator;
	private Stream<Acceleration, Velocity, Position> _integrator;
	private Stream<Position, Body, StellarBody> _consolidator;

	public override void _Ready()
	{
		// Used to accumulate all forces acting on a body from the other bodies
		// (the plain and relation Body Stream Components are backed by the same object!)
		_accumulator = world.Query<Acceleration, Body, Body>(Match.Plain, Match.Plain, Entity.Any).Stream();

		// Used to calculate the the forces into the velocities and positions
		_integrator = world.Query<Acceleration, Velocity, Position>(Match.Plain, Match.Plain, Match.Plain).Stream();

		// Used to copy the Position into the Body Components of the same object (plain = non-relation Component)
		_consolidator = world.Query<Position, Body, StellarBody>(Match.Plain, Match.Plain, Match.Plain).Stream();

		world.GC();
	}

	public override void _PhysicsProcess(double delta)
	{
		// #region Showcase
		// Clear all forces
		_accumulator.Blit(new Acceleration());

		// Accumulate all forces (we have only 1 attractor stream, this will enumerate each sun 3 times)
		_accumulator.For((ref Acceleration acc, ref Body self, ref Body attractor) =>
		{
			if (self == attractor) return; // (we are not attracted to ourselves)

			var distanceSquared = Mathf.Max(0.001f, MathF.Pow(Vector2.DistanceSquared(attractor.position, self.position), 0.75f) / 100000f);
			var direction = Vector2.Normalize(attractor.position - self.position);
			acc.Value += direction * attractor.mass / distanceSquared / self.mass;

			// Just a pinch of dark matter to keep things together a bit more
			acc.Value -= self.position * 0.005f / self.mass;
		});

		// Integrate accelerations, velocities, and positions
		_integrator.For(
		uniform: (float)delta,
		action: static (float dt, ref Acceleration accel, ref Velocity velocity, ref Position position) =>
		{
			velocity.Value += dt * accel.Value;
			position.Value += dt * velocity.Value;
		});
		// #endregion Showcase

		// Copy the Position back to the Body Components of the same object
		// (the plain and relation Components are backed by the same instances of Body!)
		_consolidator.For((ref Position position, ref Body body, ref StellarBody node) =>
		{
			body.position = position.Value;
			node.Position = new(position.Value.X, position.Value.Y);
		});
	}
}
