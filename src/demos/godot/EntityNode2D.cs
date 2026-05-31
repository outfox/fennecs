using Godot;

namespace fennecs.demos.godot;

[GlobalClass]
public partial class EntityNode2D : Node2D
{
	[Export]
	public float spawnChance = 1.0f;
	
	// If you want multiple worlds, create an Autoload and get the World from there.
	internal protected static readonly World World = new(1_000_000);

	// This is the Entity associated with this node.
	// Protected so derived classes can work with it, as well!
	protected Entity entity;

	// Unfortunately, _EnterTree can happen repeatedly.
	// Easy route: only make new Entity if not alive.
	public override void _EnterTree()
	{
		if (GD.Randf() > spawnChance)
		{
			QueueFree();
			return;
		}
		
		// Already got an Entity? Don't make a new one.
		if (entity.Alive) return;

		entity = World.Spawn();
		// "Our" Entity has "us ourselves" as a Component.
		// This makes it possible for queries and other code to
		// access the node - e.g. via Entity.Ref<EntityNode3D>()
		// or world.Query<EntityNode3D>()!
		entity.Add(this);
	}

	// This is an ok place to handle the final deletion of the Entity.
	protected override void Dispose(bool disposing)
	{
		//if (disposing) Entity.Despawn();
		base.Dispose(disposing);
	}

	public static string DebugInfo() => World.ToString();
}
