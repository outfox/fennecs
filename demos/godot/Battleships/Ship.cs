using System;
using System.Collections.Generic;
using Godot;
using Vector2 = System.Numerics.Vector2;


namespace fennecs.demos.godot.Battleships;

public partial class Ship : Sprite2D
{
	[Export] public Sprite2D[] Guns;
	
	[Export] internal float Speed = 100f;
	
	public Admiralty Faction;
	
	
	private Entity _entity;
	private readonly List<Gun> _guns = [];
	
	public override void _Ready()
	{
		base._Ready();

		WeakReference<GodotObject> wr;
		
		var demo = GetParent<BattleShipsDemo>();

		//TODO: Admiralty should instantiate its own ships.
		Faction = demo.Admiralties[Random.Shared.Next(demo.FactionCount)];
		
		var hull = GetNode<Sprite2D>("Hull");

		hull.Modulate = Faction.Color; 

		_entity = demo.World.Spawn();
		_entity.Add(this);
		_entity.Add(new MotionState {Course = Transform.Rotation, Position = new Vector2(GlobalPosition.X, GlobalPosition.Y), Speed = Speed});
		_entity.Add<Objective>();
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

public class Objective
{
	public Admiralty Owner;
	public Vector2 Position;
	public float Radius;
}

public struct Targeting
{
	private readonly List<Entity> _targets = [];
	
	public Targeting()
	{
	}
}