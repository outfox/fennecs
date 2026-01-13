// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityLifecycleTests
{
    [Fact]
    public void Entity_Can_Despawn_Itself()
    {
        using var world = new World();
        var entity = world.Spawn();
        entity.Add(123);
        entity.Add(7.0f, world.Spawn());
        entity.Add(Link.With("hello"));
        entity.Despawn();
        Assert.False(world.IsAlive(entity));
    }

    [Fact]
    public void Truthy()
    {
        using var world = new World();

        var entity = world.Spawn();
        Assert.True(entity);

        entity.Despawn();
        Assert.False(entity);
        
        entity = default;
        Assert.False(entity);
    }
}
