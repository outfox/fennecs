// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityRelationsTests
{
    [Fact]
    public void Can_Relate_to_Entity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add<int>(target);
        Assert.True(entity.Has<int>(target));
        Assert.False(entity.Has<int>(new Entity(world.Tag, 9001, 1)));
    }

    [Fact]
    public void Can_Relate_to_Entity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        entity.Add(123, target);
        Assert.True(entity.Has<int>(target));
        Assert.False(entity.Has<int>(new Entity(world.Tag, 9001, 1)));
    }
}
