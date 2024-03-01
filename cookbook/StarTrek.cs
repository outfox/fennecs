// StarTrek.cs (type declarations at bottom of file)

using fennecs;

// 🌌 Space. Fhe final Frontier!
var world = new World();

// 🚀 TOS doesn't mean Terms of Service.
var kirk = world.Spawn();
Console.WriteLine($"{"Kirk",10} is {kirk} ... boldly going!");

// 🖖 Live long and prosper. Not!
world.Despawn(kirk);

// 🛸 The Next Generation
var picard = world.Spawn();
Console.WriteLine($"{"Picard",10} is {picard} - the next Generation!");

// 🌌 Meanwhile in the Delta Quadrant
var janeway = world.Spawn();
Console.WriteLine($"{"Janeway",10} is {janeway} - the best Captain ever!");

// 🌠 And in the 32nd century
Console.WriteLine($"{"Kirk",10} is {kirk} - and alive? {world.IsAlive(kirk)}");

// 🌌 Goodbye, Gene
world.Despawn(picard);
world.Despawn(janeway);

var archer = world.Spawn();
Console.WriteLine($"{"Archer",10} is {archer}, ugh, don't we hate reboots...");

var georgiou = world.Spawn();
Console.WriteLine($"{"Georgiou",10} is {georgiou}, now THAT's a Captain!");

// ⁉️ Needed to be said.
Console.WriteLine($"{"Shatner",10} ain't ever Stewart! {kirk != picard}");
