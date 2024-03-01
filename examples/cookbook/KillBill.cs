// KillBill.cs (type declarations at bottom of file)
using fennecs;

// 💒 Set the stage.
var world = new World();

// 🛌 This is us. Here. Now. 
var us = world.Spawn().Add<Location>("here");

// 👰 They were five. This is how it went:
for (var i = 0; i < 5; i++)
{
    var them = world.Spawn()
        .Add<Location>("wedding chapel")
        .AddRelation<Betrayed>(us);

    // And just in case, we will never forget.
    us.AddRelation<Grudge>(them);
}

// 🏥 Ok, let's maybe not spend 4 years in a coma!
// Thread.Sleep(TimeSpan.FromDays(365 * 4));

// 🥷 Well rested for once, we query for their Locations; to pay that visit.
// 🧩 Also get their Entity Id. It's a surprise tool that will help us later!
var query = world.Query<Location, Identity>()
    .Has<Betrayed>(us)
    .Build();

// 5️⃣ Making a list and checking it twice.
Console.WriteLine($"As we said, there were {query.Count} of them.");

// 🧭 They went into hiding around the world.
query.For((ref Location location) => {
    location = $"hideout 0x{Random.Shared.Next():x8}";    
    Console.WriteLine($"One hides in {location}.");
});

// 🙎‍♀️ And what about us?
Console.WriteLine($"We are still {us.Ref<Location>()}.");
Console.WriteLine($"Do we hold grudges? {us.Has<Grudge>(Match.Entity)}.");

// ⬇️ This would get them all in one fell swoop. 
// query.Clear();
// 🎥 But nah... that's not enough for a movie!

// ⚔️ Done plotting our revenge! Time to visit each one!
query.For((ref Location theirLocation, ref Identity theirIdentity) => {

    // 🚕 Drive the TaxiWagon
    ref var ourLocation = ref us.Ref<Location>();
    ourLocation = theirLocation;

    // 🏠 Why don't we knock on the door and say hello?
    Console.WriteLine($"Oh, hello {theirIdentity}! Remember {us}?");
    
    //☠️ And then, they were no more.
    world.Despawn(theirIdentity);
}); 

// 🪦 The aftermath.
Console.WriteLine($"Now, there are {query.Count} of them.");
Console.WriteLine($"Let's get out of {us.Ref<Location>()}.");
us.Ref<Location>() = "traveling";

// 🎬 It's a wrap! Roll credits.
Console.WriteLine($"We've been {us.Ref<Location>()} for a while.");
Console.WriteLine($"Any more grudges? {us.Has<Grudge>(Match.Entity)}.");

// 🪚 Simple components, for a simple plot.
struct Grudge;
struct Betrayed;
struct Location(string there)
{
    // 🔤 The data.
    public string where = there;
    
    // 😉 So we don't always need to invoke the Constructor.
    public static implicit operator Location(string location) => new(location);

    // 🗺️ So we can more readily see where "where" is.
    public override string ToString() => where;
    
};
