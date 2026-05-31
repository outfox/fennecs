using Godot;

namespace fennecs.demos.godot;

[GlobalClass]
public partial class EntityNode3D : Node3D
{
    // If you want multiple worlds, create an Autoload and get the World from there.
    internal protected static readonly World World = new(1_000_000);
    
    // This is the Entity associated with this node.
    // Protected so derived classes can work with it, as well!
    protected Entity entity;
	
    // Unfortunately, _EnterTree can happen repeatedly.		
    // Easy route: only make new Entity if not alive.
    public override void _EnterTree()
    {
        // Already got an Entity? Don't make a new one.
        if (!entity.Alive) 
        {
            entity = World.Spawn();
        }

        // "Our" Entity has "us ourselves" as a Component.
        // This makes it possible for queries and other code to
        // access the node - e.g. via Entity.Ref<EntityNode3D>()
        // or world.Query<EntityNode3D>()!
        entity.Add(this);
    }
	
    public override void _ExitTree()
    {
        entity.Remove<EntityNode3D>();
    }

    // This is an ok place to handle the final deletion of the Entity.
    protected override void Dispose(bool disposing)
    {
        if (disposing) entity.Despawn();
        base.Dispose(disposing);
    }
}