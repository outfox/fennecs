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

// 🛰️ Somewhere on DS9
var sisko = world.Spawn();
Console.WriteLine($"{"Sisko",10} is {sisko} - a whole new person!");

// 🌌 Meanwhile in the Delta Quadrant
var janeway = world.Spawn();
Console.WriteLine($"{"Janeway",10} is {janeway} - timeless!");

// 🌠 And in the 32nd century
Console.WriteLine($"{"Kirk",10} is {kirk} ... but is he alive? {world.IsAlive(kirk)}");
Console.WriteLine($"{"Shatner",10}, after all, was not Stewart! {kirk != picard}");
