// StarTrek.cs (type declarations at bottom of file)

using fennecs;

Console.OutputEncoding = System.Text.Encoding.UTF8;
if (!Console.IsOutputRedirected) Console.Clear();
Console.WriteLine("Directed by Renard Nimoy.");

var world = new World();

var kirk = world.Spawn();
Console.WriteLine($"James Tiberius Kirk, StarFleet identification\n{kirk}");

Console.WriteLine("Kirk despawned.");
world.Despawn(kirk);

var picard = world.Spawn();
Console.WriteLine($"Jean-Luc Picard, (The Next Generation!) StarFleet identification\n{picard}");

var janeway = world.Spawn();
Console.WriteLine($"Kathryn Janeway, (New Series!) StarFleet identification\n{janeway}");

Console.WriteLine("Picard despawned.");
world.Despawn(picard);

var georgiou = world.Spawn();
Console.WriteLine($"Philippa Georgiou, (The Third Generation?) StarFleet identification\n{georgiou}");