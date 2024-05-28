---
title: EntityNode
outline: [2, 3]
---

# Associating an Entity with a Node
One approach of using **fenn**ecs with Godot is to create a system that is orthogonal to Godot's hierarchical composition. Only a fraction of what lives in Godot usually matters to the ECS side of your project, and vice versa.

An easy and intuitive workflow is to associate Entities with Nodes as you set them up. This way, you can use the ECS to manage the game state and the Godot scene tree to manage the visuals and interactions.

### Principles
- as a Node enters the tree for the first, it creates an Entity for itself
- as it later enters or exits the tree, it can flag its Entity as unused if needed
- as it is freed, it can Despawn the Entity



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

::: warning :neofox_sad: SORRY: DIAMOND INHERITANCE
Since our component type structure would form a parallel type chain next to Godot's, you likely have to implement this sort of class for each node you wish to associate with an Entity.

> e.g., you couldn't derive from a `EntityNode3D` if you also wanted to inherit from a `CollisionShape3D`, so you'd need to write an `EntityCollisionShape3D` under some circumstances.

This can be mitigated using C# 13's extension types.
:::

