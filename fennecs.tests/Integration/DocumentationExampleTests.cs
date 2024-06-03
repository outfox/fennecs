namespace fennecs.tests.Integration;
using Position = System.Numerics.Vector3;

public class DocumentationExampleTests
{
    [Fact]
    public void QuickStart_Example_Works()
    {
        using var world = new World();
        var entity1 = world.Spawn().Add<Position>();
        var entity2 = world.Spawn().Add(new Position(1, 2, 3)).Add<int>();

        var query = world.Query<Position>(Identity.Plain).Stream();

        const float MULTIPLIER = 10f;

        query.Job(MULTIPLIER,(ref Position pos, float uniform) => { pos *= uniform; });

        var pos1 = world.GetComponent<Position>(entity1, Identity.Plain);
        var expected = new Position() * MULTIPLIER;
        Assert.Equal(expected, pos1);

        var pos2 = world.GetComponent<Position>(entity2, Identity.Plain);
        expected = new Position(1, 2, 3) * MULTIPLIER;
        Assert.Equal(expected, pos2);
    }

    private struct TypeA;
}