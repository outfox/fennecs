// InitialD.cs (type declarations at bottom of file)

using fennecs;

// shusai: fuji hara tōfu ten
var world = new World();

// The Driver
var takumi = new Driver("Takumi Fujiwara");

// The Eight-Six
var ae86 = world.Spawn()
    .Add<Car>()
    .Add<Name>("Toyota Sprinter Trueno AE86")
    .Add(takumi) // Add<Driver>
    .Add(new Engine // Add<Engine>
    {
        Horsepower = 130, Torque = 149,
    });
 


// All cars - driver check!
var query = 
    world.Query<Driver, Name>()  // "Stream Types", the data we want to process
    .Has<Car>()                  // additional Filter Expression(s) to match 
    .Not<Ready>()                // in the Query in addition to all Stream Types
    .Build();

query.For((ref Driver driver, ref Name name) =>
{
    driver.ReportForRace();
    Console.WriteLine($"Car {name} is ready to go!");
});

// Bulk operation on the Query.
query.Add<Ready>();

// Adding component conditionally at runtime
if (ae86.Has<Ready>()) ae86.Add<Racing>();

// Removing component conditionally at runtime
if (ae86.Has<Racing>()) ae86.Remove<Ready>();


#region Components

// "tags", size-less components we use to mark & classify entities
internal struct Car;

internal struct Ready;

internal struct Racing;


// data component
internal struct Engine
{
    public float Horsepower;
    public float Torque;
}


// data component wrapping another type, here: a string
internal readonly struct Name(string value)
{
    public static implicit operator Name(string value) => new(value);
    public override string ToString() => value;
}


// a class component, wrapping a string
internal class Driver(string name)
{
    public void ReportForRace() => Console.WriteLine($"{name} is ready!");

    public override string ToString() => name;
}

#endregion