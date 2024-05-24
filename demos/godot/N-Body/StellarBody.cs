using System;
using System.Linq;
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

    public override void _Ready()
    {
        base._Ready();
        
        // Get all sibling bodies (these were all set up in _EnterTree)
        var siblings = GetParent().GetChildren().OfType<StellarBody>();
        
        // Add all attractor relations to our own entity;
        // we include ourselves - this means that all entities in
        // this star system will end up in the same Archetype.
        // It's not strictly necessary, but improves cache coherence at
        // the cost of a single reference compare and 1 additional stored
        // pointer per entity.
        foreach (var sibling in siblings)
        {
            entity.AddRelation(sibling.entity, sibling);
        }
    }
}
