// Thanos.cs (type declarations at bottom of file)

using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();

// The universe is vast, but finite. Its resources, finite.
using var world = new World();

// Oh don't be dramatic! It's not that finite.
// (fennecs can handle millions of entities)
const int initialPopulation = 1_000_000;
using var entities = world.Entity()
    .Add<Alive>()
    .Spawn(initialPopulation);

// Life. Unchecked, it will cease to exist. It needs correcting.
var thanosQuery = world.Query().Has<Alive>().Stream();  
Console.WriteLine($"Entities before Thanos Snap: {thanosQuery.Count}");

// The hardest choices require the strongest wills. (and two dumb coin flips) 
var random = new Random(2018);

using (var _ = world.Lock())
{
    // We can also use LINQ to iterate Entities: Slower, but sooo convenient.
    foreach (var entity in thanosQuery)
    {
        // *mumbles* 50% chance of being Lucky or Unlucky... right?
        if (random.NextDouble() < 0.5) entity.Add<Lucky>();
        if (random.NextDouble() < 0.5) entity.Add<Unlucky>();
    }
}

// I'm the only one who knows that. The Unlucky must go! (mkay...)
thanosQuery.Subset<Unlucky>(Match.Plain);

// And I guess that means the Lucky will stay! (uh oh!)
thanosQuery.Exclude<Lucky>(Match.Plain);

// (Aside: Thanos flunked probabilistics. Here's what's truly going on!)
var unluckyQuery = world.Query().Has<Unlucky>().Stream();  
var luckyQuery = world.Query().Has<Lucky>().Stream();
var bothQuery = world.Query().Has<Unlucky>().Not<Lucky>().Stream();

Console.WriteLine($"Lucky Entities: {luckyQuery.Count} ({Math.Round(luckyQuery.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Unlucky Entities: {unluckyQuery.Count} ({Math.Round(unluckyQuery.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Unlucky Entities that AREN'T ALSO Lucky: {bothQuery.Count} ({Math.Round(bothQuery.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Targeted by Thanos: {thanosQuery.Count} (seen this number before?)");


// I could simply snap my fingers, and they would all cease to exist.
Console.WriteLine("OH SNAP!");
thanosQuery.Despawn();

// I call that... mercy.
Console.WriteLine($"Entities surviving after Thanos Snap: {world.Count} ({Math.Round(world.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Directed by Robert B. Weide.");

// Component tags
internal readonly struct Alive;
internal readonly struct Lucky;
internal readonly struct Unlucky;

