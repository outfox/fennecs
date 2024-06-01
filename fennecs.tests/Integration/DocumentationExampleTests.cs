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

        var query = world.Query<Position>().Compile();

        const float MULTIPLIER = 10f;

        query.Job((ref Position pos, float uniform) => { pos *= uniform; }, MULTIPLIER);

        var pos1 = world.GetComponent<Position>(entity1, Match.Plain);
        var expected = new Position() * MULTIPLIER;
        Assert.Equal(expected, pos1);

        var pos2 = world.GetComponent<Position>(entity2, Match.Plain);
        expected = new Position(1, 2, 3) * MULTIPLIER;
        Assert.Equal(expected, pos2);
    }

    private struct TypeA;
}