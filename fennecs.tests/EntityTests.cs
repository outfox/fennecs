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
    /*

    [Fact(Skip = "Paradigm changed.")]
    public void Cannot_Relate_To_NonEntity()
    {
        using var world = new World();
        var entity = world.Spawn();
        var builder = new Entity(world, entity);
        
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Any); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Plain); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Object); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Identity); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation<int>(Match.Relation); });
        
        var target = world.Spawn().Id;
        builder.AddRelation<int>(target);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Identity(9001)));
    }

    [Fact(Skip = "Paradigm changed.")]
    
    public void Cannot_Relate_To_NonEntity_with_Data()
    {
        using var world = new World();
        var entity = world.Spawn().Id;
        var builder = new Entity(world, entity);
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Any, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Plain, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Object, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Identity, 123); });
        Assert.Throws<InvalidOperationException>(() => { builder.AddRelation(Match.Relation, 123); });

        var target = world.Spawn().Id;
        builder.AddRelation(target, 123);
        Assert.True(world.HasRelation<int>(entity, target));
        Assert.False(world.HasRelation<int>(entity, new Identity(9001)));
    }
    */
}