// KillBill.cs (type declarations at bottom of file)

using fennecs;

// Directed by Trentin Quarantino
var world = new World();

// This is us. Here. Now. 
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
var query = world.Query<Location, Identity>()
    .Has<Betrayed>(us)
    .Build();

Console.WriteLine($"As we said, there were {query.Count} of them.");

// They went into hiding around the world.
query.For((ref Location location) =>
{
    location = $"hideout 0x{Random.Shared.Next():x8}";
    Console.WriteLine($"One hides in {location}.");
});

Console.WriteLine($"We are still {us.Ref<Location>()}, though.");

// Has<>(Match.Entity) is the same as saying HasRelation<>()
Console.WriteLine($"Do we hold grudges? {us.Has<Grudge>(Match.Entity)}.");

// Choose your weapon:
// query.Clear(); ... or
// query.Truncate(0);
// But nah... we want to see the white in their eyes!

// Time to visit each one personally!
query.For((ref Location theirLocation, ref Identity theirIdentity) =>
{
    ref var ourLocation = ref us.Ref<Location>();
    ourLocation = theirLocation;

    // Knock knock.
    Console.WriteLine($"Oh, hello {theirIdentity}! Remember {us}?");

    // Get our revenge.
    world.Despawn(theirIdentity);
});

// Survey the aftermath.
Console.WriteLine($"Now, there are {query.Count} of them.");
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
    public static implicit operator Location(string location)
    {
        return new Location(location);
    }


    public override string ToString()
    {
        return there;
    }
}

# endregion