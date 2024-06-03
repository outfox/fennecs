// KillBill.cs (type declarations at bottom of file)

using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();
Console.WriteLine("Directed by: Trentin Quarantino");

// The world is a stage. 
var world = new World();

// This is us. Here and now.
var us = world.Spawn().Add<Location>("here");

// Ok, let's maybe not spend 4 years in a coma!
// Thread.Sleep(TimeSpan.FromDays(365 * 4));

// They were five. This is how it went:
for (var i = 0; i < 5; i++)
{
    var them = world.Spawn()
        .Add<Location>("wedding chapel")
        .Add<Betrayed>(us);

    // And just in case, we will never forget.
    us.Add<Grudge>(them);
}

// We query for their Locations; to pay them a visit;
// and their Entity Id. It's a surprise tool that will help us later!
var betrayingVipers = world.Query<Location>()
    .Has<Betrayed>(us)
    .Stream();

Console.WriteLine($"As we said, there were {betrayingVipers.Count} of them.");

// They went into hiding around the world.
betrayingVipers.For((ref Location location) =>
{
    location = $"hideout 0x{Random.Shared.Next():x8}";
    Console.WriteLine($"One hides in {location}.");
});

Console.WriteLine();
Console.WriteLine($"We are still {us.Ref<Location>()}, though.");

// Has<>(Match.Entity) is the same as saying HasRelation<>()
Console.WriteLine();
Console.WriteLine($"Do we hold grudges? {us.Has<Grudge>(Entity.Any)}.");
Console.WriteLine("This is us (and our grudges):\n" + us);

// Choose your weapon:
//    query.Despawn();
//    query.Truncate(0);
// -> visiting each entity personally
betrayingVipers.For((Entity them, ref Location theirLocation) =>
{
    Console.WriteLine();
    
    ref var ourLocation = ref us.Ref<Location>();
    ourLocation = theirLocation;

    // Knock knock.
    Console.WriteLine($"Oh, hello {them}! Remember us ({us})?");
    Console.WriteLine("They do. They remember everything");
    
    // Get our revenge. (could also them.Despawn()) 
    world.Despawn(them);
});

// Survey the aftermath.
Console.WriteLine();
Console.WriteLine($"Now, there are {betrayingVipers.Count} of them.");
Console.WriteLine($"Let's get out of {us.Ref<Location>()}.");
us.Ref<Location>() = "traveling";

// We satisfied our grudges.
Console.WriteLine($"Any more grudges? {us.Has<Grudge>(Entity.Any)}.");
Console.WriteLine("This is us now:\n" + us);
Console.WriteLine($"We'll be {us.Ref<Location>()} for a while.");


# region Components
// "tag" (size-less) component, used here to back a Relation
internal struct Grudge;


// "tag" (size-less) component, used here to back a Relation
internal struct Betrayed;


// A Location Component wrapping a string.
internal readonly struct Location(string there)
{
    public static implicit operator Location(string location) => new(location);
    public override string ToString() => there;
}
# endregion