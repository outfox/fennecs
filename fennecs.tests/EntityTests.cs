namespace fennecs.tests;

public class EntityTests
{
    [Fact]
    public void Can_Relate_to_Entity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        var builder = new Entity(world, entity);
        builder.AddRelation<int>(target);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Identity(9001)));
    }

    [Fact]
    public void Can_Relate_to_Entity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn();
        var target = world.Spawn();
        var builder = new Entity(world, entity);
        builder.AddRelation(target, 123);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Identity(9001)));
    }

    [Fact]
    public void To_String()
    {
        using var world = new World();
        var entity = world.Spawn();
        var builder = new Entity(world, entity.Id);
        Assert.Equal(entity.ToString(), builder.ToString());
    }

    [Fact]
    public void Entity_Is_Comparable()
    {
        using var world = new World();
        var entity1 = new Entity(null!, new Identity(1));
        var entity2 = new Entity(null!, new Identity(2));
        var entity3 = new Entity(null!, new Identity(3));

        Assert.True(entity1.CompareTo(entity2) < 0);
        Assert.True(entity2.CompareTo(entity3) < 0);
        Assert.True(entity1.CompareTo(entity3) < 0);
    }

    [Fact]
    public void Entity_Is_Equal_Same_Id_Same_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = new Entity(world, entity1.Id);
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
        Assert.True(entity2 == entity1);
    }

    [Fact]
    public void Entity_Is_Distinct_Same_Id_Different_World()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity3 = new Entity(null!, entity1.Id);
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
        var entity2 = new Entity(null!, new Identity(2));
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
        Assert.True(entity2 != entity1);
    }

    [Fact]
    public void Entity_is_Equatable_to_Object()
    {
        using var world = new World();
        var entity1 = world.Spawn();
        var entity2 = new Entity(world, entity1.Id);
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
        var entity3 = new Entity(world, entity1.Id);
        var entity4 = new Entity(world, entity2.Id);
        var set = new HashSet<Entity> {entity1, entity2, entity3, entity4};
        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void Entity_Decays_to_Identity()
    {
        using var world = new World();
        var entity = world.Spawn();
        Identity identity = entity;
        Assert.Equal(entity.Id, identity);
    }


    [Fact]
    public void Entity_is_Disposable()
    {
        using var world = new World();
        var builder = world.Spawn();
        Assert.IsAssignableFrom<IDisposable>(builder);
        builder.Dispose();
    }
}