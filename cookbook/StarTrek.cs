// StarTrek.cs (type declarations at bottom of file)

using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();
Console.WriteLine("Space. The final frontier.");

// Directed by Renard Nimoy
var world = new World();

var kirk = world.Spawn();
Console.WriteLine($"{"Kirk",10} is {kirk} - boldly going!");

world.Despawn(kirk);

var picard = world.Spawn();
Console.WriteLine($"{"Picard",10} is {picard} - the Next Generation!");

var janeway = world.Spawn();
Console.WriteLine($"{"Janeway",10} is {janeway} - got her crew home!");

world.Despawn(picard);

var georgiou = world.Spawn();
Console.WriteLine($"{"Georgiou",10} is {georgiou} - in a mirror darkly!");