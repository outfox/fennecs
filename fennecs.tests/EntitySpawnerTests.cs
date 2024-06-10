namespace fennecs.tests;

public class EntitySpawnerTests
{
    [Fact]
    public void Can_Obtain_Spawner()
    {
        using var world = new World();
        using var spawner = world.Entity();
        Assert.NotNull(spawner);
    }

    
    [Fact]
    public void Can_Dispose_Spawner_Once()
    {
        using var world = new World();
        var spawner = world.Entity();
        spawner.Dispose();
        Assert.Throws<ObjectDisposedException>(spawner.Dispose);
    }

    
    [Fact]
    public void Can_Spawn_One_Entity()
    {
        using var world = new World();
        using var spawner = world.Entity();
        spawner.Spawn();
        Assert.Equal(1, world.Count);
        // ReSharper disable once RedundantArgumentDefaultValue
        spawner.Spawn(1);
        Assert.Equal(2, world.Count);
    }

    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    [InlineData(420)]
    [InlineData(1_000_000)]
    public void Can_Spawn_Specific_Numbers_of_Entities(int amount)
    {
        using var world = new World();
        using var spawner = world.Entity();
        spawner.Spawn(amount);
        Assert.Equal(amount, world.Count);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(69)]
    public void Can_Spawn_With_Component(int amount)
    {
        using var world = new World();
        using var spawner = world.Entity();
        spawner.Add(Random.Shared.Next());
        spawner.Spawn(amount);

        using var query = world.Query<int>().Compile();
        Assert.Equal(amount, query.Count);
    }
    

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(31)]
    [InlineData(41)]
    [InlineData(69)]
    [InlineData(512)]
    [InlineData(4096)]
    public void Can_Add_Component_and_Spawn_Again(int amount)
    {
        using var world = new World();
        using var spawner = world.Entity();
        spawner.Add(Random.Shared.Next());
        spawner.Spawn(amount);

        spawner.Add(Random.Shared.NextSingle());
        spawner.Spawn(amount * 2);

        using var query0 = world.Query<int>().Compile();
        using var query1 = world.Query<float>().Compile();

        Assert.Equal(amount * 3, query0.Count);
        Assert.Equal(amount * 2, query1.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(31)]
    [InlineData(41)]
    [InlineData(69)]
    [InlineData(512)]
    [InlineData(4096)]
    public void Can_Remove_Component_and_Spawn_Again(int amount)
    {
        using var world = new World();
        using var spawner = world.Entity();
        
        spawner
            .Add(Random.Shared.Next())
            .Add(Random.Shared.NextSingle())
            .Spawn(amount);

        spawner.Remove<int>()
            .Spawn(amount);

        using var query0 = world.Query<int>().Compile();
        using var query1 = world.Query<float>().Compile();

        Assert.Equal(amount * 1, query0.Count);
        Assert.Equal(amount * 2, query1.Count);
    }
}
