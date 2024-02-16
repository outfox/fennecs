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

        query.Job((ref Position pos, float uniform) => { pos *= uniform; }, MULTIPLIER, chunkSize: 2048);

        var pos1 = world.GetComponent<Position>(entity1);
        var expected = new Position() * MULTIPLIER;
        Assert.Equal(expected, pos1);

        var pos2 = world.GetComponent<Position>(entity2);
        expected = new Position(1, 2, 3) * MULTIPLIER;
        Assert.Equal(expected, pos2);
    }

    [Theory]
    [InlineData(2769, 2_001)]
    [InlineData(18_000, 2_000)]
    [InlineData(20_000, 1_999)]
    [InlineData(1_200, 37)]
    public void Can_Iterate_Multiple_Chunks(int count, int chunkSize)
    {
        using var world = new World();
        for (var i = 0; i < count; i++)
        {
            world.Spawn()
                .Add(new Position(1,2,3))
                .Add<int>()
                .Add<float>()
                .Add("string string")
                .Add<short>()
                .Id();
        }

        var query1 = world.Query<Position>().Build();
        var query2 = world.Query<Position, int>().Build();
        var query3 = world.Query<float, Position, int>().Build();
        var query4 = world.Query<Entity, string, Position, int>().Build();
        var query5 = world.Query<Position, int, float, string, short>().Build();

        query1.Job((ref Position _) =>
        {
        }, chunkSize: chunkSize);
        Assert.Equal(count, query1.Count);

        query2.RunParallel((ref Position _, ref int _) =>
        {
        }, chunkSize: chunkSize);
        Assert.Equal(count, query2.Count);

        query3.RunParallel((ref float _, ref Position _, ref int _) =>
        {
        }, chunkSize: chunkSize);
        Assert.Equal(count, query3.Count);

        query4.RunParallel((ref Entity _, ref string _, ref Position _, ref int _) =>
        {
        }, chunkSize: chunkSize);
        Assert.Equal(count, query4.Count);

        query5.RunParallel((ref Position _, ref int _, ref float _, ref string _, ref short _) =>
        {
        }, chunkSize: chunkSize);
        Assert.Equal(count, query5.Count);
        
    }
}