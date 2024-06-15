using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();

var world = new World();

world.Entity() // Three Rings for the Elven-kings under the sky,
    .Add(new RingBearer("elven"))
    .Add(Link.With(OneRing.Instance)) // One Ring to rule them all,
    .Spawn(3);

world.Entity() //Seven for the Dwarf-lords in their halls of stone,
    .Add(new RingBearer("dwarven"))
    .Add(Link.With(OneRing.Instance)) // One Ring to find them,
    .Spawn(7);

world.Entity() //Nine for Mortal Men doomed to die...
    .Add(new RingBearer("human"))     // ("Human Rings"? I like it!)
    .Add(Link.With(OneRing.Instance)) // One Ring to bring them all, 
    .Spawn(9);

// Use our Palantir to find all Rings linked to the One Ring
var ringsOfPower = world
    .Query<RingBearer, OneRing>(Match.Plain, Link.Any)
    .Has(Link.With(OneRing.Instance)) // and in the darkness bind them.
    .Stream();

// Use the Query to corrupt the Ring Bearers
ringsOfPower.For((Entity ring, ref RingBearer bearer, ref OneRing link) =>
{
    bearer = bearer with { corrupted = true };
    link.CallOut(ring, bearer);  // it calls out to its master!
});

Console.WriteLine("\nDirected by: Peter Foxen");

// The Ring Bearer component represents the owner of a Ring of Power.
internal record struct RingBearer(string race, bool corrupted = false);

//But they were, all of them, deceived, for another ring was made!
internal class OneRing
{
    // Of course it's a GoF singleton! It's straight out of Mordor!
    public static readonly OneRing Instance = new();
    // No, we can't "make another", precious.
    private OneRing() { }
    // Sample interaction for linked Entities to use
    public void CallOut(Entity ring, RingBearer bearer)
    {
        if (bearer.corrupted)
            Console.WriteLine($"{ring} corrupted its {bearer.race} bearer!");
    } 
}
