using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();

var world = new World();

//But they were, all of them, deceived, for another ring was made!

// Three Rings for the Elven-kings under the sky,
world.Entity()
    .Add(new RingBearer("elven"))
    .Add(Link.With(OneRing.Instance)) // One Ring to rule them all,
    .Spawn(3);

//Seven for the Dwarf-lords in their halls of stone,
world.Entity()
    .Add(new RingBearer("dwarven"))
    .Add(Link.With(OneRing.Instance)) // One Ring to find them,
    .Spawn(7);

//Nine for Mortal Men doomed to die... 
world.Entity()
    .Add(new RingBearer("human"))     // ("Human Rings"? I like it!)
    .Add(Link.With(OneRing.Instance)) // One Ring to bring them all, and in the darkness bind them.
    .Spawn(9);


// Gaze into our Palantir to find all Rings linked to the One Ring
var linkedRings = world.Query<RingBearer>().Has<OneRing>(OneRing.Instance).Stream();

// Use the Query to corrupt the Ring Bearers
linkedRings.For((Entity ring, ref RingBearer bearer) =>
{
    bearer = bearer with { corrupted = true };
    Console.WriteLine($"{ring} has corrupted its {bearer.race} bearer - {bearer.corrupted}!");
});


Console.WriteLine();
Console.WriteLine("Directed by: Peter Foxen");

// The Ring Bearer component represents the owner of a Ring of Power.
internal record struct RingBearer(string race, bool corrupted = false);

// The Ring Power represents the magical binding power of the One Ring.
internal class OneRing
{
    // Of course it's a singleton! It's coming straight out of Mordor!
    public static readonly OneRing Instance = new();

    private OneRing() { }
}
