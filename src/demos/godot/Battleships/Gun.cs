using System;
using System.Collections.Generic;
using Godot;

namespace fennecs.demos.godot.Battleships;

public partial class Gun : Sprite2D
{
	[Export] public float Range = 800f;

	[Export] public float ReloadTime = 2f;

	/// <summary>Turret traverse speed in radians per second.</summary>
	[Export] public float Turning = 2f;

	[Export] public int Damage = 1;

	[Export] public float BulletSpeed = 600f;

	/// <summary>Aiming error per shot, in degrees — misses are what make a naval battle.</summary>
	[Export] public float SpreadDegrees = 2.5f;

	/// <summary>Traverse limits relative to the turret's mounting, in radians.</summary>
	[Export] public Vector2 FiringArc = new(-float.Pi * 2f / 3f, float.Pi * 2f / 3f);

	public float Cooldown;
	public float Recoil;

	internal Ship OwnerShip = null!; // assigned by the owning Ship in its _Ready
	internal float MountRotation;
	internal Vector2 BaseOffset;
	internal float BarrelLength;
	internal readonly List<Node2D> Muzzles = [];

	public override void _Ready()
	{
		base._Ready();

		MountRotation = Rotation;
		BaseOffset = Offset;
		BarrelLength = (Texture?.GetSize().X ?? 24f) * 0.6f;

		foreach (var child in GetChildren())
		{
			if (child is Node2D node && node.Name.ToString().StartsWith("Muzzle")) Muzzles.Add(node);
		}

		// Stagger the opening salvo so broadsides ripple instead of firing as one.
		Cooldown = ReloadTime * Random.Shared.NextSingle();
	}
}
