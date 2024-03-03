// StarTrek.cs (type declarations at bottom of file)

using fennecs;

if (!Console.IsOutputRedirected) Console.Clear();
Console.WriteLine("Space. The final frontier.");

// Directed by Renard Nimoy
var world = new World();

var kirk = world.Spawn();
Console.WriteLine($"{"Kirk",10} is {kirk} - boldly going!");

Console.WriteLine("Kirk is gone.");
world.Despawn(kirk);

var picard = world.Spawn();
Console.WriteLine($"{"Picard",10} is {picard} - the Next Generation!");

var janeway = world.Spawn();
Console.WriteLine($"{"Janeway",10} is {janeway} - got her crew home!");

Console.WriteLine("Picard is gone.");
world.Despawn(picard);

var georgiou = world.Spawn();
Console.WriteLine($"{"Georgiou",10} is {georgiou} - in a mirror darkly!");