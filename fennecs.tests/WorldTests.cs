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

    [Fact]
    public void Can_Find_Targets_of_Relation()
    {
        using var world = new World();
        var target1 = world.Spawn().Id();
        var target2 = world.Spawn().Add("hallo dieter").Id();

        world.Spawn().Add(666, target1).Id();
        world.Spawn().Add(1.0f, target2).Id();
        world.Spawn().Add("hunter2", typeof(Thread)).Id();
        
        var targets = new HashSet<Entity>();
        world._archetypes.GetTargets<int>(targets);
        Assert.Single(targets);
        Assert.Contains(target1, targets);
        targets.Clear();
        
        world._archetypes.GetTargets<float>(targets);
        Assert.Single(targets);
        Assert.Contains(target2, targets);
    }
}