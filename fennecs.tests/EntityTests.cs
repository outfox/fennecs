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
}