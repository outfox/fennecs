// InitialD.cs (type declarations at bottom of file)

using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();
Console.WriteLine("Shusai: fuji hara tofu ten");

var world = new World();

// The Driver
var takumi = new Driver("Takumi Fujiwara");

// The Eight-Six
var ae86 = world.Spawn()
    .Add<Car>()
    .Add<Model>("Toyota Sprinter Trueno AE86")
    .Add(takumi) // Add<Driver> reference type Component
    .Add(new Engine // Add<Engine> value type Component
    {
        Horsepower = 130, Torque = 149,
    });

Console.WriteLine($"Look, {takumi} is driving his dad's\n{ae86}");


// All cars in the race.
var racers =
    world.Query<Driver, Model>() // "Stream Types", data to process
        .Has<Car>() // additional Filter Expression(s) to match in the Query 
        .Not<Vroom>() // additional Filter Expression(s) to exclude
        .Stream();


Console.WriteLine($"Cars on the street: {racers.Count}");

// Drivers, get ready! (mutative per-Entity operation on Query)
racers.For((in Entity raceCar, ref Driver driver, ref Model name) =>
{
    driver.ReportForRace();
    Console.WriteLine($"{driver}'s {name} is ready to race!");
    raceCar.Add<Ready>();
});

// Adding Component conditionally outside a Query runner
if (ae86.Has<Ready>()) ae86.Add<Steady>();

// Or do bulk operations on the Query!
racers.Query.Add<Vroom>();

Console.WriteLine("Got 60 seconds to spare?");
Console.WriteLine("--> https://behance.net/gallery/101574771/D-CG-Animation");


#region Components
// "tags", size-less Components we use to mark & classify Entities
internal struct Car;

internal struct Ready;

internal struct Steady;

internal struct Vroom;


// data Component
internal struct Engine
{
    public float Horsepower;
    public float Torque;
}


// data Component wrapping another type, here: a string
internal readonly struct Model(string value)
{
    public static implicit operator Model(string value) => new(value);
    public override string ToString() => value;
}


// a class Component, here also wrapping a string
internal class Driver(string name)
{
    public void ReportForRace() => Console.WriteLine($"{name}: I'm Ready!");
    public override string ToString() => name;
}
#endregion