using Godot;


namespace fennecs.demos.godot.Battleships;

public partial class Ship : Sprite2D
{
	[Export] public Sprite2D[] Guns;

	public override void _Ready()
	{
		base._Ready();
		//TODO: Hook up to ECS world.
	}
}
