using Godot;

namespace fennecs.demos.godot.Battleships;

public partial class 
	
	Bullet : Node2D
{
	public Vector2 Velocity;
	public Admiralty Faction;
	public int Damage;
	public float Life;

	internal Entity Entity;

	public override void _Draw()
	{
		var color = Faction?.Color ?? Colors.White;
		DrawCircle(Vector2.Zero, 3f, color);
	}
}
