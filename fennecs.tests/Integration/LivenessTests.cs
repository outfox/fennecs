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
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(69)]
    public void CanDespawnWithWorldLocks(int count)
    {
        using var world = new World();
        world.Entity().Spawn(count);

        var worldLock = world.Lock();
        foreach (var entity in world) // world is a query
        { 
            // Example uses coin flip, but we sith deal in absolutes
            // if (Random.Shared.NextSingle() >= 0.5f) 
            entity.Despawn();
            if (entity.Alive) output.WriteLine("Dead Fox Walking!");
            Assert.True(entity.Alive);
        }
        worldLock.Dispose(); // this will catch up the despawns
        
        Assert.Empty(world);
    }

    [Fact]
    public void CanDespawnViaUniformSet()
    {
        using var world = new World();
        world.Entity().Add(69).Spawn(1234);
        world.Entity().Add(42).Spawn(4567);

        var stream = world.Query<int>().Stream();
        var despawns = new HashSet<Entity>();
        stream.For(
            uniform: despawns, 
            action: (HashSet<Entity> killSet, in Entity entity, ref int value) => 
        {
            if (value == 69) killSet.Add(entity); // Can also just use a closure here, i.e. despawns.
            if (value == 60 + 9 && killSet.Count % 3 == 0) killSet.Add(entity); // fake redundant addition ;)
        });
        
        Assert.Equal(1234, despawns.Count);
        Assert.Equal(1234+4567, world.Count);
        
        foreach (var d in despawns) world.Despawn(d); // I'll add a overload for any collections later
        
        Assert.Equal(4567, world.Count);

        stream.For((ref int value) =>
        {
            Assert.NotEqual(69, value);
        });
    }
    
}
