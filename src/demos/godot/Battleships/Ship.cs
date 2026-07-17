using System;
using System.Collections.Generic;
using Godot;
using NVec2 = System.Numerics.Vector2;

namespace fennecs.demos.godot.Battleships;

public partial class Ship : Sprite2D
{
	[Export] internal float Speed = 150f;

	[Export] internal int Points = 1;

	[Export] public int MaxHealth = 5;

	[Export] public float Radius = 30f;

	[Export] public float TurnRate = 0.8f;

	public Admiralty Faction = null!; // assigned by Admiralty.AddShip before entering the tree

	public int Health;

	public bool Sinking { get; private set; }

	public NVec2 CurrentVelocity;

	// Per-ship "personality", rolled on spawn. Every captain keeps their own
	// slowly drifting station around the fleet objective and their own idea
	// of a straight line — this is what turns a convoy into a melee.
	internal float OrbitAngle;
	internal float OrbitSpeed;
	internal float OrbitRadius;
	internal float WanderPhase;
	internal float WanderFrequency;
	internal float SmokeTimer;

	// Fire control: the ship designates one target per frame and every
	// turret trains on it — set by BattleShipsDemo.AcquireTargets.
	internal int TargetIndex = -1;

	private Entity _entity;
	private readonly List<Entity> _gunEntities = [];
	private BattleShipsDemo _demo = null!;

	public override void _Ready()
	{
		base._Ready();

		Health = MaxHealth;
		_demo = GetParent<BattleShipsDemo>();

		GetNode<Sprite2D>("Hull").Modulate = Faction.Color;

		var rand = Random.Shared;
		OrbitAngle = rand.NextSingle() * Mathf.Tau;
		OrbitSpeed = (0.05f + 0.1f * rand.NextSingle()) * (rand.Next(2) == 0 ? -1f : 1f);
		OrbitRadius = 150f + 500f * rand.NextSingle();
		WanderPhase = rand.NextSingle() * Mathf.Tau;
		WanderFrequency = 0.2f + 0.5f * rand.NextSingle();

		_entity = _demo.World.Spawn();

		// Used for grouping and targeting
		_entity.Add(Link.With(Faction));

		// This is a ship ...
		_entity.Add(this);

		// ... and its state Components
		var goal = Faction.FleetObjective?.GlobalPosition ?? GlobalPosition + Vector2.Right;
		goal += new Vector2(rand.Next(-500, 500), rand.Next(-500, 500));
		var course = GlobalPosition.AngleToPoint(goal);
		Rotation = course;
		CurrentVelocity = new NVec2(MathF.Cos(course), MathF.Sin(course)) * Speed;
		_entity.Add(new MotionState { Course = course, Position = new NVec2(GlobalPosition.X, GlobalPosition.Y), Speed = Speed });

		// Register the Guns as Entities, too
		foreach (var candidate in GetChildren())
		{
			if (candidate is not Gun gun) continue;
			gun.OwnerShip = this;

			var gunEntity = _demo.World.Spawn();
			gunEntity.Add(gun);
			_gunEntities.Add(gunEntity);
		}
	}

	public void TakeDamage(int damage)
	{
		if (Sinking) return;
		Health -= damage;
		if (Health > 0) return;
		Sink();
	}

	// Ships don't just vanish — they blow apart, burn, and slip under.
	private void Sink()
	{
		Sinking = true;
		Faction?.NotifyShipLost(this);

		foreach (var gunEntity in _gunEntities) gunEntity.Despawn();

		// The ship becomes a Wreck: it keeps its Entity, Ship, and MotionState,
		// but the tag re-sorts it out of the combat queries and into the
		// wreck-drift system, which lets it coast to a stop as it goes down.
		_entity.Add<Wreck>();

		_demo.OnShipSunk(this);

		var tween = CreateTween();
		tween.SetParallel();
		tween.TweenProperty(this, "modulate", new Color(0.3f, 0.28f, 0.26f, 0f), 2.2f)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		tween.TweenProperty(this, "scale", Scale * 0.85f, 2.2f);
		tween.Chain().TweenCallback(Callable.From(() =>
		{
			_entity.Despawn();
			QueueFree();
		}));
	}
}


public struct MotionState
{
	public NVec2 Position;
	public float Course;
	public float Speed;
}


/// <summary>Tag: this ship is going down. Presence is the data.</summary>
public struct Wreck;
