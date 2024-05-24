using System.Collections.Generic;
using Godot;

namespace fennecs.demos.godot.Battleships;

[GlobalClass]
public partial class BattleShipsDemo : Node2D
{
	[Export] public int FactionCount = 4;

	public readonly World World = new();

	private double _fps = 120;

	internal Dictionary<int, HashSet<SpatialClient>> SpatialHash = new();

	public override void _Process(double delta)
	{
		_fps = _fps * 0.99 + 0.01 * (1.0/delta);

		var dt = (float) delta;

		var ships = World.Query<Ship, MotionState>().Compile();
		ships.For((ref Ship ship, ref MotionState motion) =>
		{
			var direction = System.Numerics.Vector2.UnitX;
			direction = System.Numerics.Vector2.Transform(direction, System.Numerics.Matrix3x2.CreateRotation(motion.Course));
			motion.Position += motion.Speed * dt * direction;

			ship.GlobalPosition = new Vector2(motion.Position.X, motion.Position.Y);
			ship.Rotation = motion.Course;
		});


		var guns = World.Query<Gun>().Compile();
		var pos = GetGlobalMousePosition();
		guns.For((ref Gun gun, Vector2 aim) =>
		{
			gun.Aim = aim;
			gun.LookAt(gun.Aim);
		}, pos);


		GetNode<Label>("Ui Layer/Label").Text = $"Ships: {ships.Count} Guns: {guns.Count}\n FPS {Mathf.RoundToInt(_fps)}";
	}
}
