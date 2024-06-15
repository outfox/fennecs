namespace fennecs.tests.Integration;

public class LivenessTests(ITestOutputHelper output)
{
    [Fact]
    public void LivenessTest()
    {
        using var world = new World();

        var entity = world.Spawn();
        if (entity.Alive) output.WriteLine(entity.ToString());
        entity.Despawn();
        if (!entity.Alive) output.WriteLine(entity.ToString());
    }
}
