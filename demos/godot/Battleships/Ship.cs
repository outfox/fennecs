using System;
using System.Collections.Generic;
using Godot;
using Vector2 = System.Numerics.Vector2;


namespace fennecs.demos.godot.Battleships;

public partial class Ship : Sprite2D
{
	[Export] public Sprite2D[] Guns;

	[Export] internal float Speed = 100f;

	[Export] internal int Points = 1;

	public Admiralty Faction;


	private Entity _entity;
	private readonly List<Gun> _guns = [];

	public override void _Ready()
	{
		base._Ready();

		var demo = GetParent<godot.BattleShipsDemo>();

		var hull = GetNode<Sprite2D>("Hull");

		hull.Modulate = Faction.Color;

		_entity = demo.World.Spawn();

		// Used for grouping and targeting
		_entity.Add(Link.With(Faction));

		// This is a ship ...
		_entity.Add(this);

		// ... and its state Components
		var goal = Faction.FleetObjective.GlobalPosition + new Godot.Vector2(GD.RandRange(-500, 500), GD.RandRange(-500, 500));
		var course = GlobalPosition.AngleToPoint(goal);
		Rotation = course;
		_entity.Add(new MotionState {Course = course, Position = new Vector2(GlobalPosition.X, GlobalPosition.Y), Speed = Speed});
		_entity.Add<Objective>();
		_entity.Add<Targeting>();


		// Register the Guns as Entities, too
		foreach (var candidate in GetChildren())
		{
			if (candidate is not Gun gun) continue;
			_guns.Add(gun);

			var gunEntity = demo.World.Spawn();
			gunEntity.Add(gun);
		}
	}
}


public struct MotionState
{
	public Vector2 Position;
	public float Course;
	public float Speed;
}


public struct Targeting
{
	private readonly List<Entity> _targets = [];

	public Targeting()
	{
	}
}


public class SpatialClient
{
	public Vector2 Position;
	public float Radius;

	public int LastHash;

	private int GridX => Mathf.RoundToInt(Position.X / 100f);
	private int GridY => Mathf.RoundToInt(Position.Y / 100f);


	private Dictionary<int, HashSet<SpatialClient>> _grid;

	public void AddToGrid(Dictionary<int, HashSet<SpatialClient>> grid)
	{
		LastHash = GetHashCode();

		_grid = grid;
		_grid[GetHashCode()].Add(this);
	}

	public override int GetHashCode() => HashCode.Combine(GridX, GridY);
}
