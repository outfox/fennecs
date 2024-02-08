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
        
        var targets = new List<Entity>();
        world.Archetypes.CollectTargets<int>(targets);
        Assert.Single(targets);
        Assert.Contains(target1, targets);
        targets.Clear();

        world.Archetypes.CollectTargets<float>(targets);
        Assert.Single(targets);
        Assert.Contains(target2, targets);
    }


    [Fact]
    public void Despawn_Target_Removes_Relation_From_Origins()
    {
        using var world = new World();
        var target1 = world.Spawn().Id();
        var target2 = world.Spawn().Id();

        for (var i = 0; i < 1000; i++)
        {
            world.Spawn().Add(666, target1).Id();
            world.Spawn().Add(444, target2).Id();
        }

        var query1 = world.Query<Entity>().Has<int>(target1).Build();
        var query2 = world.Query<Entity>().Has<int>(target2).Build();

        Assert.Equal(1000, query1.Count);
        Assert.Equal(1000, query2.Count);
        world.Despawn(target1);
        Assert.Equal(0, query1.Count);
        Assert.Equal(1000, query2.Count);
    }
}