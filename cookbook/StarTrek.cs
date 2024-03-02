// StarTrek.cs (type declarations at bottom of file)

using fennecs;

// Directed by Renard Nimoy
var world = new World();

var kirk = world.Spawn();
Console.WriteLine($"{"Kirk",10} is {kirk} ... boldly going!");

world.Despawn(kirk);

var picard = world.Spawn();
Console.WriteLine($"{"Picard",10} is {picard} - the next Generation!");

var janeway = world.Spawn();
Console.WriteLine($"{"Janeway",10} is {janeway} - the best Captain ever!");

Console.WriteLine($"{"Kirk",10} is {kirk} - and alive? {world.IsAlive(kirk)}");

// Goodbye, Captains. Goodbye, Gene.
world.Despawn(picard);
world.Despawn(janeway);

var archer = world.Spawn();
Console.WriteLine($"{"Archer",10} is {archer}, ugh, don't we hate reboots...");

var georgiou = world.Spawn();
Console.WriteLine($"{"Georgiou",10} is {georgiou}, now THAT's a Captain!");

// We all knew it and we never said it.
Console.WriteLine($"{"Shatner",10} ain't ever Stewart! {kirk != picard}");