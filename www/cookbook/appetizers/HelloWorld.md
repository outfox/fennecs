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

First, we define a simple `Name` component to store a string value. And let's sprinkle on a little 💫*raffinésse*💫 with implicit conversion operators! 

```csharp
internal readonly struct Name(string value)
{
    public static implicit operator Name(string value) => new(value);
    public override string ToString() => value;
}
```

The struct uses a [Primary Constructor](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors), but you can also write an old-style explicit one and an auto-property for `value`.

Next, we declate two empty `Tag` components, `Human` and `Fennec`, to label Entities semantically:

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
      .Add<Name>("Erwin");

var human1 = world.Spawn()
      .Add<Human>()
      .Add<Name>("Junno");

var human2 = world.Spawn()
      .Add<Human>()
      .Add<Name>("Avinash");
```

## Querying Entities

Now, let's create two kinds of query to find all entities with a `Name` and run it to print a greeting for each entity. We also want to only match entities with a `Fennec` - in our case we can do it by inclusion or exclusion.

```csharp
var noHumans = world.Query<Name>()
    .Not<Human>()
    .Compile();

noHumans.For(static (ref Name name) =>
{
    Console.WriteLine($"Hello, {name}!");
});
```
... and just as well ...

```csharp
var onlyFennecs = world.Query<Name>()
    .Has<Fennec>()
    .Compile();

onlyFennecs.For(static (ref Name name) =>
{
    Console.WriteLine($"Hello, {name}!");
});

```

After all, this is **fenn**ecs, not, ***human ecs***.


## Expected Output

When you run this code, you should see the following output:

```
Hello, Erwin!
Hello, Erwin!
```
----------

*"Huh, **human ecs**! I like it."* :neofox_googly: 
