using Godot;

namespace fennecs.demos.godot.Battleships;

public partial class Objective : Node2D
{
	public const float CaptureTime = 10f;
	public float Timer;

	public Admiralty Owner;
	public float Radius;
}
