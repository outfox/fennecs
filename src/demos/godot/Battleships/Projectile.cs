using Godot;
using NVec2 = System.Numerics.Vector2;

namespace fennecs.demos.godot.Battleships;

/// <summary>
/// A shell in flight. Pure component data — no Node, no scene tree entry.
/// Thousands of these live only in the World and are drawn in a single
/// MultiMesh batch each frame.
/// </summary>
public struct Projectile
{
	public NVec2 Position;
	public NVec2 Velocity;
	public float Life;
	public float MaxLife;
	public int Damage;
	public Admiralty Faction;
	public Color Color;
}
