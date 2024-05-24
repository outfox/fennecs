using System.Numerics;

namespace fennecs.demos.godot;

public struct Velocity : Fox<Vector2>
{
    public Vector2 Value { get; set; }
}

public struct Acceleration : Fox<Vector2>
{
    public Vector2 Value { get; set; }
}

public struct Position : Fox<Vector2>
{
    public Vector2 Value { get; set; }
}

public partial class Body
{
    public System.Numerics.Vector2 position;
    public float mass { init; get; }
}
