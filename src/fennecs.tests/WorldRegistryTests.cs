// SPDX-License-Identifier: MIT

using fennecs.pools;

namespace fennecs.tests;

public class WorldRegistryTests
{
    [Fact]
    public void World_Tags_are_Recycled_on_Dispose()
    {
        // Far more Worlds than the 255 concurrent slots — only possible if Dispose recycles tags.
        for (var i = 0; i < 300; i++)
        {
            using var world = new World(0);
            Assert.NotEqual(0, world.Tag);
            var entity = world.Spawn();
            Assert.True(world.IsAlive(entity));
        }
    }


    [Fact]
    public void Entities_of_Disposed_World_are_not_Alive()
    {
        var world = new World(0);
        var entity = world.Spawn();
        Assert.True(entity.Alive);

        world.Dispose();
        Assert.False(entity.Alive);
    }


    [Fact]
    public void CRUD_on_Entity_of_Disposed_World_Throws()
    {
        var world = new World(0);
        var entity = world.Spawn();
        world.Dispose();

        Assert.Throws<InvalidOperationException>(() => entity.Add(123));
    }


    [Fact]
    public void Double_Dispose_is_Safe()
    {
        var world = new World(0);
        world.Dispose();
        world.Dispose();
    }


    [Fact]
    public void EntityPool_Retires_Index_on_Generation_Wrap()
    {
        var pool = new EntityPool(1, 0);

        // Exhaust the full generation space of a single index.
        var entity = pool.Spawn();
        var index = entity.Index;
        for (var gen = 1; gen < ushort.MaxValue; gen++)
        {
            Assert.Equal(index, entity.Index); // index gets recycled...
            pool.Recycle(entity);
            entity = pool.Spawn();
        }

        // ...until its generation wraps: then the index is retired and a fresh one minted.
        pool.Recycle(entity);
        var successor = pool.Spawn();
        Assert.NotEqual(index, successor.Index);

        // Retired indices don't leak into the live count.
        pool.Recycle(successor);
        Assert.Equal(0, pool.Count);
    }


    [Fact]
    public void Relation_does_not_Resurrect_on_Index_Reuse()
    {
        using var world = new World(0);

        var target = world.Spawn();
        var origin = world.Spawn().Add("payload", target);
        Assert.True(origin.Has<string>(target));

        // Despawning the target eagerly cleans up relations targeting it.
        world.Despawn(target);
        Assert.False(origin.Has<string>(Match.Target));

        // A new Entity reusing the same index must not inherit the old relation.
        var successor = world.Spawn();
        Assert.Equal(target.Index, successor.Index);
        Assert.False(origin.Has<string>(successor));
    }


    [Fact]
    public void Deferred_Double_Despawn_Throws_on_CatchUp()
    {
        using var world = new World(0);
        var entity = world.Spawn();

        var worldLock = world.Lock();
        world.Despawn(entity);
        world.Despawn(entity); // both deferred; the second one is stale at catch-up time.

        Assert.Throws<ObjectDisposedException>(() => worldLock.Dispose());
    }
}
