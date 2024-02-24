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
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Any); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Plain); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Object); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Entity); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Relation); });

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
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Any, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Plain, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Object, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Entity, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Relation, 123); });

        var target = world.Spawn().Id();
        builder.AddRelation(target, 123);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Entity(9001)));
    }
}