namespace fennecs.tests.Conceptual;

public class SortingTests(ITestOutputHelper output)
{
    [Fact]
    public void Sorting_By_Groups()
    {
        using var world = new World();

        Entity[] groups = [world.Spawn(), world.Spawn(), world.Spawn(), world.Spawn()];

        world.Spawn().Add("hello first", groups[0]);
        world.Spawn().Add("hello last", groups[^1]);
        world.Spawn().Add("hello second", groups[1]);
        world.Spawn().Add("hello first, too", groups[0]);

        foreach (var group in groups)
        {
            world.Stream<string>(group).For((ref string message) => output.WriteLine(message));
        }
    }

    [Fact]
    public void Sorting_By_Layers()
    {
        using var world = new World();

        for (var i = 0; i < 200; i++)
        {
            world.Spawn().Add(Random.Shared.Next() % 15);
        }

        for (var current = 0; current < 100; current++)
        {
            world.Stream<int>().For(
                uniform: current,
                action: (int uniform, in Entity entity, ref int layer) =>
                {
                    if (uniform == layer) output.WriteLine($"Hello from layer {layer}, {entity}");
                });
        }
    }
}
