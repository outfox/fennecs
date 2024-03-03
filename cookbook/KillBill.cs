// KillBill.cs (type declarations at bottom of file)

using fennecs;

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
        .AddRelation<Betrayed>(us);

    // And just in case, we will never forget.
    us.AddRelation<Grudge>(them);
}

// We query for their Locations; to pay them a visit;
// and their Entity Id. It's a surprise tool that will help us later!
var betrayingVipers = world.Query<Location, Identity>()
    .Has<Betrayed>(us)
    .Build();

Console.WriteLine($"As we said, there were {betrayingVipers.Count} of them.");

// They went into hiding around the world.
betrayingVipers.For((ref Location location) =>
{
    location = $"hideout 0x{Random.Shared.Next():x8}";
    Console.WriteLine($"One hides in {location}.");
});

Console.WriteLine($"We are still {us.Ref<Location>()}, though.");

// Has<>(Match.Entity) is the same as saying HasRelation<>()
Console.WriteLine($"Do we hold grudges? {us.Has<Grudge>(Match.Entity)}.");

// Choose your weapon:
//    query.Despawn();
//    query.Truncate(0);
// -> visiting each entity personally
betrayingVipers.For((ref Location theirLocation, ref Identity theirIdentity) =>
{
    ref var ourLocation = ref us.Ref<Location>();
    ourLocation = theirLocation;

    // Knock knock.
    Console.WriteLine($"Oh, hello {theirIdentity}! Remember {us}?");

    // Get our revenge.
    world.Despawn(theirIdentity);
});

// Survey the aftermath.
Console.WriteLine($"Now, there are {betrayingVipers.Count} of them.");
Console.WriteLine($"Let's get out of {us.Ref<Location>()}.");
us.Ref<Location>() = "traveling";

// We satisfied our grudges.
Console.WriteLine($"Any more grudges? {us.Has<Grudge>(Match.Entity)}.");
Console.WriteLine($"We've been {us.Ref<Location>()} for a while.");


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