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
var population = world.Query().Has<Alive>().Compile();  
Console.WriteLine($"Entities before Thanos Snap: {population.Count}");

// The hardest choices require the strongest wills. (and two dumb coin flips?) 
var random = new Random(2018);

using (var _ = world.Lock())
{
    // We can also use LINQ to iterate Entities: Slower, but sooo convenient.
    foreach (var entity in population)
    {
        // *mumbles* 50% chance of being Lucky or Unlucky... right?
        if (random.NextDouble() < 0.5) entity.Add<Lucky>();
        if (random.NextDouble() < 0.5) entity.Add<Unlucky>();
    }
}

var thanosStream = population.Stream<Alive>() with
{
    // I'm the only one who knows that. The Unlucky must go! (mkay...)
    Subset = [Component.PlainComponent<Unlucky>()],
    
    // (monologue continues) (Thanos seems confused for a second)
    
    // ... the Lucky, I'll leave to chance. (uh oh!)
    Exclude = [Component.PlainComponent<Lucky>()],
};


// (Aside: Thanos flunked probabilistics. Here's what's truly going on!)
var unluckyQuery = world.Query().Has<Unlucky>().Compile();  
var luckyQuery = world.Query().Has<Lucky>().Compile();
var bothQuery = world.Query().Has<Unlucky>().Not<Lucky>().Compile();

Console.WriteLine($"Lucky Entities: {luckyQuery.Count} ({Math.Round(luckyQuery.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Unlucky Entities: {unluckyQuery.Count} ({Math.Round(unluckyQuery.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Unlucky Entities that AREN'T ALSO Lucky: {bothQuery.Count} ({Math.Round(bothQuery.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Targeted by Thanos: {thanosStream.Count} (seen this number before?)");


// I could simply snap my fingers, and they would all cease to exist.
Console.WriteLine("OH SNAP!");
thanosStream.Despawn();

// I call that... mercy.
Console.WriteLine($"Entities surviving after Thanos Snap: {world.Count} ({Math.Round(world.Count * 100f / initialPopulation)}%)");
Console.WriteLine($"Directed by Robert B. Weide.");

// Component tags
internal readonly struct Alive;
internal readonly struct Lucky;
internal readonly struct Unlucky;

