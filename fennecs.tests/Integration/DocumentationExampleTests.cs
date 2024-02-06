namespace fennecs.tests.Integration;
using Position = System.Numerics.Vector3;

public class DocumentationExampleTests
{
    [Fact]
    public void QuickStart_Example_Works()
    {
        using var world = new World();
        var entity1 = world.Spawn().Add<Position>().Id();
        var entity2 = world.Spawn().Add(new Position(1, 2, 3)).Add<int>().Id();

        var query = world.Query<Position>().Build();

        const float MULTIPLIER = 10f;

        query.RunParallel((ref Position pos, float uniform) => { pos *= uniform; }, MULTIPLIER, chunkSize: 4000);

        var pos1 = world.GetComponent<Position>(entity1);
        var expected = new Position() * MULTIPLIER;
        Assert.Equal(expected, pos1);

        var pos2 = world.GetComponent<Position>(entity2);
        expected = new Position(1, 2, 3) * MULTIPLIER;
        Assert.Equal(expected, pos2);
    }
}