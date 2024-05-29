using fennecs;

// The universe is vast, but finite. Its resources, finite.
// (fennecs 1.x can handle around 2^31 entities, so it's not that finite)
var world = new World();

using var entities = world.Entity()
    .Add<Alive>()
    .Spawn(1_000_000);

// Life. Unchecked, it will cease to exist. It needs correcting.
var thanosQuery = world.Query<Alive>().Compile();  

// The hardest choices require the strongest wills. 
var random = new Random();
thanosQuery.For((Entity entity, ref Alive _) =>
{
    if (random.NextDouble() < 0.5) entity.Add<Survive>();
});

// I'm the only one who knows that. At least I'm the only who has the will to act on it.
thanosQuery.Exclude<Survive>(Match.Plain); 

// I could simply snap my fingers, and they would all cease to exist.
thanosQuery.Despawn();

// I call that... mercy.
var remainingCount = world.Query<Alive>().Compile().Count;
Console.WriteLine($"Entities remaining after Thanos snap: {thanosQuery.Count}");

// Component tags
internal readonly struct Alive;
internal readonly struct Survive;


