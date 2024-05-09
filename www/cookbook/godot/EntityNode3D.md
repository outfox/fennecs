---
title: EntityNode3D
outline: [2, 3]
---

# Associating an Entity with a Node

```csharp
using fennecs;
using Godot;

[GlobalClass]
public partial class EntityNode3D : Node3D
{
	// This is the Entity associated with this node.
	// Protected so derived classes can work with it, as well!
	protected Entity entity;
	
	// Unfortunately, _EnterTree can happen repeatedly.		
	// Easy route: only make new entity if not alive.
	public override void _EnterTree()
	{
		// Entity structs are truthy if alive / falsy otherwise
		if (!entity) 
		{
			// This is one way to get the world instance in a node:
			// - create an AutoLoad (here named ECS) that contains the world instance
			// - get the world instance either from the AutoLoad or a Singleton
			// - alternative 1: get it from the parent node, root node, export, etc.		
			// - alternative 2: a resource that has some runtime state?
			var world = GetNode<ECS>("/root/ECS").world;
			
			entity = world.Spawn();
		}

		// "Our" entity has "us ourselves" as a component.
		// This makes it possible for queries and other code to
		// access the node - e.g. via entity.Ref<EntityNode3D>()
		// or world.Query<EntityNode3D>()!
		entity.Add(this);
	}
	
	public override void _ExitTree()
	{
		entity.Remove<EntityNode3D>();
	}

	// This is an ok place to handle the final deletion of the entity.
	protected override void Dispose(bool disposing)
	{
		if (disposing) entity.Despawn();
		base.Dispose(disposing);
	}
}
```