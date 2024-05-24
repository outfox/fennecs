using Godot;
using Vector2 = System.Numerics.Vector2;

namespace fennecs.demos.godot;

[GlobalClass]
public partial class NBodyDemo : Node2D
{
	// We just use a shared singleton here for ease of use.
	private static World world => EntityNode2D.World;

	private Query<Acceleration, Body, Body> _accumulator;
	private Query<Acceleration, Velocity, Position> _integrator;
	private Query<Position, Body, StellarBody> _consolidator;

	public override void _Ready()
	{
		// Used to accumulate all forces acting on a body from the other bodies
		// (the plain and relation Body Stream Components are backed by the same object!)
		_accumulator = world
			.Query<Acceleration, Body, Body>(Match.Plain, Match.Plain, Match.Entity)
			.Compile();

		// Used to calculate the the forces into the velocities and positions
		_integrator = world.Query<Acceleration, Velocity, Position>().Compile();

		// Used to copy the Position into the Body components of the same object (plain = non-relation component)
		_consolidator = world.Query<Position, Body, StellarBody>(Match.Plain, Match.Plain, Match.Plain).Compile();
	}

	// Main simulation "Loop"
	public override void _Process(double delta)
	{
		// Clear all forces
		_accumulator.Blit(new Acceleration());

		// Accumulate all forces (we have only 1 attractor stream, this will enumerate each sun 3 times)
		_accumulator.For((ref Acceleration acc, ref Body self, ref Body attractor) =>
		{
			if (self == attractor) return; // (we are not attracted to ourselves)

			var distanceSquared = Mathf.Max(0.0005f, Mathf.Pow(Vector2.DistanceSquared(attractor.position, self.position), 0.75f) / 100000f);
			var direction = Vector2.Normalize(attractor.position - self.position);
			acc.Value += direction * attractor.mass / distanceSquared / self.mass;
		});

		// Integrate accelerations, velocities, and positions
		_integrator.For((ref Acceleration accel, ref Velocity velocity, ref Position position, float dt) =>
		{
			velocity.Value += dt * accel.Value;
			position.Value += dt * velocity.Value;
		}, (float) delta);

		// Copy the Position back to the Body components of the same object
		// (the plain and relation components are backed by the same instances of Body!)
		_consolidator.For((ref Position position, ref Body body, ref StellarBody node) =>
		{
			body.position = position.Value;
			node.Position = new(position.Value.X, position.Value.Y);
		});
	}
}
