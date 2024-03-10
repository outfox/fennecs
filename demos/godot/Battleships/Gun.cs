using Godot;

namespace fennecs.demos.godot.Battleships;

public partial class Gun : Sprite2D
{
    public float CoolDown = 1f;
    public Vector2 Aim = Vector2.Zero;
}