using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();

var world = new World();

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
    .Add(Link.With(OneRing.Instance)) // One Ring to bring them all, 
    .Spawn(9);


// Gaze into our Palantir to find all Rings linked to the One Ring
var linkedRings = world
    .Query<RingBearer>()
    .Has(Link.With(OneRing.Instance)) // and in the darkness bind them.
    .Stream();

// Use the Query to corrupt the Ring Bearers
linkedRings.For((Entity ring, ref RingBearer bearer) =>
{
    bearer = bearer with { corrupted = true };
    Console.WriteLine(
        $"{ring} has corrupted its {bearer.race} bearer - {bearer.corrupted}!"
    );
});


Console.WriteLine();
Console.WriteLine("Directed by: Peter Foxen");

// The Ring Bearer component represents the owner of a Ring of Power.
internal record struct RingBearer(string race, bool corrupted = false);

//But they were, all of them, deceived, for another ring was made!
internal class OneRing
{
    // Of course it's a GoF singleton! It's straight out of Mordor!
    public static readonly OneRing Instance = new();
    // No, we can't "make another", precious.
    private OneRing() { }
}
