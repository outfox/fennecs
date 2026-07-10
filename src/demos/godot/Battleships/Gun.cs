using Godot;

namespace fennecs.demos.godot.Battleships;

public partial class Gun : Sprite2D
{
	public Vector2 Aim = Vector2.Zero;

	[Export] public float Range = 800f;

	[Export] public float ReloadTime = 2f;

	[Export] public float Turning = 1f;

	[Export] public int Damage = 1;

	[Export] public float BulletSpeed = 600f;

	[Export]
	public Vector2 FiringArc = new(-float.Pi * 2f / 3f, float.Pi * 2f / 3f);

	public float Cooldown;

	internal Ship OwnerShip;
}
