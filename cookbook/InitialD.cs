// InitialD.cs (type declarations at bottom of file)

using fennecs;

Console.WriteLine("Midnight, somewhere in Japan.");

// Shusai: fuji hara tōfu ten
var world = new World();

// The Driver
var takumi = new Driver("Takumi Fujiwara");

// The Eight-Six
var ae86 = world.Spawn()
    .Add<Car>()
    .Add<Model>("Toyota Sprinter Trueno AE86")
    .Add(takumi) // Add<Driver> reference type component
    .Add(new Engine // Add<Engine> value type component
    {
        Horsepower = 130, Torque = 149,
    });


// All cars in the race.
var racers =
    world.Query<Driver, Model, Identity>() // "Stream Types", the data we want to process
        .Has<Car>() // additional Filter Expression(s) to match in the Query 
        .Not<Vroom>() // additional Filter Expression(s) to exclude from the Query
        .Build();


Console.WriteLine($"Cars on the street: {racers.Count}");

// Drivers, get ready! (mutative per-entity operation on Query)
racers.For((ref Driver driver, ref Model name, ref Identity carIdentity) =>
{
    driver.ReportForRace();
    Console.WriteLine($"{driver}'s {name} is ready to race!");
    world.On(carIdentity).Add<Ready>();
});

// Adding component conditionally outside a Query runner
if (ae86.Has<Ready>()) ae86.Add<Steady>();

// Or do bulk operations on the Query!
racers.Add<Vroom>();

Console.WriteLine("Watch https://www.behance.net/gallery/101574771/D-CG-Animation (60 second fan vid)");


#region Components
// "tags", size-less components we use to mark & classify entities
internal struct Car;

internal struct Ready;

internal struct Steady;

internal struct Vroom;


// data component
internal struct Engine
{
    public float Horsepower;
    public float Torque;
}


// data component wrapping another type, here: a string
internal readonly struct Model(string value)
{
    public static implicit operator Model(string value) => new(value);
    public override string ToString() => value;
}


// a class component, here also wrapping a string
internal class Driver(string name)
{
    public void ReportForRace() => Console.WriteLine($"{name}: Christmas is so boring.");
    public override string ToString() => name;
}
#endregion