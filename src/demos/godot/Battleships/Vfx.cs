using Godot;
using NVec2 = System.Numerics.Vector2;

namespace fennecs.demos.godot.Battleships;

public enum VfxKind
{
	MuzzleFlash,
	Hit,
	Splash,
	Smoke,
	Explosion,
}

/// <summary>
/// One transient battlefield effect — a muzzle flash, shell splash, smoke
/// puff, or explosion. Like <see cref="Projectile"/>, these are pure
/// component data: each kind renders as one instance in a shared MultiMesh,
/// with its animation frame chosen per-instance in the shader.
/// </summary>
public struct Vfx
{
	public VfxKind Kind;
	public NVec2 Position;
	public NVec2 Velocity;
	public float Rotation;
	public float Spin;
	public float Scale;    // world size in pixels
	public float Growth;   // pixels per second
	public float Age;      // negative = fuse delay before the effect appears
	public float Lifetime;
	public Color Tint;
}
