using Godot;

namespace fennecs.demos.godot.Battleships;

public partial class Objective : Node2D
{
	public const float CaptureTime = 10f;

	[Export] public float Radius = 500f;

	public float Timer;

	public Admiralty Controller;
}
