---
title: OOP & Inheritance
outline: [2, 3]
---

# Object Oriented Programming

Traditionally, ECS are about composition over inheritance. But what if we told you that inheritance can be used to great effect with **fenn**ecs?

It can be especially useful when interacting with inheritance-heavy engine hierarchies, for example Godot's Node types and Resources, or Unity's Components and ScriptableObjects.

## Inheritance

The main way to use OOP is by adding components once repeatedly with TypeExpressions based on their base class. 

```csharp
public class BaseComponent { }
public class DerivedComponent : BaseComponent { }

using var world = new World();
var entity = world.Spawn();

var component = new DerivedComponent();

entity.Add<BaseComponent>(component);
entity.Add<DerivedComponent>(component);

// Reachable in various ways (and with two type identities)!
var queryBase = world.Query<BaseComponent>();
var queryDerived = world.Query<DerivedComponent>();
var queryBoth = world.Query<BaseComponent, DerivedComponent>();
```

It means you don't get the advantages (nor disadvantages!) of polymorphism unless you explicitly ask (and Query!) for it.

## Methods

You can also use inheritance to add methods to your components that operate on their data, and you can use Inheritance and Polymorphism to bind and invoke the appropriate methods as needed, by specifynign a base class or interface as the Component type.

This is a great way to keep your code DRY and your components focused on their responsibilities without having to offload their details to your systems code that runs on the queries.

C# 13 brings Type Extensions, which will allow you to add methods to existing types as well.