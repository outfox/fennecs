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
	// Easy route: only make new entity if not alive.
	public override void _EnterTree()
	{
		if (GD.Randf() > spawnChance)
		{
			QueueFree();
			return;
		}
		
		// Entity structs are truthy if alive / falsy otherwise
		if (entity) return;

		entity = World.Spawn();
		// "Our" entity has "us ourselves" as a component.
		// This makes it possible for queries and other code to
		// access the node - e.g. via entity.Ref<EntityNode3D>()
		// or world.Query<EntityNode3D>()!
		entity.Add(this);
	}

	// This is an ok place to handle the final deletion of the entity.
	protected override void Dispose(bool disposing)
	{
		//if (disposing) entity.Despawn();
		base.Dispose(disposing);
	}

	public static string DebugInfo() => World.ToString();
}
