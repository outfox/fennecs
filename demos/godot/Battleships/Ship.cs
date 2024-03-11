using System;
using System.Collections.Generic;
using Godot;
using Vector2 = System.Numerics.Vector2;


namespace fennecs.demos.godot.Battleships;

public partial class Ship : Sprite2D
{
	private const int FactionCount = 4;
	
	
	[Export] public Sprite2D[] Guns;
	
	[Export] internal float Speed = 100f;

	[Export] internal int Faction;
	
	
	private Entity _entity;
	private readonly List<Gun> _guns = [];
	
	public override void _Ready()
	{
		base._Ready();

		var demo = GetParent<BattleShipsDemo>();

		Faction = Random.Shared.Next(FactionCount);
		var hull = GetNode<Sprite2D>("Hull");
			
		hull.Modulate = Color.FromOkHsl(Faction/(float)FactionCount, 0.9f, .9f, 1);

		_entity = demo.World.Spawn();
		_entity.Add(this);
		_entity.Add(new MotionState {Course = Transform.Rotation, Position = new Vector2(GlobalPosition.X, GlobalPosition.Y), Speed = Speed});
		
		_entity.Add<Targeting>();

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