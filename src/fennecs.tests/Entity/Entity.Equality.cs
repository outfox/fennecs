// SPDX-License-Identifier: MIT

namespace fennecs.tests;

public class EntityEqualityTests
{
    [Fact]
    public void Entity_Is_Comparable()
    {
        using var world = new World();
        var entity1 = new Entity(1, 1, 1);
        var entity2 = new Entity(1, 2, 1);
        var entity3 = new Entity(1, 3, 1);

        Assert.True(entity1.CompareTo(entity2) < 0);
        Assert.True(entity2.CompareTo(entity3) < 0);
        Assert.True(entity1.CompareTo(entity3) < 0);
    }

    [Fact]
    public void Entity_Is_Equal_Same_Value_Same_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = new Entity(entity1.ToRaw());
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
        Assert.True(entity2 == entity1);
    }

    [Fact]
    public void Entity_Is_Distinct_Same_Index_Different_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity3 = new Entity((byte) (entity1.WorldTag + 1), entity1.Index, entity1.Generation);
        Assert.NotEqual(entity1, entity3);
        Assert.True(entity1 != entity3);
        Assert.True(entity3 != entity1);
    }

    [Fact]
    public void Entity_Is_Distinct_Different_Id_Same_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn();
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
        Assert.True(entity2 != entity1);
    }

    [Fact]
    public void Entity_Is_Distinct_Different_Id_Different_World()
    {
        using var world1 = new World();
        using var world2 = new World();
        var entity1 = world2.Spawn();
        var entity2 = new Entity(world1.Tag, 2, 1);
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
        Assert.True(entity2 != entity1);
    }

    [Fact]
    public void Entity_is_Equatable_to_Object()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = new Entity(entity1.ToRaw());
        Assert.True(entity1.Equals(entity2));
        Assert.True(entity1.Equals((object) entity2));
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.False(entity1.Equals("can't touch this"));
    }

    [Fact]
    public void Entity_Is_Hashable()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = world.Spawn();
        var entity3 = new Entity(entity1.ToRaw());
        var entity4 = new Entity(entity2.ToRaw());
        var set = new HashSet<Entity> {entity1, entity2, entity3, entity4};
        Assert.Equal(2, set.Count);
    }
}
