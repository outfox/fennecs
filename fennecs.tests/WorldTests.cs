namespace fennecs.tests;

public class WorldTests
{
    [Fact]
    public World World_Creates()
    {
        var world = new World();
        Assert.NotNull(world);
        return world;
    }

    [Fact]
    public void World_Disposes()
    {
        using var world = World_Creates();
    }

    [Fact]
    public Entity World_Spawns_valid_Entities()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        Assert.NotEqual(entity, Entity.None);
        Assert.NotEqual(entity, Entity.Any);
        return entity;
    }
    
    [Fact]
    public void World_Count_Accurate()
    {
        using var world = new World();
        Assert.Equal(0, world.Count);

        var e1 = world.Spawn().Id();
        Assert.Equal(1, world.Count);

        world.On(e1).Add<int>(typeof(bool));
        Assert.Equal(1, world.Count);

        var e2 = world.Spawn().Id();
        world.On(e2).Add<int>(typeof(bool));
        Assert.Equal(2, world.Count);
    }
}