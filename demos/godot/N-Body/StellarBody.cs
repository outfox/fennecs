using System;
using Godot;

namespace fennecs.demos.godot;

[GlobalClass]
public partial class StellarBody : EntityNode3D
{
    [Export]
    public float mass = 1.0f;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        
        entity.Add(new Position { Value = new(Position.X, Position.Y) });
        entity.Add(new Velocity { Value = new(Random.Shared.NextSingle(), Random.Shared.NextSingle()) });
        entity.Add(new Acceleration { Value = new(0, 0) });
        entity.Add(new Body { mass = mass });
    }
}
