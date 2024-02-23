namespace fennecs.tests;

public class EntityBuilderTests
{
    [Fact]
    public void Can_Relate_to_Entity()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        var builder = new EntityBuilder(world, entity);
        builder.AddRelation<int>(target);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Entity(9001)));
    }

    [Fact]
    public void Can_Relate_to_Entity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var target = world.Spawn().Id();
        var builder = new EntityBuilder(world, entity);
        builder.AddRelation(target, 123);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Entity(9001)));
    }

    [Fact]
    public void Cannot_Relate_To_NonEntity()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var builder = new EntityBuilder(world, entity);
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Entity.Any); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Entity.None); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Entity.Object); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Entity.Relation); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Entity.Target); });

        var target = world.Spawn().Id();
        builder.AddRelation<int>(target);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Entity(9001)));
    }

    [Fact]
    public void Cannot_Relate_To_NonEntity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn().Id();
        var builder = new EntityBuilder(world, entity);
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Entity.Any, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Entity.None, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Entity.Object, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Entity.Relation, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Entity.Target, 123); });

        var target = world.Spawn().Id();
        builder.AddRelation(target, 123);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Entity(9001)));
    }
}