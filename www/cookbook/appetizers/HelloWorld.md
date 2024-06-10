---
title: 0. Hello! (Basics)
order: 0
head:
  - - meta
    - name: description 
      content: Beginner Cookbook for fennecs, the tiny, tiny, high-energy Entity-Component System
---

# Say hello to **fenn**ecs!

Let's start with a simple "Hello, World!" example to get you familiar with the basics of **fenn**ecs.

* defining Components
* spawning Entities
* compiling Queries (and using them!)


## Defining Components

First, we define a simple `Name` component to store a string value. And let's sprinkle on a little 💫 *raffinésse* 💫 with a conversion operator... (it's a surprise tool that will help us later!)

```csharp
internal readonly struct Name(string value)
{
    public static implicit operator Name(string value) => new(value);
    public override string ToString() => value;
}
```
::: details Weird syntax? 
The struct uses a [Primary Constructor](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors), but you can also write an old-style explicit one and an auto-property for `value`; or a public field.
:::

Next, we declare two empty `Tag` components that will act as labels, `Human` and `Fennec`, to tag our Entities semantically:

```csharp
internal readonly struct Human;
internal readonly struct Fennec;
```

## Creating Entities

Next, we create a new `World` to hold our entities and components, and spawn a few Entities with a `Name` component, and with tags to denote their species:

```csharp
var world = new World();

var fennec = world.Spawn()
      .Add<Fennec>()
      .Add<Name>("Erwin"); // Erwin is a Fennec!

var human1 = world.Spawn()
      .Add<Human>()
      .Add<Name>("Junno"); // Conversion operator makes these so readable!

var human2 = world.Spawn()
      .Add<Human>()
      .Add<Name>("Avinash"); // Feels good!
```

## Querying Entities

Now, let's create two kinds of query to find all entities with a `Name` and run it to print a greeting for each entity. We also want to only match entities with a `Fennec` - in our case we can do it by inclusion or exclusion.

### :neofox_floof_mug_reverse: *"Aww! This is **fenn**ecs, not ***human ecs***!"*

```csharp{2}
var noHumans = world.Query<Name>()
    .Not<Human>()
    .Stream();

noHumans.For(static (ref Name name) =>
{
    Console.WriteLine($"Hello, {name}!");
});
```

### *"Hmm, **human ecs**?! ... I like it."* :neofox_uwu_nod.gif: 

```csharp{2}
var onlyFennecs = world.Query<Name>()
    .Has<Fennec>()
    .Stream();

onlyFennecs.For(static (ref Name name) =>
{
    Console.WriteLine($"Hello, {name}!");
});

```

## Expected Output

When you run this code, you should see the following output:

```
Hello, Erwin!
Hello, Erwin!
```
----------

### :neofox_think_anime: *"**Only**Fennecs... now there's an idea for a web search."*

