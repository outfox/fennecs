// SPDX-License-Identifier: MIT

namespace fennecs.tests;

/// <summary>
/// Entities are world-relative: relations may not target Entities of other Worlds.
/// (Aspects cover the segregation use cases that formerly motivated cross-world constructs)
/// </summary>
public class CrossWorldRelationTests
{
    [Fact]
    public void Fluent_Add_Rejects_Foreign_Relation_Target()
    {
        using var world1 = new World(0);
        using var world2 = new World(0);

        var entity = world1.Spawn();
        var foreign = world2.Spawn();

        var ex = Assert.Throws<InvalidOperationException>(() => entity.Add(123, foreign));
        Assert.Contains("another World", ex.Message);
    }


    [Fact]
    public void Fluent_Add_Rejects_Foreign_Target_in_Deferred_Mode()
    {
        using var world1 = new World(0);
        using var world2 = new World(0);

        var entity = world1.Spawn();
        var foreign = world2.Spawn();

        using var worldLock = world1.Lock();

        // Fails fast at the call site, not at catch-up.
        Assert.Throws<InvalidOperationException>(() => entity.Add(123, foreign));
    }


    [Fact]
    public void EntityRef_Add_Rejects_Foreign_Relation_Target()
    {
        using var world1 = new World(0);
        using var world2 = new World(0);

        world1.Spawn().Add(1);
        var foreign = world2.Spawn();

        var checked_ = 0;
        world1.Stream<int>().For((in e, ref _) =>
        {
            // (no Assert.Throws here: EntityRef cannot be captured by its nested lambda)
            try
            {
                e.Add(1.0f, foreign);
                Assert.Fail("Expected InvalidOperationException for cross-world relation.");
            }
            catch (InvalidOperationException)
            {
                checked_++;
            }
        });
        Assert.Equal(1, checked_);
    }


    [Fact]
    public void Spawner_Rejects_Foreign_Relation_Target()
    {
        using var world1 = new World(0);
        using var world2 = new World(0);

        var foreign = world2.Spawn();

        using var spawner = world1.Entity().Add(123, foreign);
        Assert.Throws<InvalidOperationException>(() => spawner.Spawn(1));
    }


    [Fact]
    public void Batch_Rejects_Foreign_Relation_Target()
    {
        using var world1 = new World(0);
        using var world2 = new World(0);

        world1.Spawn().Add(1);
        var foreign = world2.Spawn();

        var query = world1.Query<int>().Compile();
        Assert.Throws<InvalidOperationException>(() => query.Batch().Add(1.0f, foreign));
        Assert.Throws<InvalidOperationException>(() => query.Batch().Remove<float>(foreign));
    }


    [Fact]
    public void Same_World_Relations_Still_Work()
    {
        using var world = new World(0);

        var entity = world.Spawn();
        var target = world.Spawn();

        entity.Add(123, target);
        Assert.True(entity.Has<int>(target));
    }


    [Fact]
    public void Foreign_Has_Reads_Are_False_Not_Throwing()
    {
        using var world1 = new World(0);
        using var world2 = new World(0);

        var entity = world1.Spawn().Add(123);
        var foreign = world2.Spawn();

        // Read paths stay cheap: a foreign key simply never matches anything.
        Assert.False(entity.Has<int>(foreign));
    }
}
