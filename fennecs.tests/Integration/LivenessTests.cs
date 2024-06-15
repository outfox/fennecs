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
}
